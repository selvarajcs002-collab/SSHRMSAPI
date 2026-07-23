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
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public AttendanceService(
            IAttendanceRepository repository,
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<AttendanceDto> MarkAttendance(AttendanceMarkDto dto)
        {
            var employee = await _employeeRepository.GetById(dto.EmployeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{dto.EmployeeId}' not found.");
            }

            var dailyLogs = await _repository.GetDailyAttendance(dto.Date);
            var existing = dailyLogs.FirstOrDefault(a => a.EmployeeId == dto.EmployeeId);

            if (existing != null)
            {
                existing.Status = dto.Status;
                existing.Remarks = dto.Remarks;
                existing.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                var updated = await _repository.Update(existing);
                updated.Employee = employee;
                return _mapper.Map<AttendanceDto>(updated);
            }
            else
            {
                var attendance = _mapper.Map<EmsAttendance>(dto);
                attendance.Id = Guid.NewGuid();
                attendance.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                attendance.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var created = await _repository.Add(attendance);
                created.Employee = employee;
                return _mapper.Map<AttendanceDto>(created);
            }
        }

        public async Task<List<AttendanceDto>> MarkBulkAttendance(AttendanceBulkMarkDto dto)
        {
            var results = new List<AttendanceDto>();
            foreach (var record in dto.Records)
            {
                var markDto = new AttendanceMarkDto
                {
                    EmployeeId = record.EmployeeId,
                    Date = dto.Date,
                    Status = record.Status,
                    Remarks = record.Remarks
                };

                try
                {
                    var marked = await MarkAttendance(markDto);
                    results.Add(marked);
                }
                catch (Exception)
                {
                    // Log or handle individual errors, but continue bulk process
                }
            }
            return results;
        }

        public async Task<AttendanceDailyDto> GetDailyAttendance(DateOnly date)
        {
            var logs = (await _repository.GetDailyAttendance(date)).ToList();
            var mappedLogs = _mapper.Map<List<AttendanceDto>>(logs);

            // Fetch all active employees to map missing logs as absent or unset
            var (employees, _) = await _employeeRepository.GetAll(1, 1000, null);
            var activeEmployees = employees.Where(e => e.IsConfirmed && e.Status == EmsEmployeeStatus.Active).ToList();

            var fullRecords = new List<AttendanceDto>();
            foreach (var emp in activeEmployees)
            {
                var log = mappedLogs.FirstOrDefault(l => l.EmployeeId == emp.Id);
                if (log != null)
                {
                    fullRecords.Add(log);
                }
                else
                {
                    fullRecords.Add(new AttendanceDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        EmployeeCode = emp.EmployeeCode,
                        Date = date,
                        Status = EmsAttendanceStatus.Absent, // default to absent if not marked
                        Remarks = "Not marked (auto-absent)",
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                        UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    });
                }
            }

            return new AttendanceDailyDto
            {
                Date = date,
                TotalPresent = fullRecords.Count(r => r.Status == EmsAttendanceStatus.Present),
                TotalAbsent = fullRecords.Count(r => r.Status == EmsAttendanceStatus.Absent),
                Records = fullRecords
            };
        }

        public async Task<List<AttendanceDto>> GetEmployeeMonthlyAttendance(Guid employeeId, int month, int year)
        {
            var logs = await _repository.GetEmployeeMonthlyAttendance(employeeId, month, year);
            return _mapper.Map<List<AttendanceDto>>(logs.ToList());
        }

        public async Task<List<AttendanceSummaryDto>> GetMonthlySummary(int month, int year)
        {
            var (employees, _) = await _employeeRepository.GetAll(1, 1000, null);
            var activeEmployees = employees.Where(e => e.IsConfirmed).ToList();
            
            var summaryList = new List<AttendanceSummaryDto>();
            foreach (var emp in activeEmployees)
            {
                var logs = (await _repository.GetEmployeeMonthlyAttendance(emp.Id, month, year)).ToList();
                summaryList.Add(new AttendanceSummaryDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    EmployeeCode = emp.EmployeeCode,
                    PresentCount = logs.Count(l => l.Status == EmsAttendanceStatus.Present),
                    AbsentCount = logs.Count(l => l.Status == EmsAttendanceStatus.Absent)
                });
            }

            return summaryList;
        }

        public async Task<AttendanceDto> UpdateAttendance(Guid id, AttendanceUpdateDto dto)
        {
            var attendance = await _repository.GetById(id);
            if (attendance == null)
            {
                throw new KeyNotFoundException($"Attendance record with Id '{id}' not found.");
            }

            attendance.Status = dto.Status;
            attendance.Remarks = dto.Remarks;
            attendance.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            var updated = await _repository.Update(attendance);
            var employee = await _employeeRepository.GetById(updated.EmployeeId);
            updated.Employee = employee;

            return _mapper.Map<AttendanceDto>(updated);
        }

        public async Task<List<AttendanceDto>> GetAttendanceByDateRange(DateOnly startDate, DateOnly endDate)
        {
            var logs = await _repository.GetAttendanceByDateRangeAsync(startDate, endDate);
            return _mapper.Map<List<AttendanceDto>>(logs.ToList());
        }
    }
}
