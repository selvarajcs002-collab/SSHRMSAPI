using EMS.API.DTOs;
using EMS.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendance(
            [FromQuery] DateTime? attendanceDate,
            [FromQuery] string? shift,
            [FromQuery] int? employeeId)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceAsync(attendanceDate, shift, employeeId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving attendance.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendance([FromBody] BulkAttendanceRequest request)
        {
            try
            {
                // In a real application, you would extract the UserId from the authenticated user's claims.
                int userId = 1; // Defaulting to 1 for this example

                var result = await _attendanceService.SaveAttendanceAsync(request, userId);
                return Ok(new { success = true, message = "Attendance saved successfully.", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while saving attendance.", error = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetAttendanceSummary(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string? shift)
        {
            try
            {
                if (!fromDate.HasValue || !toDate.HasValue)
                    return BadRequest(new { success = false, message = "FromDate and ToDate are required." });

                var result = await _attendanceService.GetAttendanceSummaryAsync(fromDate.Value, toDate.Value, shift);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving attendance summary.", error = ex.Message });
            }
        }

        [HttpGet("employee/{employeeId:int}/details")]
        public async Task<IActionResult> GetEmployeeAttendanceDetails(
            int employeeId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                if (!fromDate.HasValue || !toDate.HasValue)
                    return BadRequest(new { success = false, message = "FromDate and ToDate are required." });

                var result = await _attendanceService.GetEmployeeAttendanceDetailsAsync(employeeId, fromDate.Value, toDate.Value);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving attendance details.", error = ex.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveEmployeeAttendance([FromBody] SaveEmployeeAttendanceRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _attendanceService.SaveEmployeeAttendanceAsync(request, userId);
                return Ok(new { success = true, message = "Attendance saved successfully.", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while saving attendance.", error = ex.Message });
            }
        }

        [HttpPut("{employeeId}")]
        public async Task<IActionResult> UpdateAttendance(int employeeId, [FromBody] AttendanceRequest request)
        {
            try
            {
                int userId = 1; // Replace with authenticated user ID
                var result = await _attendanceService.UpdateAttendanceAsync(employeeId, request, userId);
                return Ok(new { success = true, message = "Attendance updated successfully.", data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating attendance.", error = ex.Message });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 1;
        }
    }
}
