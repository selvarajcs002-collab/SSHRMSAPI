using System.Data;
using Dapper;
using EMS.API.Common;
using EMS.API.DTOs.Employee;
using EMS.API.Models;
using EMS.API.Repositories.Interfaces;

namespace EMS.API.Repositories.Implementations;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbConnection _dbConnection;

    public EmployeeRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<PaginationResponse<Employee>> GetListAsync(PaginationRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Search", request.Search);
        parameters.Add("@PageNumber", request.PageNumber);
        parameters.Add("@PageSize", request.PageSize);
        parameters.Add("@SortColumn", request.SortColumn);
        parameters.Add("@SortDirection", request.SortDirection);

        var result = await _dbConnection.QueryAsync<Employee>(
            "dbo.usp_Employee_GetList",
            parameters,
            commandType: CommandType.StoredProcedure);

        var response = new PaginationResponse<Employee>
        {
            Items = result,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalRecords = result.FirstOrDefault()?.TotalCount ?? 0
        };

        return response;
    }

    public async Task<Employee?> GetByIdAsync(int employeeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);

        return await _dbConnection.QueryFirstOrDefaultAsync<Employee>(
            "dbo.usp_Employee_GetById",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertAsync(Employee employee, int? createdBy = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeCode", employee.EmployeeCode);
        parameters.Add("@Name", employee.Name);
        parameters.Add("@Designation", employee.Designation);
        parameters.Add("@City", employee.City);
        parameters.Add("@PerDaySalary", employee.PerDaySalary);
        parameters.Add("@JoinedYear", employee.JoinedYear);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@EmployeeId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _dbConnection.ExecuteAsync(
            "dbo.usp_Employee_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@EmployeeId");
    }

    public async Task UpdateAsync(Employee employee, int? updatedBy = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employee.EmployeeId);
        parameters.Add("@Name", employee.Name);
        parameters.Add("@Designation", employee.Designation);
        parameters.Add("@City", employee.City);
        parameters.Add("@PerDaySalary", employee.PerDaySalary);
        parameters.Add("@JoinedYear", employee.JoinedYear);
        parameters.Add("@UpdatedBy", updatedBy);

        await _dbConnection.ExecuteAsync(
            "dbo.usp_Employee_Update",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int employeeId, int deletedBy)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@DeletedBy", deletedBy);

        var result = await _dbConnection.QueryFirstOrDefaultAsync<DeleteEmployeeResponse>(
            "dbo.usp_Employee_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);

        return result ?? new DeleteEmployeeResponse
        {
            Success = false,
            Message = "No response from database.",
            EmployeeId = employeeId
        };
    }

    public async Task DeleteAsync(int employeeId, int? updatedBy = null)
    {
        await DeleteEmployeeAsync(employeeId, updatedBy ?? 1);
    }

    public async Task<int> DuplicateAsync(int employeeId, string newEmployeeCode, int? createdBy = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);
        parameters.Add("@NewEmployeeCode", newEmployeeCode);
        parameters.Add("@CreatedBy", createdBy);
        parameters.Add("@NewEmployeeId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _dbConnection.ExecuteAsync(
            "dbo.usp_Employee_Duplicate",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@NewEmployeeId");
    }

    public async Task<int> SaveDocumentAsync(EmployeeDocument document, int? uploadedBy = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", document.EmployeeId);
        parameters.Add("@DocumentName", document.DocumentName);
        parameters.Add("@OriginalFileName", document.OriginalFileName);
        parameters.Add("@FilePath", document.FilePath);
        parameters.Add("@ContentType", document.ContentType);
        parameters.Add("@FileSize", document.FileSize);
        parameters.Add("@UploadedBy", uploadedBy);
        parameters.Add("@DocumentId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await _dbConnection.ExecuteAsync(
            "dbo.usp_Employee_SaveDocument",
            parameters,
            commandType: CommandType.StoredProcedure);

        return parameters.Get<int>("@DocumentId");
    }

    public async Task<EmployeeDocument?> GetDocumentAsync(int employeeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);

        return await _dbConnection.QueryFirstOrDefaultAsync<EmployeeDocument>(
            "dbo.usp_Employee_GetDocument",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task ShiftManagementAsync(EMS.API.DTOs.Employee.ShiftManagementRequest request, int? updatedBy = null)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", request.EmployeeId);
        parameters.Add("@Name", request.Name);
        parameters.Add("@Designation", request.Designation);
        parameters.Add("@City", request.City);
        parameters.Add("@PerDaySalary", request.PerDaySalary);
        parameters.Add("@JoinedYear", request.JoinedYear);
        parameters.Add("@Shift", request.Shift);
        parameters.Add("@UpdatedBy", updatedBy);

        await _dbConnection.ExecuteAsync(
            "dbo.usp_ShiftManagement_Save",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}

