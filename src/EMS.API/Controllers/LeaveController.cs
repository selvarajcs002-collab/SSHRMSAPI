using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/leaves")]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _leaveService.GetAll();
            return Ok(new ApiResponse<List<LeaveDto>>(result, "Leaves retrieved successfully.", result.Count));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _leaveService.GetById(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<LeaveDto>(null, $"Leave request with ID '{id}' not found."));
            }
            return Ok(new ApiResponse<LeaveDto>(result, "Leave request retrieved successfully."));
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(Guid employeeId)
        {
            var result = await _leaveService.GetByEmployeeId(employeeId);
            return Ok(new ApiResponse<List<LeaveDto>>(result, "Employee leaves retrieved successfully.", result.Count));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveApplyDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ApiResponse<LeaveDto>(null, "Leave application details are required."));
            }
            if (dto.StartDate > dto.EndDate)
            {
                return BadRequest(new ApiResponse<LeaveDto>(null, "Start date cannot be after end date."));
            }
            var result = await _leaveService.ApplyLeave(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<LeaveDto>(result, "Leave request applied successfully."));
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveLeave(Guid id)
        {
            try
            {
                var result = await _leaveService.ApproveLeave(id);
                return Ok(new ApiResponse<LeaveDto>(result, "Leave request approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<LeaveDto>(null, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<LeaveDto>(null, ex.Message));
            }
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectLeave(Guid id)
        {
            try
            {
                var result = await _leaveService.RejectLeave(id);
                return Ok(new ApiResponse<LeaveDto>(result, "Leave request rejected successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<LeaveDto>(null, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<LeaveDto>(null, ex.Message));
            }
        }
    }
}
