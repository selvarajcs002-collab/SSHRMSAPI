using System.Globalization;
using EMS.API.Configuration;
using EMS.API.DTOs.Salary;
using EMS.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EMS.API.Services.Implementations;

public class PayslipPdfService : IPayslipPdfService
{
    private static readonly CultureInfo DateCulture = CultureInfo.InvariantCulture;
    private static readonly CultureInfo MoneyCulture = CultureInfo.GetCultureInfo("en-IN");
    private static readonly string LightBlueBg = "#d9eaf7";
    private static readonly string LightGreenBg = "#d4eddb";
    private static readonly string BorderColor = "#bdc3c7";
    private static readonly string DarkBlueText = "#061536";

    private readonly CompanyDetailsOptions _company;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PayslipPdfService> _logger;

    public PayslipPdfService(
        IOptions<CompanyDetailsOptions> companyOptions,
        IWebHostEnvironment environment,
        ILogger<PayslipPdfService> logger)
    {
        _company = companyOptions.Value;
        _environment = environment;
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePayslipPdfAsync(SalaryPayslipData data)
    {
        var documentTitle = $"Payslip_{data.VoucherNo}";

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.PageColor(Colors.White);
                page.MarginHorizontal(8, Unit.Millimetre);
                page.MarginVertical(8, Unit.Millimetre);
                page.DefaultTextStyle(style => style.FontSize(8).FontFamily(Fonts.Arial).FontColor(Colors.Black));

                page.Content().Border(1).BorderColor(BorderColor).Padding(10).Column(column =>
                {
                    column.Spacing(6);
                    
                    ComposeHeader(column);
                    
                    column.Item().LineHorizontal(1).LineColor(BorderColor);
                    
                    ComposeTitleArea(column, data);
                    
                    ComposeEmployeeDetails(column, data);
                    
                    ComposeAttendance(column, data);
                    
                    ComposeSalaryDetails(column, data);
                    
                    ComposeNetSalary(column, data.NetSalary);
                    
                    ComposeRemarks(column, data.Remarks);
                    
                    ComposeSignatures(column);
                });
            });
        })
        .WithMetadata(new DocumentMetadata
        {
            Title = documentTitle,
            Author = _company.CompanyName,
            Creator = _company.CompanyName,
            Producer = "SS HRMS"
        })
        .GeneratePdf();

        return Task.FromResult(pdf);
    }

    private void ComposeHeader(ColumnDescriptor column)
    {
        column.Item().AlignCenter().Text("S.S. EMBROIDERY").FontSize(18).Bold().FontColor(DarkBlueText);
        column.Item().AlignCenter().Text("Computerised Embroidery").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
        column.Item().AlignCenter().Text("123, Textile Street, Tirupur - 641601, Tamil Nadu, India").FontSize(8);
        column.Item().AlignCenter().Text("Phone: +91 98765 43210 | GST: 33ABCDE1234F1Z5").FontSize(8);
    }

    private void ComposeTitleArea(ColumnDescriptor column, SalaryPayslipData data)
    {
        column.Item().Background(LightBlueBg).PaddingVertical(4).AlignCenter().Text("PAYSLIP").FontSize(12).Bold().FontColor(Colors.Black);
        
        column.Item().PaddingTop(3).Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("Voucher No  :  ").FontColor(Colors.Grey.Darken2);
                text.Span(data.VoucherNo).Bold();
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Date  :  ").FontColor(Colors.Grey.Darken2);
                text.Span(FormatDate(data.SalaryToDate));
            });
        });

        column.Item().Text(text =>
        {
            text.Span("Salary Period  :  ").FontColor(Colors.Grey.Darken2);
            text.Span($"{FormatDate(data.SalaryFromDate)} - {FormatDate(data.SalaryToDate)}").Bold();
        });
    }

    private void ComposeEmployeeDetails(ColumnDescriptor column, SalaryPayslipData data)
    {
        column.Item().Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(LightBlueBg).BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5)
                .Text("EMPLOYEE DETAILS").Bold().FontSize(9);
            
            col.Item().Padding(5).Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().PaddingVertical(1).Row(r => { r.ConstantItem(80).Text("Name").FontColor(Colors.Grey.Darken2); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(data.EmployeeName).Bold(); });
                    inner.Item().PaddingVertical(1).Row(r => { r.ConstantItem(80).Text("Employee ID").FontColor(Colors.Grey.Darken2); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(data.EmployeeId.ToString()).Bold(); });
                    inner.Item().PaddingVertical(1).Row(r => { r.ConstantItem(80).Text("Employee Code").FontColor(Colors.Grey.Darken2); r.ConstantItem(10).Text(":"); r.RelativeItem().Text(data.EmployeeCode).Bold(); });
                });
            });
        });
    }

    private void ComposeAttendance(ColumnDescriptor column, SalaryPayslipData data)
    {
        column.Item().Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(LightBlueBg).BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5)
                .Text("ATTENDANCE").Bold().FontSize(9);
            
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderRight(1).BorderColor(BorderColor).PaddingVertical(3).AlignCenter().Text("Present Days").FontColor(Colors.Grey.Darken2);
                    header.Cell().BorderRight(1).BorderColor(BorderColor).PaddingVertical(3).AlignCenter().Text("Leave Days").FontColor(Colors.Grey.Darken2);
                    header.Cell().PaddingVertical(3).AlignCenter().Text("Half Days").FontColor(Colors.Grey.Darken2);
                });

                table.Cell().BorderTop(1).BorderRight(1).BorderColor(BorderColor).PaddingVertical(4).AlignCenter().Text(data.PresentDays.ToString()).Bold();
                table.Cell().BorderTop(1).BorderRight(1).BorderColor(BorderColor).PaddingVertical(4).AlignCenter().Text(data.AbsentDays.ToString()).Bold();
                table.Cell().BorderTop(1).BorderColor(BorderColor).PaddingVertical(4).AlignCenter().Text(data.HalfDays.ToString()).Bold();
            });
        });
    }

    private void ComposeSalaryDetails(ColumnDescriptor column, SalaryPayslipData data)
    {
        column.Item().Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(LightBlueBg).BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5)
                .Text("SALARY DETAILS").Bold().FontSize(9);
            
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().BorderRight(1).BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5).Text("Description").Bold();
                    header.Cell().BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text("Amount (₹)").Bold();
                });

                var presentSalary = data.PerDaySalary * data.PresentDays;
                var halfDaySalary = (data.PerDaySalary / 2) * data.HalfDays;

                AddSalaryRow(table, "Per Day Salary", data.PerDaySalary);
                AddSalaryRow(table, "Present Salary", presentSalary);
                AddSalaryRow(table, "Half Day Salary", halfDaySalary);
                
                table.Cell().BorderRight(1).BorderTop(1).BorderBottom(1).BorderColor(BorderColor).Background(LightBlueBg).PaddingVertical(3).PaddingHorizontal(5).Text("Total Salary").Bold();
                table.Cell().BorderTop(1).BorderBottom(1).BorderColor(BorderColor).Background(LightBlueBg).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(FormatMoney(data.TotalSalary)).Bold();
                
                AddSalaryRow(table, "Incentive", data.Incentive);
                AddSalaryRow(table, "Advance", data.Advance);
            });
        });
    }

    private void AddSalaryRow(TableDescriptor table, string description, decimal amount)
    {
        table.Cell().BorderRight(1).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(5).Text(description);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(5).AlignRight().Text(FormatMoney(amount));
    }

    private void ComposeNetSalary(ColumnDescriptor column, decimal netSalary)
    {
        column.Item().Border(1).BorderColor(BorderColor).Background(LightGreenBg).Padding(6).Row(row =>
        {
            row.RelativeItem().Text("NET SALARY").FontSize(11).Bold();
            row.RelativeItem().AlignRight().Text($"₹ {FormatMoney(netSalary)}").FontSize(11).Bold();
        });
    }

    private void ComposeRemarks(ColumnDescriptor column, string? remarks)
    {
        column.Item().Border(1).BorderColor(BorderColor).Column(col =>
        {
            col.Item().Background(LightBlueBg).BorderBottom(1).BorderColor(BorderColor).PaddingVertical(3).PaddingHorizontal(5)
                .Text("REMARKS").Bold().FontSize(9);
            
            col.Item().Padding(5).MinHeight(20).Text(string.IsNullOrWhiteSpace(remarks) ? "-" : remarks);
        });
    }

    private void ComposeSignatures(ColumnDescriptor column)
    {
        column.Item().PaddingTop(30).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(3).AlignCenter().Text("Employee Signature").FontSize(8);
            });
            row.ConstantItem(40);
            row.RelativeItem().Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(3).AlignCenter().Text("Authorized Signature").FontSize(8);
            });
        });
    }

    private static string FormatMoney(decimal amount) =>
        amount.ToString("N2", MoneyCulture);

    private static string FormatDate(DateTime date) =>
        date.ToString("dd MMM yyyy", DateCulture);
}
