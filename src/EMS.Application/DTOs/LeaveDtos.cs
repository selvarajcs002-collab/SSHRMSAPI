using System;
using EMS.Domain.Enums;

namespace EMS.Application.DTOs
{
    public class LeaveApplyDto
    {
        public Guid EmployeeId { get; set; }
        public EmsLeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
    }

    public class LeaveUpdateStatusDto
    {
        public EmsLeaveStatus Status { get; set; }
    }

    public class LeaveDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public EmsLeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public EmsLeaveStatus Status { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
