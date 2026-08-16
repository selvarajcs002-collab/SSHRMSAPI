using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.Common.Helpers;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public EmployeeService(
            IEmployeeRepository repository,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<EmployeeBasicDetailDto> CreateBasicDetails(EmployeeBasicDetailCreateDto dto)
        {
            var existing = await _repository.GetByAadhaarNumber(dto.AadhaarNumber);
            if (existing != null)
            {
                throw new InvalidOperationException($"An employee with Aadhaar number '{dto.AadhaarNumber}' already exists.");
            }

            var employee = _mapper.Map<EmsEmployee>(dto);
            employee.Id = Guid.NewGuid();
            employee.IsConfirmed = false;
            employee.Status = EmsEmployeeStatus.Inactive;
            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.Add(employee);
            return _mapper.Map<EmployeeBasicDetailDto>(created);
        }

        public async Task<EmployeeBankDetailDto> SaveBankDetails(Guid employeeId, EmployeeBankDetailCreateDto dto)
        {
            var employee = await _repository.GetWithFullDetails(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            if (employee.BankDetail != null)
            {
                _mapper.Map(dto, employee.BankDetail);
                employee.BankDetail.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var bankDetail = _mapper.Map<EmsEmployeeBankDetail>(dto);
                bankDetail.Id = Guid.NewGuid();
                bankDetail.EmployeeId = employeeId;
                bankDetail.CreatedAt = DateTime.UtcNow;
                bankDetail.UpdatedAt = DateTime.UtcNow;
                employee.BankDetail = bankDetail;
            }

            employee.UpdatedAt = DateTime.UtcNow;
            await _repository.Update(employee);

            return _mapper.Map<EmployeeBankDetailDto>(employee.BankDetail);
        }

        public async Task<EmployeeDocumentDto> UploadDocument(Guid employeeId, EmsDocumentType documentType, string fileName, string contentType, Stream fileStream)
        {
            var employee = await _repository.GetById(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            // Save to physical storage
            var folder = Path.Combine("employees", employeeId.ToString());
            var filePath = await _fileStorageService.SaveFile(folder, fileName, fileStream);

            // Record document in employee collection
            var document = new EmsEmployeeDocument
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                DocumentType = documentType,
                FileName = fileName,
                FilePath = filePath,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            };

            employee.Documents.Add(document);
            employee.UpdatedAt = DateTime.UtcNow;
            await _repository.Update(employee);

            return _mapper.Map<EmployeeDocumentDto>(document);
        }

        public async Task<EmployeePreviewDto> GetPreview(Guid employeeId)
        {
            var employee = await _repository.GetWithFullDetails(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            return MapToPreview(employee);
        }

        public async Task<EmployeePreviewDto> ConfirmRegistration(Guid employeeId)
        {
            var employee = await _repository.GetWithFullDetails(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            if (employee.IsConfirmed)
            {
                throw new InvalidOperationException("Registration is already confirmed.");
            }



            // Generate employee code and confirm
            var seq = await _repository.GetNextSequenceNumber();
            employee.EmployeeCode = EmsCodeGenerator.GenerateEmployeeCode(seq);
            employee.IsConfirmed = true;
            employee.Status = EmsEmployeeStatus.Active;
            employee.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(employee);
            return MapToPreview(employee);
        }

        public async Task<EmsPagedResponse<EmployeeListDto>> GetAllEmployees(int pageNumber, int pageSize, string? searchTerm)
        {
            var (items, total) = await _repository.GetAll(pageNumber, pageSize, searchTerm);

            var list = items.Select(e => new EmployeeListDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullName = $"{e.FirstName} {e.LastName}".Trim(),
                PhoneNumber = e.PhoneNumber,
                City = e.City,
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                PerDaySalary = e.PerDaySalary,
                Email = e.Email,
                Designation = e.Designation,
                DepartmentName = e.DepartmentName,
                ProfilePicture = e.ProfilePicture
            }).ToList();

            return new EmsPagedResponse<EmployeeListDto>
            {
                Items = list,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<EmployeePreviewDto> GetEmployeeById(Guid id)
        {
            var employee = await _repository.GetWithFullDetails(id);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{id}' not found.");
            }
            return MapToPreview(employee);
        }

        public async Task<EmployeeBasicDetailDto> UpdateEmployee(Guid id, EmployeeBasicDetailCreateDto dto)
        {
            var employee = await _repository.GetById(id);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{id}' not found.");
            }

            // Snapshot immutable identity fields
            var originalAadhaar = employee.AadhaarNumber;
            var originalPan = employee.PanNumber;
            var originalDob = employee.DateOfBirth;

            _mapper.Map(dto, employee);
            
            // Re-apply immutable identity fields to prevent tampering
            employee.AadhaarNumber = originalAadhaar;
            employee.PanNumber = originalPan;
            employee.DateOfBirth = originalDob;
            employee.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.Update(employee);
            return _mapper.Map<EmployeeBasicDetailDto>(updated);
        }

        public async Task<bool> DeleteEmployee(Guid id)
        {
            var deleted = await _repository.SoftDelete(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Employee with Id '{id}' not found.");
            }
            return true;
        }

        public async Task<bool> HardDeleteEmployee(Guid id)
        {
            var deleted = await _repository.HardDelete(id);
            if (!deleted)
            {
                throw new KeyNotFoundException($"Employee with Id '{id}' not found.");
            }
            return true;
        }

        private EmployeePreviewDto MapToPreview(EmsEmployee employee)
        {
            return new EmployeePreviewDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                IsConfirmed = employee.IsConfirmed,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt,
                BasicDetails = _mapper.Map<EmployeeBasicDetailDto>(employee),
                BankDetails = employee.BankDetail != null ? _mapper.Map<EmployeeBankDetailDto>(employee.BankDetail) : null,
                Documents = employee.Documents.Select(d => _mapper.Map<EmployeeDocumentDto>(d)).ToList()
            };
        }

        public async Task<(byte[] FileContents, string ContentType, string FileName)> GetDocument(Guid employeeId, EmsDocumentType documentType)
        {
            var employee = await _repository.GetWithFullDetails(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            var doc = employee.Documents.FirstOrDefault(d => d.DocumentType == documentType);
            if (doc == null)
            {
                throw new FileNotFoundException($"Document of type '{documentType}' not found for employee.");
            }

            var bytes = await _fileStorageService.GetFile(doc.FilePath);
            return (bytes, doc.ContentType, doc.FileName);
        }

        public async Task<bool> DeleteDocument(Guid employeeId, EmsDocumentType documentType)
        {
            var employee = await _repository.GetWithFullDetails(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            var doc = employee.Documents.FirstOrDefault(d => d.DocumentType == documentType);
            if (doc == null)
            {
                return false;
            }

            // Delete physically
            await _fileStorageService.DeleteFile(doc.FilePath);

            // Remove from DB via repository
            var deleted = await _repository.DeleteDocument(doc.Id);
            
            // Reload/Update employee state
            await _repository.Update(employee);
            return deleted;
        }
    }
}
