namespace EMS.API.DTOs.Shift;

public class EmployeeShiftResponse
{
    public int? EmployeeShiftId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Designation { get; set; }
    public string? Shift { get; set; }
}
