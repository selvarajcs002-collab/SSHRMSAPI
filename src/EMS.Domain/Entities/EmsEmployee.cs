using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities
{
    [Table("EMS_Employees")]
    public class EmsEmployee
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string AadhaarNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Pincode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PanNumber { get; set; }

        [MaxLength(5)]
        public string? BloodGroup { get; set; }

        [MaxLength(20)]
        public string? MaritalStatus { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(50)]
        public string? Nationality { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PerDaySalary { get; set; }

        [MaxLength(100)]
        public string? Referral { get; set; }

        public bool IsConfirmed { get; set; }

        public EmsEmployeeStatus Status { get; set; }
        
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        [MaxLength(100)]
        public string? DepartmentName { get; set; }

        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation Properties
        public virtual EmsEmployeeBankDetail? BankDetail { get; set; }
        public virtual ICollection<EmsEmployeeDocument> Documents { get; set; } = new List<EmsEmployeeDocument>();
        public virtual ICollection<EmsShift> Shifts { get; set; } = new List<EmsShift>();
        public virtual ICollection<EmsAttendance> Attendances { get; set; } = new List<EmsAttendance>();
        public virtual ICollection<EmsPayroll> Payrolls { get; set; } = new List<EmsPayroll>();
        public virtual ICollection<EmsLeave> Leaves { get; set; } = new List<EmsLeave>();
    }
}
