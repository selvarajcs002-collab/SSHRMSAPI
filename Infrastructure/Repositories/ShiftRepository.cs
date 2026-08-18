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
    public class ShiftRepository : IShiftRepository
    {
        private readonly ApplicationDbContext _context;

        public ShiftRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsShift?> GetById(Guid id)
        {
            return await _context.Shifts
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<EmsShift>> GetAll()
        {
            return await _context.Shifts
                .Include(s => s.Employee)
                .OrderByDescending(s => s.AssignmentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmsShift>> GetByEmployeeId(Guid employeeId)
        {
            return await _context.Shifts
                .Include(s => s.Employee)
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.AssignmentDate)
                .ToListAsync();
        }

        public async Task<EmsShift> Add(EmsShift shift)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.AssignShift} @Id, @EmployeeId, @ShiftType, @MachineAllocation, @AssignmentDate, @CreatedAt",
                new SqlParameter("@Id", shift.Id),
                new SqlParameter("@EmployeeId", shift.EmployeeId),
                new SqlParameter("@ShiftType", (int)shift.ShiftType),
                new SqlParameter("@MachineAllocation", shift.MachineName),
                new SqlParameter("@AssignmentDate", shift.AssignmentDate),
                new SqlParameter("@CreatedAt", shift.CreatedAt)
            );
            return shift;
        }

        public async Task<EmsShift> Update(EmsShift shift)
        {
            await Add(shift);
            _context.Entry(shift).State = EntityState.Unchanged;
            return shift;
        }

        public async Task<bool> Delete(Guid id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return false;

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
