using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Application.DTOs;

namespace EMS.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<List<SettingDto>> GetSettings();
        Task<SettingDto> UpdateSetting(string key, string value);
        Task<List<NotificationScheduleDto>> GetNotificationSchedules();
    }
}
