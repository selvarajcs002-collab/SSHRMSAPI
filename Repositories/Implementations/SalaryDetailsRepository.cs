using System.Data;
using Dapper;
using EMS.API.DTOs.Salary;
using EMS.API.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace EMS.API.Repositories.Implementations;

public class SalaryDetailsRepository : ISalaryDetailsRepository
{
    private readonly IDbConnection _dbConnection;

    public SalaryDetailsRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<SalaryDetailsResponse> CreateSalaryDetailsAsync(CreateSalaryDetailsRequest request, int createdBy)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", request.EmployeeId);
        parameters.Add("@SalaryFromDate", request.SalaryFromDate);
        parameters.Add("@SalaryToDate", request.SalaryToDate);
        parameters.Add("@Incentive", request.Incentive);
        parameters.Add("@Advance", request.Advance);
        parameters.Add("@Remarks", request.Remarks);
        parameters.Add("@CreatedBy", createdBy);

        try
        {
            var result = await _dbConnection.QuerySingleOrDefaultAsync<SalaryDetailsResponse>(
                "dbo.usp_CreateSalaryDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? throw new InvalidOperationException("Salary details could not be created.");
        }
        catch (SqlException ex)
        {
            throw MapSalarySqlException(ex);
        }
    }

    public async Task<SalaryDetailsResponse> UpdateSalaryDetailsAsync(int employeeId, UpdateSalaryDetailsRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@SalaryFromDate", request.SalaryFromDate);
        parameters.Add("@SalaryToDate", request.SalaryToDate);
        parameters.Add("@Incentive", request.Incentive);
        parameters.Add("@Advance", request.Advance);
        parameters.Add("@Remarks", request.Remarks);
        parameters.Add("@UpdatedBy", request.UpdatedBy);

        try
        {
            var result = await _dbConnection.QuerySingleOrDefaultAsync<SalaryDetailsResponse>(
                "dbo.usp_UpdateSalaryDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? throw new KeyNotFoundException("Salary details not found for this employee and salary period.");
        }
        catch (SqlException ex)
        {
            throw MapSalarySqlException(ex);
        }
    }

    public async Task<IEnumerable<SalaryDetailsResponse>> GetSalaryDetailsAsync(int? employeeId, DateTime? fromDate, DateTime? toDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@FromDate", fromDate?.Date);
        parameters.Add("@ToDate", toDate?.Date);

        try
        {
            return await _dbConnection.QueryAsync<SalaryDetailsResponse>(
                "dbo.usp_GetEmployeeSalaryDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
        catch (SqlException ex)
        {
            throw MapSalarySqlException(ex);
        }
    }

    public async Task<SalaryDetailsResponse> MarkSalaryAsPaidAsync(int employeeId, DateTime salaryFromDate, DateTime salaryToDate, int paidBy)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@SalaryFromDate", salaryFromDate.Date);
        parameters.Add("@SalaryToDate", salaryToDate.Date);
        parameters.Add("@PaidBy", paidBy);

        try
        {
            var result = await _dbConnection.QuerySingleOrDefaultAsync<SalaryDetailsResponse>(
                "dbo.usp_MarkEmployeeSalaryAsPaid",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? throw new KeyNotFoundException("Salary details not found for this employee and salary period.");
        }
        catch (SqlException ex)
        {
            throw MapSalarySqlException(ex);
        }
    }

    public async Task<SalarySummaryResponse> GetSalarySummaryAsync(DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@FromDate", fromDate.Date);
        parameters.Add("@ToDate", toDate.Date);

        try
        {
            var result = await _dbConnection.QuerySingleOrDefaultAsync<SalarySummaryResponse>(
                "dbo.usp_GetEmployeeSalarySummary",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new SalarySummaryResponse();
        }
        catch (SqlException ex)
        {
            throw MapSalarySqlException(ex);
        }
    }

    public async Task<SalaryPayslipData> GetOrCreatePayslipAsync(int employeeId, DateTime salaryFromDate, DateTime salaryToDate)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@SalaryFromDate", salaryFromDate.Date);
        parameters.Add("@SalaryToDate", salaryToDate.Date);

        try
        {
            var result = await _dbConnection.QuerySingleOrDefaultAsync<SalaryPayslipData>(
                "dbo.usp_GetOrCreateSalaryPayslip",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null || string.IsNullOrWhiteSpace(result.VoucherNo))
                throw new InvalidOperationException("Voucher generation failed.");

            return result;
        }
        catch (SqlException ex)
        {
            throw MapPayslipSqlException(ex);
        }
    }

    private static Exception MapPayslipSqlException(SqlException ex) => MapSalarySqlException(ex);

    private static Exception MapSalarySqlException(SqlException ex)
    {
        return ex.Number switch
        {
            50001 => new InvalidOperationException("Salary details already exist for this employee and salary period."),
            50002 => new KeyNotFoundException("Employee not found."),
            50003 => new KeyNotFoundException("Salary details not found for this employee and salary period."),
            50004 => new ArgumentException(NormalizeValidationMessage(ex.Message)),
            50005 => new InvalidOperationException("Voucher generation failed."),
            50006 => new InvalidOperationException("Salary is already marked as paid."),
            50007 => new ArgumentException("Salary amount cannot be negative."),
            50008 => new InvalidOperationException("Paid salary cannot be modified."),
            2601 or 2627 => new InvalidOperationException("Salary details already exist for this employee and salary period."),
            _ => ex
        };
    }

    private static string NormalizeValidationMessage(string message)
    {
        if (message.Contains("period", StringComparison.OrdinalIgnoreCase))
            return "Invalid salary period.";
        if (message.Contains("Incentive", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Advance", StringComparison.OrdinalIgnoreCase))
            return "Incentive and Advance cannot be negative.";
        return "Invalid EmployeeId.";
    }
}
