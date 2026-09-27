using EMS.API.DTOs.Salary;

namespace EMS.API.Services.Interfaces;

public interface ISalaryDetailsService
{
    Task<SalaryDetailsResponse> CreateSalaryDetailsAsync(CreateSalaryDetailsRequest request, int createdBy);
    Task<SalaryDetailsResponse> UpdateSalaryDetailsAsync(int employeeId, UpdateSalaryDetailsRequest request);
    Task<IEnumerable<SalaryDetailsResponse>> GetSalaryDetailsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate);
    Task<SalaryDetailsResponse> MarkSalaryAsPaidAsync(int employeeId, MarkSalaryPaidRequest request, int paidBy);
    Task<SalarySummaryResponse> GetSalarySummaryAsync(DateTime fromDate, DateTime toDate);
    Task<PayslipPdfResult> GeneratePayslipAsync(int employeeId, GeneratePayslipRequest request);
}
