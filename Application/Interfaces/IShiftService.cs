using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IShiftService
    {
        Task<List<ShiftTypeDto>> GetAllShiftTypes();
        Task<ShiftAssignmentDto> AssignShift(ShiftAssignmentCreateDto dto);
        Task<List<ShiftAssignmentDto>> GetAssignmentsByEmployee(Guid employeeId);
        Task<List<ShiftAssignmentDto>> GetAllAssignments();
        Task<ShiftAssignmentDto> UpdateAssignment(Guid id, ShiftAssignmentUpdateDto dto);
        Task<bool> RemoveAssignment(Guid id);
    }
}
