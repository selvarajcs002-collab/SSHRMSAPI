using System;
using System.Collections.Generic;
using EMS.Domain.Enums;

namespace EMS.Application.DTOs
{
    public class AttendanceMarkDto
    {
        public Guid EmployeeId { get; set; }
        public DateOnly Date { get; set; }
        public EmsAttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class EmployeeAttendanceRecordDto
    {
        public Guid EmployeeId { get; set; }
        public EmsAttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceBulkMarkDto
    {
        public DateOnly Date { get; set; }
        public List<EmployeeAttendanceRecordDto> Records { get; set; } = new List<EmployeeAttendanceRecordDto>();
    }

    public class AttendanceUpdateDto
    {
        public EmsAttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public EmsAttendanceStatus Status { get; set; }
        public string? Remarks { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AttendanceDailyDto
    {
        public DateOnly Date { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public List<AttendanceDto> Records { get; set; } = new List<AttendanceDto>();
    }

    public class AttendanceSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalDays => PresentCount + AbsentCount;
    }
}
