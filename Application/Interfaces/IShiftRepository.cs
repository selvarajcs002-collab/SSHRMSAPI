using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IShiftRepository
    {
        Task<EmsShift?> GetById(Guid id);
        Task<IEnumerable<EmsShift>> GetAll();
        Task<IEnumerable<EmsShift>> GetByEmployeeId(Guid employeeId);
        Task<EmsShift> Add(EmsShift shift);
        Task<EmsShift> Update(EmsShift shift);
        Task<bool> Delete(Guid id);
    }
}
