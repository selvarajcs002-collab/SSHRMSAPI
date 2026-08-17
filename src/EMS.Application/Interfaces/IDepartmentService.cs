using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<DepartmentDto> GetById(Guid id);
        Task<List<DepartmentDto>> GetAll();
        Task<DepartmentDto> Create(DepartmentCreateDto dto);
        Task<DepartmentDto> Update(Guid id, DepartmentCreateDto dto);
        Task<bool> Delete(Guid id);
    }
}
