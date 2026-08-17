using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using EMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EMS.Domain.Constants;

namespace EMS.Infrastructure.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly ApplicationDbContext _context;

        public SettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmsSetting?> GetByKey(string key)
        {
            return await _context.Settings
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<IEnumerable<EmsSetting>> GetAll()
        {
            return await _context.Settings.ToListAsync();
        }

        public async Task<EmsSetting> Update(EmsSetting setting)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"EXEC {EmsStoredProcedures.UpdateSetting} @Id, @Key, @Value, @Description, @UpdatedAt",
                new SqlParameter("@Id", setting.Id),
                new SqlParameter("@Key", setting.Key),
                new SqlParameter("@Value", setting.Value),
                new SqlParameter("@Description", (object?)setting.Description ?? DBNull.Value),
                new SqlParameter("@UpdatedAt", setting.UpdatedAt)
            );
            _context.Entry(setting).State = EntityState.Unchanged;
            return setting;
        }
    }
}
