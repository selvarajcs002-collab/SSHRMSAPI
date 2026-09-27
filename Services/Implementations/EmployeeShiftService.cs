using EMS.API.DTOs.Shift;
using EMS.API.Repositories.Interfaces;
using EMS.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EMS.API.Services.Implementations;

public class EmployeeShiftService : IEmployeeShiftService
{
    private readonly IEmployeeShiftRepository _shiftRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeShiftService> _logger;

    public EmployeeShiftService(
        IEmployeeShiftRepository shiftRepository,
        IEmployeeRepository employeeRepository,
        ILogger<EmployeeShiftService> logger)
    {
        _shiftRepository = shiftRepository;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<int> SaveEmployeeShiftAsync(SaveEmployeeShiftRequest request)
    {
        _logger.LogInformation("Employee shift save started for EmployeeId: {EmployeeId}", request.EmployeeId);

        if (request.EmployeeId <= 0)
        {
            throw new ArgumentException("EmployeeId must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(request.Shift))
        {
            throw new ArgumentException("Shift is required.");
        }

        if (request.EffectiveFrom == default)
        {
            throw new ArgumentException("EffectiveFrom is required.");
        }

        if (request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom)
        {
            throw new ArgumentException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee == null)
        {
            _logger.LogWarning("Employee does not exist: {EmployeeId}", request.EmployeeId);
            throw new ArgumentException("Employee does not exist.");
        }

        var shiftId = await _shiftRepository.SaveEmployeeShiftAsync(request);

        _logger.LogInformation("Employee shift saved successfully. ShiftId: {ShiftId}", shiftId);

        return shiftId;
    }

    public async Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftsAsync()
    {
        _logger.LogInformation("Employee shift retrieval started.");
        return await _shiftRepository.GetEmployeeShiftsAsync();
    }

    public async Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftByEmployeeIdAsync(int employeeId)
    {
        _logger.LogInformation("Employee shift retrieval started for EmployeeId: {EmployeeId}", employeeId);
        return await _shiftRepository.GetEmployeeShiftByEmployeeIdAsync(employeeId);
    }
}
