namespace EMS.API.DTOs.Salary;

public class SalaryPayslipData
{
    public int EmployeeSalaryId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal PerDaySalary { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public decimal TotalSalary { get; set; }
    public decimal Incentive { get; set; }
    public decimal Advance { get; set; }
    public decimal NetSalary { get; set; }
    public string? Remarks { get; set; }
    public DateTime SalaryFromDate { get; set; }
    public DateTime SalaryToDate { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
}
