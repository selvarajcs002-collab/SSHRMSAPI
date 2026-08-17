using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EMS.Infrastructure.Persistence;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;
using EMS.Domain.Entities;

namespace EMS.API.Jobs
{
    public class PayrollArchivalService : BackgroundService
    {
        private readonly ILogger<PayrollArchivalService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;

        public PayrollArchivalService(
            ILogger<PayrollArchivalService> logger,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PayrollArchivalService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run the archival check
                    await RunArchivalProcessAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during archival process.");
                }

                // Check every day at 00:00
                var now = DateTime.Now;
                var nextRunTime = now.Date.AddDays(1); // Next midnight
                var delay = nextRunTime - now;

                _logger.LogInformation($"Next archival check scheduled in {delay.TotalHours} hours.");
                
                await Task.Delay(delay, stoppingToken);
            }
        }

        public async Task RunArchivalProcessAsync(CancellationToken stoppingToken = default)
        {
            var today = DateTime.Today;

            // Only run on the 1st of the month (uncomment for production).
            // if (today.Day != 1) return;

            // Last month
            var lastMonthDate = today.AddMonths(-1);
            int lastMonth = lastMonthDate.Month;
            int lastMonthYear = lastMonthDate.Year;

            // Two months ago
            var twoMonthsAgoDate = today.AddMonths(-2);
            int twoMonthsAgo = twoMonthsAgoDate.Month;
            int twoMonthsAgoYear = twoMonthsAgoDate.Year;

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Process "Last Month"
                await ProcessMonthAsync(dbContext, lastMonth, lastMonthYear, forceArchive: false);

                // 2. Process "Two Months Ago" (Force archive regardless of unpaid)
                await ProcessMonthAsync(dbContext, twoMonthsAgo, twoMonthsAgoYear, forceArchive: true);
            }
        }

        private async Task ProcessMonthAsync(ApplicationDbContext dbContext, int month, int year, bool forceArchive)
        {
            var payrolls = await dbContext.Payrolls
                .Include(p => p.Employee)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();

            if (!payrolls.Any())
            {
                return; // Nothing to archive
            }

            bool allPaid = payrolls.All(p => p.IsPaid);

            if (!allPaid && !forceArchive)
            {
                _logger.LogInformation($"Skipping archival for {month}/{year} because not all payrolls are paid.");
                return; // Wait until next month (force archive)
            }

            // We proceed with archiving
            _logger.LogInformation($"Archiving payrolls for {month}/{year}. AllPaid: {allPaid}, ForceArchive: {forceArchive}");

            string archivesPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "Archives");
            if (!Directory.Exists(archivesPath))
            {
                Directory.CreateDirectory(archivesPath);
            }

            string fileName = $"Payroll_Archive_{month:D2}_{year}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string filePath = Path.Combine(archivesPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add($"Payroll {month}-{year}");
                
                worksheet.Cell(1, 1).Value = "Employee Code";
                worksheet.Cell(1, 2).Value = "Employee Name";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Base Salary";
                worksheet.Cell(1, 5).Value = "Incentives";
                worksheet.Cell(1, 6).Value = "Allowances";
                worksheet.Cell(1, 7).Value = "Advances";
                worksheet.Cell(1, 8).Value = "Net Payable";
                worksheet.Cell(1, 9).Value = "Status";
                worksheet.Cell(1, 10).Value = "Paid Date";

                var headerRow = worksheet.Row(1);
                headerRow.Style.Font.Bold = true;

                int row = 2;
                foreach (var p in payrolls)
                {
                    worksheet.Cell(row, 1).Value = p.Employee?.EmployeeCode ?? "N/A";
                    worksheet.Cell(row, 2).Value = p.Employee != null ? $"{p.Employee.FirstName} {p.Employee.LastName}" : "N/A";
                    worksheet.Cell(row, 3).Value = $"{p.Month:D2}/{p.Year}";
                    worksheet.Cell(row, 4).Value = p.BaseSalary;
                    worksheet.Cell(row, 5).Value = p.Incentives;
                    worksheet.Cell(row, 6).Value = p.Allowances;
                    worksheet.Cell(row, 7).Value = p.AdvancePayments;
                    worksheet.Cell(row, 8).Value = p.NetPayable;
                    worksheet.Cell(row, 9).Value = p.IsPaid ? "PAID" : "UNPAID";
                    worksheet.Cell(row, 10).Value = p.PaidDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }

            _logger.LogInformation($"Excel archive saved to {filePath}");

            // Now delete from DB
            dbContext.Payrolls.RemoveRange(payrolls);

            // Delete corresponding attendances
            var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
            var attendances = await dbContext.Attendances
                .Where(a => a.Date.Month == month && a.Date.Year == year && employeeIds.Contains(a.EmployeeId))
                .ToListAsync();

            dbContext.Attendances.RemoveRange(attendances);

            await dbContext.SaveChangesAsync();

            _logger.LogInformation($"Deleted {payrolls.Count} payroll records and {attendances.Count} attendance records for {month}/{year}.");
        }
    }
}
