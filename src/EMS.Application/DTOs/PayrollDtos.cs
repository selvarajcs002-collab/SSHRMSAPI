using System;

namespace EMS.Application.DTOs
{
    public class PayrollGenerateDto
    {
        public decimal Incentives { get; set; }
        public decimal Allowances { get; set; }
        public decimal AdvancePayments { get; set; }
        public decimal BaseSalary { get; set; }
    }

    public class PayrollDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Incentives { get; set; }
        public decimal Allowances { get; set; }
        public decimal AdvancePayments { get; set; }
        public decimal NetPayable { get; set; }
        public bool IsPaid { get; set; }
        public string? PayslipFilePath { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayrollSummaryDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalNetPayable { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnpaid { get; set; }
        public int TotalEmployees { get; set; }
        public int PaidCount { get; set; }
        public int UnpaidCount { get; set; }
    }
}

