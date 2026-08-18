using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface IPayrollService
    {
        Task<PayrollDto> GeneratePayroll(Guid employeeId, int month, int year, PayrollGenerateDto dto);
        Task<PayrollDto> GetPayrollById(Guid id);
        Task<List<PayrollDto>> GetEmployeePayrollHistory(Guid employeeId);
        Task<PayrollSummaryDto> GetMonthlyPayrollSummary(int month, int year);
        Task<List<PayrollDto>> GetPayrollsByDateRange(int fromMonth, int fromYear, int toMonth, int toYear);
        Task<PayrollDto> MarkAsPaid(Guid id);
        Task<(byte[] FileContents, string FileName)> DownloadPayslipPdf(Guid id);
    }
}
