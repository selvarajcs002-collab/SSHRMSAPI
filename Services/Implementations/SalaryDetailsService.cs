using EMS.API.DTOs.Salary;
using EMS.API.Repositories.Interfaces;
using EMS.API.Services.Interfaces;

namespace EMS.API.Services.Implementations;

public class SalaryDetailsService : ISalaryDetailsService
{
    private readonly ISalaryDetailsRepository _salaryDetailsRepository;
    private readonly IPayslipPdfService _payslipPdfService;
    private readonly ILogger<SalaryDetailsService> _logger;

    public SalaryDetailsService(
        ISalaryDetailsRepository salaryDetailsRepository,
        IPayslipPdfService payslipPdfService,
        ILogger<SalaryDetailsService> logger)
    {
        _salaryDetailsRepository = salaryDetailsRepository;
        _payslipPdfService = payslipPdfService;
        _logger = logger;
    }

    public async Task<SalaryDetailsResponse> CreateSalaryDetailsAsync(CreateSalaryDetailsRequest request, int createdBy)
    {
        ValidateDates(request.SalaryFromDate, request.SalaryToDate);
        ValidateAmounts(request.Incentive, request.Advance);

        if (request.EmployeeId <= 0)
            throw new ArgumentException("Invalid EmployeeId.");

        return await _salaryDetailsRepository.CreateSalaryDetailsAsync(request, createdBy);
    }

    public async Task<SalaryDetailsResponse> UpdateSalaryDetailsAsync(int employeeId, UpdateSalaryDetailsRequest request)
    {
        ValidateDates(request.SalaryFromDate, request.SalaryToDate);
        ValidateAmounts(request.Incentive, request.Advance);

        if (employeeId <= 0)
            throw new ArgumentException("Invalid EmployeeId.");

        return await _salaryDetailsRepository.UpdateSalaryDetailsAsync(employeeId, request);
    }

    public async Task<IEnumerable<SalaryDetailsResponse>> GetSalaryDetailsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate)
    {
        if (!fromDate.HasValue || !toDate.HasValue)
            throw new ArgumentException("FromDate and ToDate are required.");

        if (fromDate.Value > toDate.Value)
            throw new ArgumentException("FromDate cannot be greater than ToDate.");

        return await _salaryDetailsRepository.GetSalaryDetailsAsync(employeeId, fromDate.Value.Date, toDate.Value.Date);
    }

    public async Task<SalaryDetailsResponse> MarkSalaryAsPaidAsync(int employeeId, MarkSalaryPaidRequest request, int paidBy)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Invalid EmployeeId.");

        if (request == null)
            throw new ArgumentException("Mark paid request is required.");

        ValidateDates(request.SalaryFromDate, request.SalaryToDate);

        _logger.LogInformation(
            "Mark salary as paid started. EmployeeId: {EmployeeId}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}, PaidBy: {PaidBy}",
            employeeId, request.SalaryFromDate.Date, request.SalaryToDate.Date, paidBy);

        var result = await _salaryDetailsRepository.MarkSalaryAsPaidAsync(
            employeeId,
            request.SalaryFromDate.Date,
            request.SalaryToDate.Date,
            paidBy);

        _logger.LogInformation(
            "Mark salary as paid completed. EmployeeId: {EmployeeId}, EmployeeSalaryId: {EmployeeSalaryId}, PaymentStatus: {PaymentStatus}",
            employeeId, result.EmployeeSalaryId, result.PaymentStatus);

        return result;
    }

    public async Task<SalarySummaryResponse> GetSalarySummaryAsync(DateTime fromDate, DateTime toDate)
    {
        ValidateDates(fromDate, toDate);
        return await _salaryDetailsRepository.GetSalarySummaryAsync(fromDate.Date, toDate.Date);
    }

    public async Task<PayslipPdfResult> GeneratePayslipAsync(int employeeId, GeneratePayslipRequest request)
    {
        if (employeeId <= 0)
            throw new ArgumentException("Invalid EmployeeId.");

        if (request == null)
            throw new ArgumentException("Payslip request is required.");

        if (request.EmployeeId > 0 && request.EmployeeId != employeeId)
            throw new ArgumentException("Invalid EmployeeId.");

        if (request.SalaryFromDate == default || request.SalaryToDate == default)
        {
            if (!string.IsNullOrEmpty(request.SalaryMonth) && DateTime.TryParse(request.SalaryMonth, out var parsedDate))
            {
                request.SalaryFromDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                request.SalaryToDate = request.SalaryFromDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                throw new ArgumentException("Invalid salary period.");
            }
        }

        ValidateDates(request.SalaryFromDate, request.SalaryToDate);

        var salaryFromDate = request.SalaryFromDate.Date;
        var salaryToDate = request.SalaryToDate.Date;

        _logger.LogInformation(
            "PDF generation started. EmployeeId: {EmployeeId}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}",
            employeeId, salaryFromDate, salaryToDate);

        SalaryPayslipData payslip;
        try
        {
            payslip = await _salaryDetailsRepository.GetOrCreatePayslipAsync(employeeId, salaryFromDate, salaryToDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "PDF generation failed. EmployeeId: {EmployeeId}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}",
                employeeId, salaryFromDate, salaryToDate);
            throw;
        }

        _logger.LogInformation(
            "Voucher ready. EmployeeId: {EmployeeId}, VoucherNo: {VoucherNo}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}",
            employeeId, payslip.VoucherNo, payslip.SalaryFromDate, payslip.SalaryToDate);

        try
        {
            var pdf = await _payslipPdfService.GeneratePayslipPdfAsync(payslip);
            _logger.LogInformation(
                "PDF generation completed. EmployeeId: {EmployeeId}, VoucherNo: {VoucherNo}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}",
                employeeId, payslip.VoucherNo, payslip.SalaryFromDate, payslip.SalaryToDate);

            return new PayslipPdfResult
            {
                Content = pdf,
                FileName = $"Payslip_{payslip.VoucherNo}.pdf",
                VoucherNo = payslip.VoucherNo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "PDF generation failed. EmployeeId: {EmployeeId}, VoucherNo: {VoucherNo}, SalaryFromDate: {SalaryFromDate}, SalaryToDate: {SalaryToDate}",
                employeeId, payslip.VoucherNo, payslip.SalaryFromDate, payslip.SalaryToDate);
            throw;
        }
    }

    private void ValidateDates(DateTime fromDate, DateTime toDate)
    {
        if (fromDate == default || toDate == default)
            throw new ArgumentException("SalaryFromDate and SalaryToDate are required.");

        if (fromDate > toDate)
            throw new ArgumentException("SalaryFromDate cannot be greater than SalaryToDate.");
    }

    private void ValidateAmounts(decimal incentive, decimal advance)
    {
        if (incentive < 0)
            throw new ArgumentException("Incentive cannot be negative.");
        if (advance < 0)
            throw new ArgumentException("Advance cannot be negative.");
    }
}
