using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EMS.Domain.Entities
{
    [Table("EMS_Payrolls")]
    public class EmsPayroll
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Incentives { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AdvancePayments { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPayable { get; set; }

        public bool IsPaid { get; set; }

        [MaxLength(500)]
        public string? PayslipFilePath { get; set; }

        public DateTime? PaidDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Property
        [ForeignKey("EmployeeId")]
        public virtual EmsEmployee? Employee { get; set; }
    }
}
