using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/payroll")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpPost("generate/{employeeId:guid}")]
        public async Task<IActionResult> GeneratePayroll([FromRoute] Guid employeeId, [FromQuery] int month, [FromQuery] int year, [FromBody] PayrollGenerateDto dto)
        {
            var result = await _payrollService.GeneratePayroll(employeeId, month, year, dto);
            return Ok(new ApiResponse<PayrollDto>(result, "Payroll generated successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPayrollById([FromRoute] Guid id)
        {
            var result = await _payrollService.GetPayrollById(id);
            return Ok(new ApiResponse<PayrollDto>(result, "Payroll record loaded successfully."));
        }

        [HttpGet("employee/{employeeId:guid}")]
        public async Task<IActionResult> GetEmployeePayrollHistory([FromRoute] Guid employeeId)
        {
            var result = await _payrollService.GetEmployeePayrollHistory(employeeId);
            return Ok(new ApiResponse<List<PayrollDto>>(result, "Employee payroll history loaded successfully.", result.Count));
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMonthlyPayrollSummary([FromQuery] int month, [FromQuery] int year)
        {
            var result = await _payrollService.GetMonthlyPayrollSummary(month, year);
            return Ok(new ApiResponse<PayrollSummaryDto>(result, "Monthly payroll summary loaded successfully."));
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetPayrollsByDateRange([FromQuery] int fromMonth, [FromQuery] int fromYear, [FromQuery] int toMonth, [FromQuery] int toYear)
        {
            var result = await _payrollService.GetPayrollsByDateRange(fromMonth, fromYear, toMonth, toYear);
            return Ok(new ApiResponse<List<PayrollDto>>(result, "Payrolls by date range loaded successfully.", result.Count));
        }

        [HttpPut("{id:guid}/pay")]
        public async Task<IActionResult> MarkAsPaid([FromRoute] Guid id)
        {
            var result = await _payrollService.MarkAsPaid(id);
            return Ok(new ApiResponse<PayrollDto>(result, "Payroll record marked as paid successfully."));
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> DownloadPayslipPdf([FromRoute] Guid id)
        {
            var (fileContents, fileName) = await _payrollService.DownloadPayslipPdf(id);
            return File(fileContents, "text/plain", fileName);
        }
    }
}
