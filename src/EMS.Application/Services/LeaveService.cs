using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public LeaveService(ILeaveRepository repository, IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<LeaveDto> GetById(Guid id)
        {
            var leave = await _repository.GetById(id);
            if (leave == null)
            {
                throw new KeyNotFoundException($"Leave application with Id '{id}' not found.");
            }
            return _mapper.Map<LeaveDto>(leave);
        }

        public async Task<List<LeaveDto>> GetAll()
        {
            var leaves = await _repository.GetAll();
            return _mapper.Map<List<LeaveDto>>(leaves);
        }

        public async Task<List<LeaveDto>> GetByEmployeeId(Guid employeeId)
        {
            var leaves = await _repository.GetByEmployeeId(employeeId);
            return _mapper.Map<List<LeaveDto>>(leaves);
        }

        public async Task<LeaveDto> ApplyLeave(LeaveApplyDto dto)
        {
            var employee = await _employeeRepository.GetById(dto.EmployeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{dto.EmployeeId}' not found.");
            }

            var leave = _mapper.Map<EmsLeave>(dto);
            leave.Id = Guid.NewGuid();
            leave.Status = EmsLeaveStatus.Pending;
            leave.CreatedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            var created = await _repository.Add(leave);
            return _mapper.Map<LeaveDto>(created);
        }

        public async Task<LeaveDto> ApproveLeave(Guid id)
        {
            var updated = await _repository.UpdateStatus(id, EmsLeaveStatus.Approved);
            return _mapper.Map<LeaveDto>(updated);
        }

        public async Task<LeaveDto> RejectLeave(Guid id)
        {
            var updated = await _repository.UpdateStatus(id, EmsLeaveStatus.Rejected);
            return _mapper.Map<LeaveDto>(updated);
        }
    }
}
