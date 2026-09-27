using System.Data;
using Dapper;
using EMS.API.DTOs.Shift;
using EMS.API.Repositories.Interfaces;

namespace EMS.API.Repositories.Implementations;

public class EmployeeShiftRepository : IEmployeeShiftRepository
{
    private readonly IDbConnection _dbConnection;

    public EmployeeShiftRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<int> SaveEmployeeShiftAsync(SaveEmployeeShiftRequest request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeShiftId", request.EmployeeShiftId);
        parameters.Add("@EmployeeId", request.EmployeeId);
        parameters.Add("@Shift", request.Shift);
        parameters.Add("@EffectiveFrom", request.EffectiveFrom);
        parameters.Add("@EffectiveTo", request.EffectiveTo);
        parameters.Add("@IsActive", request.IsActive);
        parameters.Add("@CreatedBy", request.CreatedBy);
        parameters.Add("@UpdatedBy", request.UpdatedBy);

        var result = await _dbConnection.QuerySingleAsync<int>(
            "usp_SaveEmployeeShift", 
            parameters, 
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftsAsync()
    {
        var result = await _dbConnection.QueryAsync<EmployeeShiftResponse>(
            "usp_GetEmployeeShifts", 
            commandType: CommandType.StoredProcedure);

        return result;
    }

    public async Task<IEnumerable<EmployeeShiftResponse>> GetEmployeeShiftByEmployeeIdAsync(int employeeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", employeeId);

        var result = await _dbConnection.QueryAsync<EmployeeShiftResponse>(
            "usp_GetEmployeeShifts", 
            parameters, 
            commandType: CommandType.StoredProcedure);

        return result;
    }
}
