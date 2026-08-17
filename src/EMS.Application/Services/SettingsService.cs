using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;

namespace EMS.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingRepository _repository;
        private readonly IMapper _mapper;

        public SettingsService(ISettingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<SettingDto>> GetSettings()
        {
            var settings = await _repository.GetAll();
            var list = settings.ToList();

            // Seed defaults if empty
            if (!list.Any())
            {
                var defaultSettings = new List<EmsSetting>
                {
                    new EmsSetting { Id = Guid.NewGuid(), Key = "SystemName", Value = "Employee Management System", Description = "Name of the system shown on headers", UpdatedAt = DateTime.UtcNow },
                    new EmsSetting { Id = Guid.NewGuid(), Key = "AdminEmail", Value = "admin@ems.local", Description = "System administrator email address", UpdatedAt = DateTime.UtcNow },
                    new EmsSetting { Id = Guid.NewGuid(), Key = "Alert9AM", Value = "Good morning! Time to clock-in for the morning shift.", Description = "Message for morning shift reminder", UpdatedAt = DateTime.UtcNow },
                    new EmsSetting { Id = Guid.NewGuid(), Key = "Alert9PM", Value = "Good evening! Time to clock-in for the night shift.", Description = "Message for night shift reminder", UpdatedAt = DateTime.UtcNow }
                };

                foreach (var s in defaultSettings)
                {
                    await _repository.Update(s);
                }
                settings = await _repository.GetAll();
                list = settings.ToList();
            }

            return _mapper.Map<List<SettingDto>>(list);
        }

        public async Task<SettingDto> UpdateSetting(string key, string value)
        {
            var setting = await _repository.GetByKey(key);
            if (setting == null)
            {
                setting = new EmsSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            var updated = await _repository.Update(setting);
            return _mapper.Map<SettingDto>(updated);
        }

        public async Task<List<NotificationScheduleDto>> GetNotificationSchedules()
        {
            var settings = await GetSettings();
            var morningMsg = settings.FirstOrDefault(s => s.Key == "Alert9AM")?.Value ?? "Time to clock-in for the morning shift.";
            var eveningMsg = settings.FirstOrDefault(s => s.Key == "Alert9PM")?.Value ?? "Time to clock-in for the night shift.";

            var now = DateTime.Now; // use local server time for alerts
            
            var schedule = new List<NotificationScheduleDto>
            {
                CalculateSchedule("Morning Shift Reminder (9:00 AM)", 9, 0, morningMsg, now),
                CalculateSchedule("Night Shift Reminder (9:00 PM)", 21, 0, eveningMsg, now)
            };

            return schedule;
        }

        private NotificationScheduleDto CalculateSchedule(string alertName, int hour, int minute, string message, DateTime now)
        {
            var alertTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (now > alertTime)
            {
                alertTime = alertTime.AddDays(1);
            }

            var timeUntil = alertTime - now;

            return new NotificationScheduleDto
            {
                AlertName = alertName,
                ScheduledTime = alertTime.ToString("yyyy-MM-dd HH:mm:ss"),
                AlertMessage = message,
                CurrentTime = now,
                TimeUntilAlertMinutes = Math.Round(timeUntil.TotalMinutes, 1)
            };
        }
    }
}
