using System;

namespace EMS.Application.DTOs
{
    public class SettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SettingUpdateDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class NotificationScheduleDto
    {
        public string AlertName { get; set; } = string.Empty;
        public string ScheduledTime { get; set; } = string.Empty;
        public string AlertMessage { get; set; } = string.Empty;
        public DateTime CurrentTime { get; set; }
        public double TimeUntilAlertMinutes { get; set; }
    }
}
