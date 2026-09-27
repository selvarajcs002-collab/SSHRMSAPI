using EMS.API.DTOs.Shift;

namespace EMS.API.Repositories.Interfaces;

public interface IEmployeeShiftRepository
{
    Task<int> SaveEmployeeShiftAsync(SaveEmployeeShiftRequest request);
    Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftsAsync();
    Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftByEmployeeIdAsync(int employeeId);
}
