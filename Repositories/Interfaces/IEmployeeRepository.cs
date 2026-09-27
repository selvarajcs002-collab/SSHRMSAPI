using EMS.API.Common;
using EMS.API.DTOs.Employee;
using EMS.API.Models;

namespace EMS.API.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<PaginationResponse<Employee>> GetListAsync(PaginationRequest request);
    Task<Employee?> GetByIdAsync(int employeeId);
    Task<int> InsertAsync(Employee employee, int? createdBy = null);
    Task UpdateAsync(Employee employee, int? updatedBy = null);
    Task DeleteAsync(int employeeId, int? updatedBy = null);
    Task<DeleteEmployeeResponse> DeleteEmployeeAsync(int employeeId, int deletedBy);
    Task<int> DuplicateAsync(int employeeId, string newEmployeeCode, int? createdBy = null);
    
    Task<int> SaveDocumentAsync(EmployeeDocument document, int? uploadedBy = null);
    Task<EmployeeDocument?> GetDocumentAsync(int employeeId);
    Task ShiftManagementAsync(EMS.API.DTOs.Employee.ShiftManagementRequest request, int? updatedBy = null);
}
