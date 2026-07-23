using System;
using System.Linq;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("mark")]
        public async Task<IActionResult> MarkAttendance([FromBody] AttendanceMarkDto dto)
        {
            var result = await _attendanceService.MarkAttendance(dto);
            return Ok(new ApiResponse<AttendanceDto>(result, "Attendance marked successfully."));
        }

        [HttpPost("mark-bulk")]
        public async Task<IActionResult> MarkBulkAttendance([FromBody] AttendanceBulkMarkDto dto)
        {
            var result = await _attendanceService.MarkBulkAttendance(dto);
            return Ok(new ApiResponse<System.Collections.Generic.List<AttendanceDto>>(result, "Bulk attendance marked successfully.", result.Count));
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyAttendance([FromQuery] DateOnly date)
        {
            var result = await _attendanceService.GetDailyAttendance(date);
            return Ok(new ApiResponse<AttendanceDailyDto>(result, "Daily attendance loaded successfully."));
        }

        [HttpGet("employee/{employeeId:guid}")]
        public async Task<IActionResult> GetEmployeeMonthlyAttendance([FromRoute] Guid employeeId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _attendanceService.GetEmployeeMonthlyAttendance(employeeId, month, year);
            return Ok(new ApiResponse<System.Collections.Generic.List<AttendanceDto>>(result, "Employee monthly attendance loaded successfully.", result.Count));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year)
        {
            var result = await _attendanceService.GetMonthlySummary(month, year);
            return Ok(new ApiResponse<System.Collections.Generic.List<AttendanceSummaryDto>>(result, "Monthly attendance summary loaded successfully.", result.Count));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAttendance([FromRoute] Guid id, [FromBody] AttendanceUpdateDto dto)
        {
            var result = await _attendanceService.UpdateAttendance(id, dto);
            return Ok(new ApiResponse<AttendanceDto>(result, "Attendance record updated successfully."));
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetAttendanceByDateRange([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _attendanceService.GetAttendanceByDateRange(startDate, endDate);
            return Ok(new ApiResponse<System.Collections.Generic.List<AttendanceDto>>(result, "Attendance records for range loaded successfully.", result.Count));
        }

        [HttpPost("archive-manual")]
        public async Task<IActionResult> ArchiveMonthlyAttendance(
            [FromQuery] int month, 
            [FromQuery] int year, 
            [FromServices] IAttendanceRepository attendanceRepo,
            [FromServices] IExcelExportService excelExportService)
        {
            var attendances = await attendanceRepo.GetAllMonthlyAttendance(month, year);
            if (attendances == null || !attendances.Any())
            {
                return BadRequest(new ApiResponse<string>(null, $"No attendance records found for {month}/{year}."));
            }

            var exportPath = await excelExportService.ExportAttendanceLogsAsync(attendances, month, year);
            await attendanceRepo.DeleteAttendancesForMonthAsync(month, year);

            return Ok(new ApiResponse<string>(exportPath, $"Successfully exported to {exportPath} and deleted DB records."));
        }
    }
}
