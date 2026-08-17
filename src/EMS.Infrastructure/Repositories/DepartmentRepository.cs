using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Domain.Constants;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EMS.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsDepartment?> GetById(Guid id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task<IEnumerable<EmsDepartment>> GetAll()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<EmsDepartment> Upsert(EmsDepartment department)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.UpsertDepartment} @Id, @Name, @Description, @CreatedAt",
                new SqlParameter("@Id", department.Id),
                new SqlParameter("@Name", department.Name),
                new SqlParameter("@Description", (object?)department.Description ?? DBNull.Value),
                new SqlParameter("@CreatedAt", department.CreatedAt)
            );

            // Synchronize with database state
            var tracked = await _context.Departments.FindAsync(department.Id);
            if (tracked != null)
            {
                await _context.Entry(tracked).ReloadAsync();
                return tracked;
            }

            return department;
        }

        public async Task<bool> Delete(Guid id)
        {
            var exists = await _context.Departments.AnyAsync(d => d.Id == id);
            if (!exists) return false;

            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.DeleteDepartment} @Id",
                new SqlParameter("@Id", id)
            );

            return true;
        }
    }
}
