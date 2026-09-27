namespace EMS.API.DTOs.Salary;

public class GeneratePayslipRequest
{
    public int EmployeeId { get; set; }
    public string? SalaryMonth { get; set; }
    public DateTime SalaryFromDate { get; set; }
    public DateTime SalaryToDate { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? Designation { get; set; }
    public decimal PerDaySalary { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public decimal Incentive { get; set; }
    public decimal Advance { get; set; }
    public string? Remarks { get; set; }
}
