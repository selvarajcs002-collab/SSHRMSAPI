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
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsAttendance?> GetById(Guid id)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<EmsAttendance>> GetDailyAttendance(DateOnly date)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date == date)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmsAttendance>> GetEmployeeMonthlyAttendance(Guid employeeId, int month, int year)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employeeId && a.Date.Month == month && a.Date.Year == year)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<EmsAttendance> Add(EmsAttendance attendance)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.MarkAttendance} @Id, @EmployeeId, @Date, @Status, @Remarks, @CreatedAt, @UpdatedAt",
                new SqlParameter("@Id", attendance.Id),
                new SqlParameter("@EmployeeId", attendance.EmployeeId),
                new SqlParameter("@Date", attendance.Date),
                new SqlParameter("@Status", (int)attendance.Status),
                new SqlParameter("@Remarks", (object?)attendance.Remarks ?? DBNull.Value),
                new SqlParameter("@CreatedAt", attendance.CreatedAt),
                new SqlParameter("@UpdatedAt", attendance.UpdatedAt)
            );
            return attendance;
        }

        public async Task AddRange(IEnumerable<EmsAttendance> attendances)
        {
            foreach (var attendance in attendances)
            {
                await Add(attendance);
            }
        }

        public async Task<EmsAttendance> Update(EmsAttendance attendance)
        {
            await Add(attendance);
            _context.Entry(attendance).State = EntityState.Unchanged;
            return attendance;
        }

        public async Task<IEnumerable<EmsAttendance>> GetAllMonthlyAttendance(int month, int year)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Employee!.FirstName)
                .ToListAsync();
        }

        public async Task DeleteAttendancesForMonthAsync(int month, int year)
        {
            var attendancesToDelete = await _context.Attendances
                .Where(a => a.Date.Month == month && a.Date.Year == year)
                .ToListAsync();

            if (attendancesToDelete.Any())
            {
                _context.Attendances.RemoveRange(attendancesToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<EmsAttendance>> GetAttendanceByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Employee!.FirstName)
                .ToListAsync();
        }
    }
}
