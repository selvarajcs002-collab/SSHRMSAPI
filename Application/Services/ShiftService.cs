using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public ShiftService(
            IShiftRepository repository,
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public Task<List<ShiftTypeDto>> GetAllShiftTypes()
        {
            var types = new List<ShiftTypeDto>
            {
                new ShiftTypeDto { Id = (int)EmsShiftType.Morning, Name = "Morning Shift", Timing = "09:00 AM - 09:00 PM" },
                new ShiftTypeDto { Id = (int)EmsShiftType.Night, Name = "Night Shift", Timing = "09:00 PM - 09:00 AM" }
            };
            return Task.FromResult(types);
        }

        public async Task<ShiftAssignmentDto> AssignShift(ShiftAssignmentCreateDto dto)
        {
            var employee = await _employeeRepository.GetById(dto.EmployeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{dto.EmployeeId}' not found.");
            }

            var shift = _mapper.Map<EmsShift>(dto);
            shift.Id = Guid.NewGuid();
            shift.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows) ? TimeZoneInfo.FindSystemTimeZoneById("India Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")));

            var created = await _repository.Add(shift);
            created.Employee = employee; // populate navigation for mapping

            return _mapper.Map<ShiftAssignmentDto>(created);
        }

        public async Task<List<ShiftAssignmentDto>> GetAssignmentsByEmployee(Guid employeeId)
        {
            var list = await _repository.GetByEmployeeId(employeeId);
            return _mapper.Map<List<ShiftAssignmentDto>>(list.ToList());
        }

        public async Task<List<ShiftAssignmentDto>> GetAllAssignments()
        {
            var list = await _repository.GetAll();
            return _mapper.Map<List<ShiftAssignmentDto>>(list.ToList());
        }

        public async Task<ShiftAssignmentDto> UpdateAssignment(Guid id, ShiftAssignmentUpdateDto dto)
        {
            var assignment = await _repository.GetById(id);
            if (assignment == null)
            {
                throw new KeyNotFoundException($"Shift assignment with Id '{id}' not found.");
            }

            assignment.ShiftType = dto.ShiftType;
            assignment.MachineName = dto.MachineName;
            assignment.AssignmentDate = dto.AssignmentDate;

            var updated = await _repository.Update(assignment);
            
            // Re-fetch with employee details if needed
            var employee = await _employeeRepository.GetById(updated.EmployeeId);
            updated.Employee = employee;

            return _mapper.Map<ShiftAssignmentDto>(updated);
        }

        public async Task<bool> RemoveAssignment(Guid id)
        {
            return await _repository.Delete(id);
        }
    }
}
