namespace EMS.API.DTOs.Employee;

public class EmployeeCreateRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? City { get; set; }
    public decimal PerDaySalary { get; set; }
    public int JoinedYear { get; set; }
}

public class EmployeeUpdateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? City { get; set; }
    public decimal PerDaySalary { get; set; }
    public int JoinedYear { get; set; }
}

public class EmployeeDuplicateRequest
{
    public string NewEmployeeCode { get; set; } = string.Empty;
}

public class ShiftManagementRequest
{
    public int? EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string? City { get; set; }
    public decimal PerDaySalary { get; set; }
    public int JoinedYear { get; set; }
    public string Shift { get; set; } = string.Empty;
}

public class DeleteEmployeeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public int? DeletedRows { get; set; }
    public string? Error { get; set; }
}
