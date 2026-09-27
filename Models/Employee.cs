namespace EMS.API.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? City { get; set; }
    public decimal PerDaySalary { get; set; }
    public int JoinedYear { get; set; }
    public bool IsActive { get; set; }
    
    // Additional fields mapped from stored procedure
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public int TotalCount { get; set; }
}

public class EmployeeDocument
{
    public int DocumentId { get; set; }
    public int EmployeeId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public DateTime UploadedDate { get; set; }
    public int? UploadedBy { get; set; }
    public bool IsActive { get; set; }
}
