using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> MarkAttendance(AttendanceMarkDto dto);
        Task<List<AttendanceDto>> MarkBulkAttendance(AttendanceBulkMarkDto dto);
        Task<AttendanceDailyDto> GetDailyAttendance(DateOnly date);
        Task<List<AttendanceDto>> GetEmployeeMonthlyAttendance(Guid employeeId, int month, int year);
        Task<List<AttendanceSummaryDto>> GetMonthlySummary(int month, int year);
        Task<AttendanceDto> UpdateAttendance(Guid id, AttendanceUpdateDto dto);
        Task<List<AttendanceDto>> GetAttendanceByDateRange(DateOnly startDate, DateOnly endDate);
    }
}
