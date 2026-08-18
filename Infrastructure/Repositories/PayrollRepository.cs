using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EMS.Domain.Constants;

namespace EMS.Infrastructure.Repositories
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly ApplicationDbContext _context;

        public PayrollRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsPayroll?> GetById(Guid id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<EmsPayroll?> GetByEmployeeAndPeriod(Guid employeeId, int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.Month == month && p.Year == year);
        }

        public async Task<IEnumerable<EmsPayroll>> GetMonthlyPayrolls(int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmsPayroll>> GetPayrollsByDateRange(int fromMonth, int fromYear, int toMonth, int toYear)
        {
            var start = new DateTime(fromYear, fromMonth, 1);
            var end = new DateTime(toYear, toMonth, DateTime.DaysInMonth(toYear, toMonth));
            
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p => 
                    (p.Year > fromYear || (p.Year == fromYear && p.Month >= fromMonth)) &&
                    (p.Year < toYear || (p.Year == toYear && p.Month <= toMonth))
                )
                .ToListAsync();
        }

        public async Task<EmsPayroll> Add(EmsPayroll payroll)
        {
            await ExecuteGeneratePayrollSp(payroll);
            return payroll;
        }

        public async Task<EmsPayroll> Update(EmsPayroll payroll)
        {
            await ExecuteGeneratePayrollSp(payroll);
            return payroll;
        }

        private async Task ExecuteGeneratePayrollSp(EmsPayroll payroll)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.GeneratePayroll} @Id, @EmployeeId, @Month, @Year, @BaseSalary, @Incentives, @Allowances, @AdvancePayments, @NetPayable, @IsPaid, @PaidDate, @PayslipFilePath, @CreatedAt, @UpdatedAt",
                new SqlParameter("@Id", payroll.Id),
                new SqlParameter("@EmployeeId", payroll.EmployeeId),
                new SqlParameter("@Month", payroll.Month),
                new SqlParameter("@Year", payroll.Year),
                new SqlParameter("@BaseSalary", payroll.BaseSalary),
                new SqlParameter("@Incentives", payroll.Incentives),
                new SqlParameter("@Allowances", payroll.Allowances),
                new SqlParameter("@AdvancePayments", payroll.AdvancePayments),
                new SqlParameter("@NetPayable", payroll.NetPayable),
                new SqlParameter("@IsPaid", payroll.IsPaid),
                new SqlParameter("@PaidDate", (object?)payroll.PaidDate ?? DBNull.Value),
                new SqlParameter("@PayslipFilePath", (object?)payroll.PayslipFilePath ?? DBNull.Value),
                new SqlParameter("@CreatedAt", payroll.CreatedAt),
                new SqlParameter("@UpdatedAt", payroll.UpdatedAt == default(DateTime) ? DBNull.Value : (object)payroll.UpdatedAt)
            );
            _context.Entry(payroll).State = EntityState.Unchanged;
        }
    }
}
