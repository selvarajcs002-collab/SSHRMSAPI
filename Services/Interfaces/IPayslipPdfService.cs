using EMS.API.DTOs.Salary;

namespace EMS.API.Services.Interfaces;

public interface IPayslipPdfService
{
    Task<byte[]> GeneratePayslipPdfAsync(SalaryPayslipData data);
}
