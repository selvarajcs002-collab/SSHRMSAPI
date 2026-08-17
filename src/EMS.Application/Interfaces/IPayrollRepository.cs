using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IPayrollRepository
    {
        Task<EmsPayroll?> GetById(Guid id);
        Task<EmsPayroll?> GetByEmployeeAndPeriod(Guid employeeId, int month, int year);
        Task<IEnumerable<EmsPayroll>> GetMonthlyPayrolls(int month, int year);
        Task<IEnumerable<EmsPayroll>> GetPayrollsByDateRange(int fromMonth, int fromYear, int toMonth, int toYear);
        Task<EmsPayroll> Add(EmsPayroll payroll);
        Task<EmsPayroll> Update(EmsPayroll payroll);
    }
}
