using EMS.API.DTOs.Salary;

namespace EMS.API.Repositories.Interfaces;

public interface ISalaryDetailsRepository
{
    Task<SalaryDetailsResponse> CreateSalaryDetailsAsync(CreateSalaryDetailsRequest request, int createdBy);
    Task<SalaryDetailsResponse> UpdateSalaryDetailsAsync(int employeeId, UpdateSalaryDetailsRequest request);
    Task<IEnumerable<SalaryDetailsResponse>> GetSalaryDetailsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate);
    Task<SalaryDetailsResponse> MarkSalaryAsPaidAsync(int employeeId, DateTime salaryFromDate, DateTime salaryToDate, int paidBy);
    Task<SalarySummaryResponse> GetSalarySummaryAsync(DateTime fromDate, DateTime toDate);
    Task<SalaryPayslipData> GetOrCreatePayslipAsync(int employeeId, DateTime salaryFromDate, DateTime salaryToDate);
}
