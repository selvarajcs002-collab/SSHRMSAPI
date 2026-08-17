using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Domain.Enums;

namespace EMS.Application.Interfaces
{
    public interface ILeaveService
    {
        Task<LeaveDto> GetById(Guid id);
        Task<List<LeaveDto>> GetAll();
        Task<List<LeaveDto>> GetByEmployeeId(Guid employeeId);
        Task<LeaveDto> ApplyLeave(LeaveApplyDto dto);
        Task<LeaveDto> ApproveLeave(Guid id);
        Task<LeaveDto> RejectLeave(Guid id);
    }
}
