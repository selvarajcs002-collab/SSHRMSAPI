using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EMS.Domain.Constants;

namespace EMS.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsEmployee?> GetById(Guid id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<EmsEmployee?> GetWithFullDetails(Guid id)
        {
            return await _context.Employees
                .Include(e => e.BankDetail)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EmsEmployee?> GetByAadhaarNumber(string aadhaarNumber)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.AadhaarNumber == aadhaarNumber);
        }

        public async Task<(IEnumerable<EmsEmployee> Items, int TotalCount)> GetAll(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(e => e.FirstName.ToLower().Contains(lowerSearch) 
                                      || e.LastName.ToLower().Contains(lowerSearch) 
                                      || e.EmployeeCode.ToLower().Contains(lowerSearch)
                                      || e.PhoneNumber.Contains(lowerSearch));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<EmsEmployee> Add(EmsEmployee employee)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.RegisterEmployeeBasic} @Id, @EmployeeCode, @FirstName, @LastName, @AadhaarNumber, @PhoneNumber, @Address, @City, @State, @District, @Pincode, @PanNumber, @BloodGroup, @MaritalStatus, @SalaryPerMonth, @Referral, @IsConfirmed, @Status, @CreatedAt, @UpdatedAt, @Email, @Designation, @DepartmentName, @ProfilePicture",
                new SqlParameter("@Id", employee.Id),
                new SqlParameter("@EmployeeCode", employee.EmployeeCode ?? string.Empty),
                new SqlParameter("@FirstName", employee.FirstName),
                new SqlParameter("@LastName", employee.LastName),
                new SqlParameter("@AadhaarNumber", employee.AadhaarNumber),
                new SqlParameter("@PhoneNumber", employee.PhoneNumber),
                new SqlParameter("@Address", employee.Address),
                new SqlParameter("@City", employee.City),
                new SqlParameter("@State", employee.State),
                new SqlParameter("@District", employee.District),
                new SqlParameter("@Pincode", employee.Pincode),
                new SqlParameter("@PanNumber", (object?)employee.PanNumber ?? DBNull.Value),
                new SqlParameter("@BloodGroup", (object?)employee.BloodGroup ?? DBNull.Value),
                new SqlParameter("@MaritalStatus", (object?)employee.MaritalStatus ?? DBNull.Value),
                new SqlParameter("@SalaryPerMonth", employee.SalaryPerMonth),
                new SqlParameter("@Referral", (object?)employee.Referral ?? DBNull.Value),
                new SqlParameter("@IsConfirmed", employee.IsConfirmed),
                new SqlParameter("@Status", (int)employee.Status),
                new SqlParameter("@CreatedAt", employee.CreatedAt),
                new SqlParameter("@UpdatedAt", employee.UpdatedAt),
                new SqlParameter("@Email", (object?)employee.Email ?? DBNull.Value),
                new SqlParameter("@Designation", (object?)employee.Designation ?? DBNull.Value),
                new SqlParameter("@DepartmentName", (object?)employee.DepartmentName ?? DBNull.Value),
                new SqlParameter("@ProfilePicture", (object?)employee.ProfilePicture ?? DBNull.Value)
            );
            return employee;
        }

        public async Task<EmsEmployee> Update(EmsEmployee employee)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.UpdateEmployee} @Id, @FirstName, @LastName, @AadhaarNumber, @PhoneNumber, @Address, @City, @State, @District, @Pincode, @PanNumber, @BloodGroup, @MaritalStatus, @SalaryPerMonth, @Referral, @Status, @IsConfirmed, @EmployeeCode, @UpdatedAt, @Email, @Designation, @DepartmentName, @ProfilePicture",
                new SqlParameter("@Id", employee.Id),
                new SqlParameter("@FirstName", employee.FirstName),
                new SqlParameter("@LastName", employee.LastName),
                new SqlParameter("@AadhaarNumber", employee.AadhaarNumber),
                new SqlParameter("@PhoneNumber", employee.PhoneNumber),
                new SqlParameter("@Address", employee.Address),
                new SqlParameter("@City", employee.City),
                new SqlParameter("@State", employee.State),
                new SqlParameter("@District", employee.District),
                new SqlParameter("@Pincode", employee.Pincode),
                new SqlParameter("@PanNumber", (object?)employee.PanNumber ?? DBNull.Value),
                new SqlParameter("@BloodGroup", (object?)employee.BloodGroup ?? DBNull.Value),
                new SqlParameter("@MaritalStatus", (object?)employee.MaritalStatus ?? DBNull.Value),
                new SqlParameter("@SalaryPerMonth", employee.SalaryPerMonth),
                new SqlParameter("@Referral", (object?)employee.Referral ?? DBNull.Value),
                new SqlParameter("@Status", (int)employee.Status),
                new SqlParameter("@IsConfirmed", employee.IsConfirmed),
                new SqlParameter("@EmployeeCode", (object?)employee.EmployeeCode ?? DBNull.Value),
                new SqlParameter("@UpdatedAt", employee.UpdatedAt),
                new SqlParameter("@Email", (object?)employee.Email ?? DBNull.Value),
                new SqlParameter("@Designation", (object?)employee.Designation ?? DBNull.Value),
                new SqlParameter("@DepartmentName", (object?)employee.DepartmentName ?? DBNull.Value),
                new SqlParameter("@ProfilePicture", (object?)employee.ProfilePicture ?? DBNull.Value)
            );

            if (employee.BankDetail != null)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    $"EXEC {EmsStoredProcedures.SaveBankDetails} @Id, @EmployeeId, @AccountNumber, @IfscCode, @BankName, @PhoneNumber, @UpiId, @CreatedAt, @UpdatedAt",
                    new SqlParameter("@Id", employee.BankDetail.Id),
                    new SqlParameter("@EmployeeId", employee.BankDetail.EmployeeId),
                    new SqlParameter("@AccountNumber", employee.BankDetail.AccountNumber),
                    new SqlParameter("@IfscCode", employee.BankDetail.IfscCode),
                    new SqlParameter("@BankName", employee.BankDetail.BankName),
                    new SqlParameter("@PhoneNumber", employee.BankDetail.PhoneNumber),
                    new SqlParameter("@UpiId", (object?)employee.BankDetail.UpiId ?? DBNull.Value),
                    new SqlParameter("@CreatedAt", employee.BankDetail.CreatedAt),
                    new SqlParameter("@UpdatedAt", employee.BankDetail.UpdatedAt)
                );
            }

            // Save documents using normal EF Core tracking (no direct SP for documents requested)
            foreach (var doc in employee.Documents)
            {
                var isNew = !_context.Documents.Any(d => d.Id == doc.Id);
                if (isNew)
                {
                    _context.Entry(doc).State = EntityState.Added;
                }
            }

            // Reload from database to synchronize in-memory state with stored procedure updates
            await _context.Entry(employee).ReloadAsync();
            if (employee.BankDetail != null)
            {
                await _context.Entry(employee.BankDetail).ReloadAsync();
            }

            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> SoftDelete(Guid id)
        {
            var employee = await GetById(id);
            if (employee == null) return false;

            employee.Status = EmsEmployeeStatus.Inactive;
            employee.UpdatedAt = DateTime.UtcNow;

            // Remove any shift assignments since employee is deactivated
            var activeShifts = _context.Shifts.Where(s => s.EmployeeId == id);
            _context.Shifts.RemoveRange(activeShifts);
            await _context.SaveChangesAsync();

            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.SoftDeleteEmployee} @Id, @Status, @UpdatedAt",
                new SqlParameter("@Id", employee.Id),
                new SqlParameter("@Status", (int)employee.Status),
                new SqlParameter("@UpdatedAt", employee.UpdatedAt)
            );

            _context.Entry(employee).State = EntityState.Unchanged;
            return true;
        }

        public async Task<bool> HardDelete(Guid id)
        {
            var employee = await GetById(id);
            if (employee == null) return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetNextSequenceNumber()
        {
            // Simple sequence generator based on count of registered employees
            var count = await _context.Employees.CountAsync(e => e.IsConfirmed);
            return count + 1;
        }

        public async Task<bool> DeleteDocument(Guid docId)
        {
            var doc = await _context.Documents.FindAsync(docId);
            if (doc == null) return false;
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
