using EMS.API.Common;
using EMS.API.DTOs.Employee;
using EMS.API.Models;
using EMS.API.Repositories.Interfaces;
using EMS.API.Services.Interfaces;

namespace EMS.API.Services.Implementations;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<PaginationResponse<Employee>> GetListAsync(PaginationRequest request)
    {
        return await _employeeRepository.GetListAsync(request);
    }

    public async Task<Employee?> GetByIdAsync(int employeeId)
    {
        return await _employeeRepository.GetByIdAsync(employeeId);
    }

    public async Task<ApiResponse<int>> CreateAsync(EmployeeCreateRequest request, int? userId = null)
    {
        // Add basic validation for demonstration
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            return ApiResponse<int>.Error("Name and Employee Code are required.");
        }

        var newEmployee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            Name = request.Name,
            Designation = request.Designation,
            City = request.City,
            PerDaySalary = request.PerDaySalary,
            JoinedYear = request.JoinedYear
        };

        var id = await _employeeRepository.InsertAsync(newEmployee, userId);
        return ApiResponse<int>.Ok(id, "Employee created successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateAsync(int employeeId, EmployeeUpdateRequest request, int? userId = null)
    {
        var existing = await _employeeRepository.GetByIdAsync(employeeId);
        if (existing == null)
            return ApiResponse<bool>.Error("Employee not found.");

        existing.Name = request.Name;
        existing.Designation = request.Designation;
        existing.City = request.City;
        existing.PerDaySalary = request.PerDaySalary;
        existing.JoinedYear = request.JoinedYear;

        await _employeeRepository.UpdateAsync(existing, userId);
        return ApiResponse<bool>.Ok(true, "Employee updated successfully.");
    }

    public async Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int employeeId, int deletedBy)
    {
        try
        {
            return await _employeeRepository.DeleteEmployeeAsync(employeeId, deletedBy);
        }
        catch (Exception ex)
        {
            return new DeleteEmployeeResponse
            {
                Success = false,
                Message = "Unable to delete employee.",
                EmployeeId = employeeId,
                Error = ex.Message
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int employeeId, int? userId = null)
    {
        var result = await DeleteEmployeeAsync(employeeId, userId ?? 1);
        if (!result.Success)
            return ApiResponse<bool>.Error(result.Message ?? "Unable to delete employee.");

        return ApiResponse<bool>.Ok(true, result.Message);
    }

    public async Task<ApiResponse<int>> DuplicateAsync(int employeeId, EmployeeDuplicateRequest request, int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(request.NewEmployeeCode))
            return ApiResponse<int>.Error("New Employee Code is required.");

        var existing = await _employeeRepository.GetByIdAsync(employeeId);
        if (existing == null)
            return ApiResponse<int>.Error("Source employee not found.");

        var id = await _employeeRepository.DuplicateAsync(employeeId, request.NewEmployeeCode, userId);
        return ApiResponse<int>.Ok(id, "Employee duplicated successfully.");
    }

    public async Task<EmployeeDocument?> GetDocumentAsync(int employeeId)
    {
        return await _employeeRepository.GetDocumentAsync(employeeId);
    }

    public async Task<ApiResponse<bool>> ShiftManagementAsync(ShiftManagementRequest request, int? userId = null)
    {
        try
        {
            await _employeeRepository.ShiftManagementAsync(request, userId);
            return ApiResponse<bool>.Ok(true, "Shift management processed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Error(ex.Message);
        }
    }
}
