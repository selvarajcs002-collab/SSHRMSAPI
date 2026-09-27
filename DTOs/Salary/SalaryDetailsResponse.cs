namespace EMS.API.DTOs.Salary;

public class SalaryDetailsResponse
{
    public int EmployeeSalaryId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal PerDaySalary { get; set; }
    public int PresentDays { get; set; }
    public int LeaveDays { get; set; }
    public int HalfDays { get; set; }
    public decimal PresentSalary { get; set; }
    public decimal HalfDaySalary { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal Incentive { get; set; }
    public decimal Advance { get; set; }
    public decimal Salary { get; set; }
    public string? Remarks { get; set; }
    public DateTime SalaryFromDate { get; set; }
    public DateTime SalaryToDate { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public DateTime? PaidDate { get; set; }
    public int? PaidBy { get; set; }
    public string? VoucherNo { get; set; }
}
