using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities
{
    [Table("EMS_Attendances")]
    public class EmsAttendance
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public EmsAttendanceStatus Status { get; set; }

        [MaxLength(250)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual EmsEmployee? Employee { get; set; }
    }
}
