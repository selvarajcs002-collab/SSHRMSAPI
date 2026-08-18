using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Interfaces
{
    public interface ILeaveRepository
    {
        Task<EmsLeave?> GetById(Guid id);
        Task<IEnumerable<EmsLeave>> GetAll();
        Task<IEnumerable<EmsLeave>> GetByEmployeeId(Guid employeeId);
        Task<EmsLeave> Add(EmsLeave leave);
        Task<EmsLeave> UpdateStatus(Guid id, EmsLeaveStatus status);
    }
}
