using System;
using System.Threading;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EMS.API.Services
{
    public class AttendanceArchivalService : BackgroundService
    {
        private readonly ILogger<AttendanceArchivalService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AttendanceArchivalService(ILogger<AttendanceArchivalService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Archival Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var lastDayOfMonth = DateTime.DaysInMonth(now.Year, now.Month);

                // Run on the last day of the month between 23:00 and 23:59
                if (now.Day == lastDayOfMonth && now.Hour == 23)
                {
                    _logger.LogInformation("Triggering monthly attendance archival process.");
                    try
                    {
                        await RunArchivalProcessAsync(now.Month, now.Year);
                        _logger.LogInformation("Archival process completed successfully. Next run will be at the end of next month.");
                        
                        // Sleep until next day to prevent multiple executions in the same hour
                        await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred during the monthly attendance archival process.");
                    }
                }

                // Check every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        public async Task RunArchivalProcessAsync(int month, int year)
        {
            using var scope = _serviceProvider.CreateScope();
            var attendanceRepo = scope.ServiceProvider.GetRequiredService<IAttendanceRepository>();
            var excelExportService = scope.ServiceProvider.GetRequiredService<IExcelExportService>();

            var attendances = await attendanceRepo.GetAllMonthlyAttendance(month, year);

            if (attendances == null || !attendances.Any())
            {
                _logger.LogInformation("No attendance records found for {Month}/{Year}. Skipping export.", month, year);
                return;
            }

            // Export to Excel
            var exportPath = await excelExportService.ExportAttendanceLogsAsync(attendances, month, year);
            _logger.LogInformation("Attendance logs exported successfully to: {Path}", exportPath);

            // Delete records
            await attendanceRepo.DeleteAttendancesForMonthAsync(month, year);
            _logger.LogInformation("Deleted attendance records for {Month}/{Year} from database.", month, year);
        }
    }
}
