using System;
using System.Collections.Generic;
using EMS.Domain.Enums;

namespace EMS.Application.DTOs
{
    public class EmployeeBasicDetailCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? AadhaarNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? District { get; set; }
        public string? Pincode { get; set; }
        public string? PanNumber { get; set; }
        public string? BloodGroup { get; set; }
        public string? MaritalStatus { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Nationality { get; set; }
        public decimal PerDaySalary { get; set; }
        public string? Referral { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }
        public string? DepartmentName { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class EmployeeBasicDetailDto : EmployeeBasicDetailCreateDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
        public EmsEmployeeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeBankDetailCreateDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? UpiId { get; set; }
    }

    public class EmployeeBankDetailDto : EmployeeBankDetailCreateDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EmployeeDocumentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public EmsDocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class EmployeePreviewDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public EmployeeBasicDetailDto? BasicDetails { get; set; }
        public EmployeeBankDetailDto? BankDetails { get; set; }
        public List<EmployeeDocumentDto> Documents { get; set; } = new List<EmployeeDocumentDto>();
    }

    public class EmployeeListDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public EmsEmployeeStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal PerDaySalary { get; set; }
        public string? Email { get; set; }
        public string? Designation { get; set; }
        public string? DepartmentName { get; set; }
        public string? ProfilePicture { get; set; }
    }

    public class EmsPagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
