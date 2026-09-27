namespace EMS.API.DTOs.Shift;

public class SaveEmployeeShiftRequest
{
    public int? EmployeeShiftId { get; set; }
    public int EmployeeId { get; set; }
    public string Shift { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
