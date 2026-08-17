using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/departments")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _departmentService.GetAll();
            return Ok(new ApiResponse<List<DepartmentDto>>(result, "Departments retrieved successfully.", result.Count));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _departmentService.GetById(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<DepartmentDto>(null, $"Department with ID '{id}' not found."));
            }
            return Ok(new ApiResponse<DepartmentDto>(result, "Department retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new ApiResponse<DepartmentDto>(null, "Department name is required."));
            }
            var result = await _departmentService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<DepartmentDto>(result, "Department created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentCreateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new ApiResponse<DepartmentDto>(null, "Department name is required."));
            }
            var result = await _departmentService.Update(id, dto);
            if (result == null)
            {
                return NotFound(new ApiResponse<DepartmentDto>(null, $"Department with ID '{id}' not found."));
            }
            return Ok(new ApiResponse<DepartmentDto>(result, "Department updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _departmentService.Delete(id);
            if (!success)
            {
                return NotFound(new ApiResponse<bool>(false, $"Department with ID '{id}' not found."));
            }
            return Ok(new ApiResponse<bool>(true, "Department deleted successfully."));
        }
    }
}
