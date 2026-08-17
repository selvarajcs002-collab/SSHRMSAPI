using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities
{
    [Table("EMS_Leaves")]
    public class EmsLeave
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public EmsLeaveType LeaveType { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        public EmsLeaveStatus Status { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual EmsEmployee? Employee { get; set; }
    }
}
