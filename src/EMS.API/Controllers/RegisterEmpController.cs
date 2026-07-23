using System;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/employees")]
    public class RegisterEmpController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public RegisterEmpController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // --- STEP 1: Basic Details ---
        [HttpPost("register/basic")]
        public async Task<IActionResult> CreateBasicDetails([FromBody] EmployeeBasicDetailCreateDto dto)
        {
            var result = await _employeeService.CreateBasicDetails(dto);
            return Ok(new ApiResponse<EmployeeBasicDetailDto>(result, "Basic details saved successfully."));
        }

        // --- STEP 2: Bank Details ---
        [HttpPost("register/{id:guid}/bank")]
        public async Task<IActionResult> SaveBankDetails([FromRoute] Guid id, [FromBody] EmployeeBankDetailCreateDto dto)
        {
            var result = await _employeeService.SaveBankDetails(id, dto);
            return Ok(new ApiResponse<EmployeeBankDetailDto>(result, "Bank details saved successfully."));
        }

        // --- STEP 3: Document Upload ---
        [HttpPost("register/{id:guid}/document")]
        public async Task<IActionResult> UploadDocument([FromRoute] Guid id, [FromQuery] EmsDocumentType type, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object>(null, "No file was uploaded."));
            }

            using var stream = file.OpenReadStream();
            var result = await _employeeService.UploadDocument(id, type, file.FileName, file.ContentType, stream);
            return Ok(new ApiResponse<EmployeeDocumentDto>(result, "Document uploaded successfully."));
        }

        [HttpGet("register/{id:guid}/document/{type}")]
        public async Task<IActionResult> GetDocument([FromRoute] Guid id, [FromRoute] EmsDocumentType type)
        {
            try
            {
                var (bytes, contentType, fileName) = await _employeeService.GetDocument(id, type);
                return File(bytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return NotFound(new ApiResponse<object>(null, ex.Message));
            }
        }

        [HttpDelete("register/{id:guid}/document/{type}")]
        public async Task<IActionResult> DeleteDocument([FromRoute] Guid id, [FromRoute] EmsDocumentType type)
        {
            try
            {
                var result = await _employeeService.DeleteDocument(id, type);
                if (!result)
                {
                    return NotFound(new ApiResponse<bool>(false, "Document not found."));
                }
                return Ok(new ApiResponse<bool>(true, "Document deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>(false, ex.Message));
            }
        }

        // --- STEP 4: Preview ---
        [HttpGet("register/{id:guid}/preview")]
        public async Task<IActionResult> GetPreview([FromRoute] Guid id)
        {
            var result = await _employeeService.GetPreview(id);
            return Ok(new ApiResponse<EmployeePreviewDto>(result, "Preview loaded successfully."));
        }

        // --- STEP 5: Confirm Registration ---
        [HttpPost("register/{id:guid}/confirm")]
        public async Task<IActionResult> ConfirmRegistration([FromRoute] Guid id)
        {
            var result = await _employeeService.ConfirmRegistration(id);
            return Ok(new ApiResponse<EmployeePreviewDto>(result, "Registration confirmed successfully."));
        }

        // --- GENERAL CRUD ---
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _employeeService.GetAllEmployees(pageNumber, pageSize, search);
            return Ok(new ApiResponse<List<EmployeeListDto>>(result.Items, "Employees list loaded successfully.", result.TotalCount));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEmployeeById([FromRoute] Guid id)
        {
            var result = await _employeeService.GetEmployeeById(id);
            return Ok(new ApiResponse<EmployeePreviewDto>(result, "Employee details loaded successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid id, [FromBody] EmployeeBasicDetailCreateDto dto)
        {
            var result = await _employeeService.UpdateEmployee(id, dto);
            return Ok(new ApiResponse<EmployeeBasicDetailDto>(result, "Employee updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] Guid id)
        {
            var result = await _employeeService.DeleteEmployee(id);
            return Ok(new ApiResponse<bool>(result, "Employee deactivated successfully."));
        }

        [HttpDelete("{id:guid}/hard")]
        public async Task<IActionResult> HardDeleteEmployee([FromRoute] Guid id)
        {
            var result = await _employeeService.HardDeleteEmployee(id);
            return Ok(new ApiResponse<bool>(result, "Employee permanently deleted successfully."));
        }
    }
}
