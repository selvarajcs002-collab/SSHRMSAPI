using EMS.API.DTOs.Shift;

namespace EMS.API.Services.Interfaces;

public interface IEmployeeShiftService
{
    Task<int> SaveEmployeeShiftAsync(SaveEmployeeShiftRequest request);
    Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftsAsync();
    Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftByEmployeeIdAsync(int employeeId);
}
