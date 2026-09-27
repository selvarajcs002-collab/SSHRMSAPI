using EMS.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EMS.API.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceSummaryResponse> GetAttendanceAsync(DateTime? attendanceDate, string? shift, int? employeeId);
        Task<AttendanceSummaryResponse> SaveAttendanceAsync(BulkAttendanceRequest request, int userId);
        Task<AttendanceResponse> UpdateAttendanceAsync(int employeeId, AttendanceRequest request, int userId);
        Task<IEnumerable<AttendancePeriodSummaryDto>> GetAttendanceSummaryAsync(DateTime fromDate, DateTime toDate, string? shift);
        Task<AttendanceEmployeeDetailDto> GetEmployeeAttendanceDetailsAsync(int employeeId, DateTime fromDate, DateTime toDate);
        Task<AttendanceResponse> SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest request, int userId);
    }
}
