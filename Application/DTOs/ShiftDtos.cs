using System;
using EMS.Domain.Enums;

namespace EMS.Application.DTOs
{
    public class ShiftTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Timing { get; set; } = string.Empty;
    }

    public class ShiftAssignmentCreateDto
    {
        public Guid EmployeeId { get; set; }
        public EmsShiftType ShiftType { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
    }

    public class ShiftAssignmentUpdateDto
    {
        public EmsShiftType ShiftType { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
    }

    public class ShiftAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public EmsShiftType ShiftType { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public DateTime AssignmentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
