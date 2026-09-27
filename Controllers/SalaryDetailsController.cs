using EMS.API.Common;
using EMS.API.DTOs.Salary;
using EMS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/salary-details")]
public class SalaryDetailsController : ControllerBase
{
    private readonly ISalaryDetailsService _salaryDetailsService;

    public SalaryDetailsController(ISalaryDetailsService salaryDetailsService)
    {
        _salaryDetailsService = salaryDetailsService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalaryDetailsRequest request)
    {
        try
        {
            int createdBy = GetCurrentUserId();
            var response = await _salaryDetailsService.CreateSalaryDetailsAsync(request, createdBy);
            return Ok(ApiResponse<SalaryDetailsResponse>.Ok(response, "Salary details created successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPut("{employeeId:int}")]
    public async Task<IActionResult> Update(int employeeId, [FromBody] UpdateSalaryDetailsRequest request)
    {
        try
        {
            request.UpdatedBy = GetCurrentUserId();
            var response = await _salaryDetailsService.UpdateSalaryDetailsAsync(employeeId, request);
            return Ok(ApiResponse<SalaryDetailsResponse>.Ok(response, "Salary details updated successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        try
        {
            var response = await _salaryDetailsService.GetSalarySummaryAsync(fromDate, toDate);
            return Ok(ApiResponse<SalarySummaryResponse>.Ok(response, "Salary summary retrieved successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var response = await _salaryDetailsService.GetSalaryDetailsAsync(employeeId, fromDate, toDate);
            return Ok(ApiResponse<IEnumerable<SalaryDetailsResponse>>.Ok(response, "Salary details retrieved successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var response = await _salaryDetailsService.GetSalaryDetailsAsync(employeeId, fromDate, toDate);
            return Ok(ApiResponse<IEnumerable<SalaryDetailsResponse>>.Ok(response, "Salary details retrieved successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPost("{employeeId:int}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(int employeeId, [FromBody] MarkSalaryPaidRequest request)
    {
        try
        {
            int paidBy = GetCurrentUserId();
            var response = await _salaryDetailsService.MarkSalaryAsPaidAsync(employeeId, request, paidBy);
            return Ok(ApiResponse<SalaryDetailsResponse>.Ok(response, "Salary marked as paid successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Error(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Error(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Error(ex.Message));
        }
    }

    [HttpPost("{employeeId:int}/payslip")]
    public async Task<IActionResult> GeneratePayslip(int employeeId, [FromBody] GeneratePayslipRequest request)
    {
        var result = await _salaryDetailsService.GeneratePayslipAsync(employeeId, request);
        return File(result.Content, "application/pdf", result.FileName);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        // Default to 1 (e.g., Admin) if no user is found/authenticated for now
        return 1;
    }
}
