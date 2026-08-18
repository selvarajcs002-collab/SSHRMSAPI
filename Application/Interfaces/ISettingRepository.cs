using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface ISettingRepository
    {
        Task<EmsSetting?> GetByKey(string key);
        Task<IEnumerable<EmsSetting>> GetAll();
        Task<EmsSetting> Update(EmsSetting setting);
    }
}
