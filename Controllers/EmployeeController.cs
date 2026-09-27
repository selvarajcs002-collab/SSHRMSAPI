using EMS.API.Common;
using EMS.API.DTOs.Employee;
using EMS.API.Models;
using EMS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require JWT token for all endpoints
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<Employee>>> GetList([FromQuery] PaginationRequest request)
    {
        var response = await _employeeService.GetListAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound(ApiResponse<object>.Error("Employee not found"));
        return Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] EmployeeCreateRequest request)
    {
        // Extract UserID from claims in a real app: int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        int? currentUserId = 1; 

        var response = await _employeeService.CreateAsync(request, currentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(int id, [FromBody] EmployeeUpdateRequest request)
    {
        int? currentUserId = 1;
        var response = await _employeeService.UpdateAsync(id, request, currentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> DeleteEmployee(int employeeId)
    {
        if (employeeId <= 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid employee ID.",
                employeeId = employeeId
            });
        }

        int currentUserId = GetCurrentUserId();
        var response = await _employeeService.DeleteEmployeeAsync(employeeId, currentUserId);

        if (!response.Success)
        {
            if (response.Message != null && response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new
                {
                    success = false,
                    message = "Employee not found.",
                    employeeId = employeeId
                });
            }

            return StatusCode(500, new
            {
                success = false,
                message = "Unable to delete employee.",
                error = response.Error ?? response.Message
            });
        }

        return Ok(new
        {
            success = true,
            message = "Employee and all related data deleted successfully.",
            employeeId = response.EmployeeId
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return 1;
    }

    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<ApiResponse<int>>> Duplicate(int id, [FromBody] EmployeeDuplicateRequest request)
    {
        int? currentUserId = 1;
        var response = await _employeeService.DuplicateAsync(id, request, currentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{id}/document")]
    public async Task<ActionResult<EmployeeDocument>> GetDocument(int id)
    {
        var document = await _employeeService.GetDocumentAsync(id);
        if (document == null) return NotFound(ApiResponse<object>.Error("Document not found"));
        return Ok(document);
    }

    [HttpPost("shiftmanagement")]
    public async Task<ActionResult<ApiResponse<bool>>> ShiftManagement([FromBody] ShiftManagementRequest request)
    {
        int? currentUserId = 1;
        var response = await _employeeService.ShiftManagementAsync(request, currentUserId);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
