using System.Threading.Tasks;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers
{
    [ApiController]
    [Route("api/ems/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var result = await _settingsService.GetSettings();
            return Ok(new ApiResponse<System.Collections.Generic.List<SettingDto>>(result, "System settings loaded successfully.", result.Count));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSetting([FromBody] SettingUpdateDto dto)
        {
            var result = await _settingsService.UpdateSetting(dto.Key, dto.Value);
            return Ok(new ApiResponse<SettingDto>(result, "Setting updated successfully."));
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotificationSchedules()
        {
            var result = await _settingsService.GetNotificationSchedules();
            return Ok(new ApiResponse<System.Collections.Generic.List<NotificationScheduleDto>>(result, "Notification schedules loaded successfully.", result.Count));
        }
    }
}
