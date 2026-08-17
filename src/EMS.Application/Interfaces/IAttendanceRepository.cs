using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<EmsAttendance?> GetById(Guid id);
        Task<IEnumerable<EmsAttendance>> GetDailyAttendance(DateOnly date);
        Task<IEnumerable<EmsAttendance>> GetEmployeeMonthlyAttendance(Guid employeeId, int month, int year);
        Task<EmsAttendance> Add(EmsAttendance attendance);
        Task AddRange(IEnumerable<EmsAttendance> attendances);
        Task<EmsAttendance> Update(EmsAttendance attendance);
        Task<IEnumerable<EmsAttendance>> GetAllMonthlyAttendance(int month, int year);
        Task DeleteAttendancesForMonthAsync(int month, int year);
        Task<IEnumerable<EmsAttendance>> GetAttendanceByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    }
}
