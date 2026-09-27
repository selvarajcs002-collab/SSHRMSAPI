namespace EMS.API.DTOs.Salary;

public class CreateSalaryDetailsRequest
{
    public int EmployeeId { get; set; }
    public DateTime SalaryFromDate { get; set; }
    public DateTime SalaryToDate { get; set; }
    public decimal Incentive { get; set; }
    public decimal Advance { get; set; }
    public string? Remarks { get; set; }
}
