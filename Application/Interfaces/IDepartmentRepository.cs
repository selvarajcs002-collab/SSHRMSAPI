using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<EmsDepartment?> GetById(Guid id);
        Task<IEnumerable<EmsDepartment>> GetAll();
        Task<EmsDepartment> Upsert(EmsDepartment department);
        Task<bool> Delete(Guid id);
    }
}
