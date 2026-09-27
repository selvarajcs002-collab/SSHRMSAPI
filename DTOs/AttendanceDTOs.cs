using System;
using System.Collections.Generic;

namespace EMS.API.DTOs
{
    public class AttendanceRequest
    {
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class BulkAttendanceRequest
    {
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = string.Empty;
        public List<AttendanceRequest> Attendance { get; set; } = new List<AttendanceRequest>();
    }

    public class AttendanceResponse
    {
        public int? AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceSummaryResponse
    {
        public DateTime AttendanceDate { get; set; }
        public string Shift { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int PresentCount { get; set; }
        public int LeaveCount { get; set; }
        public int HalfDayCount { get; set; }
        public List<AttendanceResponse> Employees { get; set; } = new List<AttendanceResponse>();
    }

    public class AttendancePeriodSummaryDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int HalfDays { get; set; }
        public int NotMarkedDays { get; set; }
    }

    public class SaveEmployeeAttendanceRequest
    {
        public int EmployeeId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AttendanceDateStatusDto
    {
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AttendanceEmployeeDetailRow
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class AttendanceEmployeeDetailDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Shift { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int HalfDays { get; set; }
        public int NotMarkedDays { get; set; }
        public List<DateTime> AbsentDates { get; set; } = new();
        public List<DateTime> HalfDayDates { get; set; } = new();
        public List<DateTime> NotMarkedDates { get; set; } = new();
        public List<AttendanceDateStatusDto> AllDates { get; set; } = new();
    }
}
