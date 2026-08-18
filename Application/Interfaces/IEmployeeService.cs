using System;
using System.IO;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Domain.Enums;

namespace EMS.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeBasicDetailDto> CreateBasicDetails(EmployeeBasicDetailCreateDto dto);
        Task<EmployeeBankDetailDto> SaveBankDetails(Guid employeeId, EmployeeBankDetailCreateDto dto);
        Task<EmployeeDocumentDto> UploadDocument(Guid employeeId, EmsDocumentType documentType, string fileName, string contentType, Stream fileStream);
        Task<EmployeePreviewDto> GetPreview(Guid employeeId);
        Task<EmployeePreviewDto> ConfirmRegistration(Guid employeeId);
        Task<EmsPagedResponse<EmployeeListDto>> GetAllEmployees(int pageNumber, int pageSize, string? searchTerm);
        Task<EmployeePreviewDto> GetEmployeeById(Guid id);
        Task<EmployeeBasicDetailDto> UpdateEmployee(Guid id, EmployeeBasicDetailCreateDto dto);
        Task<bool> DeleteEmployee(Guid id);
        Task<bool> HardDeleteEmployee(Guid id);
        Task<(byte[] FileContents, string ContentType, string FileName)> GetDocument(Guid employeeId, EmsDocumentType documentType);
        Task<bool> DeleteDocument(Guid employeeId, EmsDocumentType documentType);
    }
}
