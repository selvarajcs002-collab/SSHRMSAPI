using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace EMS.Infrastructure.Services
{
    public class ExcelExportService : IExcelExportService
    {
        private readonly IConfiguration _configuration;

        public ExcelExportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> ExportAttendanceLogsAsync(IEnumerable<EmsAttendance> attendances, int month, int year)
        {
            var folderPath = _configuration["ArchivalSettings:AttendanceExportPath"] ?? @"C:\EMS_Archives\Attendance";
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"Attendance_Log_{year}_{month:D2}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(folderPath, fileName);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"Attendance {month}-{year}");

            // Add Headers
            worksheet.Cell(1, 1).Value = "Employee Code";
            worksheet.Cell(1, 2).Value = "Employee Name";
            worksheet.Cell(1, 3).Value = "Designation";
            worksheet.Cell(1, 4).Value = "Date";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "Remarks";

            int row = 2;
            foreach (var att in attendances.OrderBy(a => a.Date).ThenBy(a => a.Employee?.FirstName))
            {
                worksheet.Cell(row, 1).Value = att.Employee?.EmployeeCode ?? "N/A";
                worksheet.Cell(row, 2).Value = $"{att.Employee?.FirstName} {att.Employee?.LastName}";
                worksheet.Cell(row, 3).Value = att.Employee?.Designation ?? "N/A";
                worksheet.Cell(row, 4).Value = att.Date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 5).Value = att.Status.ToString();
                worksheet.Cell(row, 6).Value = att.Remarks ?? string.Empty;
                row++;
            }

            // Formatting
            if (row > 2)
            {
                var tableRange = worksheet.Range(1, 1, row - 1, 6);
                var excelTable = tableRange.CreateTable();
                excelTable.Theme = XLTableTheme.TableStyleMedium2;
                worksheet.Columns().AdjustToContents();
            }

            workbook.SaveAs(filePath);

            return await Task.FromResult(filePath);
        }
    }
}
