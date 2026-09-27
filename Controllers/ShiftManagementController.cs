using EMS.API.Common;
using EMS.API.DTOs.Shift;
using EMS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/shift-management")]
[Authorize] // Require JWT token for all endpoints
public class ShiftManagementController : ControllerBase
{
    private readonly IEmployeeShiftService _shiftService;

    public ShiftManagementController(IEmployeeShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> SaveEmployeeShift([FromBody] SaveEmployeeShiftRequest request)
    {
        try
        {
            // Extract UserID from claims in a real app
            int currentUserId = 1; 

            // If UpdatedBy/CreatedBy are not provided, default to current user
            if (request.EmployeeShiftId > 0 && !request.UpdatedBy.HasValue)
            {
                request.UpdatedBy = currentUserId;
            }
            if (request.EmployeeShiftId <= 0 && request.CreatedBy == 0)
            {
                request.CreatedBy = currentUserId;
            }

            var shiftId = await _shiftService.SaveEmployeeShiftAsync(request);
            return Ok(ApiResponse<int>.Ok(shiftId, "Employee shift saved successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<int>.Error(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeShiftResponse>>>> GetEmployeeShifts()
    {
        var shifts = await _shiftService.GetEmployeeShiftsAsync();
        return Ok(ApiResponse<IEnumerable<EmployeeShiftResponse>>.Ok(shifts, "Shifts retrieved successfully."));
    }

    [HttpGet("{employeeId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeShiftResponse>>>> GetEmployeeShiftByEmployeeId(int employeeId)
    {
        var shifts = await _shiftService.GetEmployeeShiftByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<IEnumerable<EmployeeShiftResponse>>.Ok(shifts, "Shifts retrieved successfully."));
    }
}
