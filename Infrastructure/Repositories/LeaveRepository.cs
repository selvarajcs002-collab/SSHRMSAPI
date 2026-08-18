using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using EMS.Domain.Constants;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EMS.Infrastructure.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsLeave?> GetById(Guid id)
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<EmsLeave>> GetAll()
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmsLeave>> GetByEmployeeId(Guid employeeId)
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmsLeave> Add(EmsLeave leave)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.ApplyLeave} @Id, @EmployeeId, @LeaveType, @StartDate, @EndDate, @Status, @Reason, @CreatedAt, @UpdatedAt",
                new SqlParameter("@Id", leave.Id),
                new SqlParameter("@EmployeeId", leave.EmployeeId),
                new SqlParameter("@LeaveType", (int)leave.LeaveType),
                new SqlParameter("@StartDate", leave.StartDate),
                new SqlParameter("@EndDate", leave.EndDate),
                new SqlParameter("@Status", (int)leave.Status),
                new SqlParameter("@Reason", (object?)leave.Reason ?? DBNull.Value),
                new SqlParameter("@CreatedAt", leave.CreatedAt),
                new SqlParameter("@UpdatedAt", leave.UpdatedAt)
            );

            // Synchronize with database state
            var tracked = await GetById(leave.Id);
            if (tracked != null)
            {
                await _context.Entry(tracked).ReloadAsync();
                return tracked;
            }

            return leave;
        }

        public async Task<EmsLeave> UpdateStatus(Guid id, EmsLeaveStatus status)
        {
            var leave = await GetById(id);
            if (leave == null)
            {
                throw new KeyNotFoundException($"Leave application with Id '{id}' not found.");
            }

            leave.Status = status;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.UpdateLeaveStatus} @Id, @Status, @UpdatedAt",
                new SqlParameter("@Id", id),
                new SqlParameter("@Status", (int)status),
                new SqlParameter("@UpdatedAt", leave.UpdatedAt)
            );

            // Reload to sync tracking state
            await _context.Entry(leave).ReloadAsync();
            return leave;
        }
    }
}
