using EMS.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.API.Repositories
{
    public interface IAttendanceRepository
    {
        Task<(IEnumerable<AttendanceResponse> Data, int TotalEmployees, int PresentCount, int LeaveCount, int HalfDayCount)> GetAttendanceAsync(DateTime attendanceDate, string? shift, int? employeeId);
        Task<AttendanceResponse?> UpsertAttendanceAsync(AttendanceRequest request, int userId);
        Task<IEnumerable<AttendancePeriodSummaryDto>> GetAttendanceSummaryAsync(DateTime fromDate, DateTime toDate, string? shift);
        Task<IEnumerable<AttendanceEmployeeDetailRow>> GetEmployeeAttendanceDetailsAsync(int employeeId, DateTime fromDate, DateTime toDate);
        Task<AttendanceResponse?> SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest request, int userId);
    }
}
