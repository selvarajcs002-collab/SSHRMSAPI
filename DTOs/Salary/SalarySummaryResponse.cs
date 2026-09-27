namespace EMS.API.DTOs.Salary;

public class SalarySummaryResponse
{
    public int TotalEmployees { get; set; }
    public decimal TotalPayment { get; set; }
    public int CompletedPaid { get; set; }
    public int Remaining { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
}
