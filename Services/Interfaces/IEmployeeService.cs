using EMS.API.Common;
using EMS.API.DTOs.Employee;
using EMS.API.Models;

namespace EMS.API.Services.Interfaces;

public interface IEmployeeService
{
    Task<PaginationResponse<Employee>> GetListAsync(PaginationRequest request);
    Task<Employee?> GetByIdAsync(int employeeId);
    Task<ApiResponse<int>> CreateAsync(EmployeeCreateRequest request, int? userId = null);
    Task<ApiResponse<bool>> UpdateAsync(int employeeId, EmployeeUpdateRequest request, int? userId = null);
    Task<ApiResponse<bool>> DeleteAsync(int employeeId, int? userId = null);
    Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int employeeId, int deletedBy);
    Task<ApiResponse<int>> DuplicateAsync(int employeeId, EmployeeDuplicateRequest request, int? userId = null);
    
    // Future expansion for document handling
    // Task<ApiResponse<int>> SaveDocumentAsync(int employeeId, IFormFile file, int? userId = null);
    Task<EmployeeDocument?> GetDocumentAsync(int employeeId);
    Task<ApiResponse<bool>> ShiftManagementAsync(ShiftManagementRequest request, int? userId = null);
}
