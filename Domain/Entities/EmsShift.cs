using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities
{
    [Table("EMS_Shifts")]
    public class EmsShift
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public EmsShiftType ShiftType { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("MachineAllocation")]
        public string MachineName { get; set; } = string.Empty;

        [Required]
        public DateTime AssignmentDate { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual EmsEmployee? Employee { get; set; }
    }
}
