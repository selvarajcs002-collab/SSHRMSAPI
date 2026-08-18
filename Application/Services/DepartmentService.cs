using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;

namespace EMS.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;
        private readonly IMapper _mapper;

        public DepartmentService(IDepartmentRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<DepartmentDto> GetById(Guid id)
        {
            var dept = await _repository.GetById(id);
            if (dept == null)
            {
                throw new KeyNotFoundException($"Department with Id '{id}' not found.");
            }
            return _mapper.Map<DepartmentDto>(dept);
        }

        public async Task<List<DepartmentDto>> GetAll()
        {
            var depts = await _repository.GetAll();
            return _mapper.Map<List<DepartmentDto>>(depts);
        }

        public async Task<DepartmentDto> Create(DepartmentCreateDto dto)
        {
            var dept = _mapper.Map<EmsDepartment>(dto);
            dept.Id = Guid.NewGuid();
            dept.CreatedAt = DateTime.UtcNow;

            var created = await _repository.Upsert(dept);
            return _mapper.Map<DepartmentDto>(created);
        }

        public async Task<DepartmentDto> Update(Guid id, DepartmentCreateDto dto)
        {
            var existing = await _repository.GetById(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Department with Id '{id}' not found.");
            }

            _mapper.Map(dto, existing);
            var updated = await _repository.Upsert(existing);
            return _mapper.Map<DepartmentDto>(updated);
        }

        public async Task<bool> Delete(Guid id)
        {
            return await _repository.Delete(id);
        }
    }
}
