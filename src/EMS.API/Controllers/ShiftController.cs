using System;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/shifts")]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetAllShiftTypes()
        {
            var result = await _shiftService.GetAllShiftTypes();
            return Ok(new ApiResponse<List<ShiftTypeDto>>(result, "Shift types loaded successfully.", result.Count));
        }

        [HttpPost("assignments")]
        public async Task<IActionResult> AssignShift([FromBody] ShiftAssignmentCreateDto dto)
        {
            var result = await _shiftService.AssignShift(dto);
            return Ok(new ApiResponse<ShiftAssignmentDto>(result, "Shift assigned successfully."));
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetAllAssignments()
        {
            var result = await _shiftService.GetAllAssignments();
            return Ok(new ApiResponse<List<ShiftAssignmentDto>>(result, "Shift assignments loaded successfully.", result.Count));
        }

        [HttpGet("assignments/employee/{employeeId:guid}")]
        public async Task<IActionResult> GetAssignmentsByEmployee([FromRoute] Guid employeeId)
        {
            var result = await _shiftService.GetAssignmentsByEmployee(employeeId);
            return Ok(new ApiResponse<List<ShiftAssignmentDto>>(result, "Employee shift assignments loaded successfully.", result.Count));
        }

        [HttpPut("assignments/{id:guid}")]
        public async Task<IActionResult> UpdateAssignment([FromRoute] Guid id, [FromBody] ShiftAssignmentUpdateDto dto)
        {
            var result = await _shiftService.UpdateAssignment(id, dto);
            return Ok(new ApiResponse<ShiftAssignmentDto>(result, "Shift assignment updated successfully."));
        }

        [HttpDelete("assignments/{id:guid}")]
        public async Task<IActionResult> RemoveAssignment([FromRoute] Guid id)
        {
            var success = await _shiftService.RemoveAssignment(id);
            if (!success)
            {
                throw new KeyNotFoundException("Shift assignment not found.");
            }
            return Ok(new ApiResponse<bool>(success, "Shift assignment removed successfully."));
        }
    }
}
