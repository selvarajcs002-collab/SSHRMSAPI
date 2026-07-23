using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EMS.Application.DTOs;
using EMS.Application.Interfaces;
using EMS.Domain.Entities;

namespace EMS.Application.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IPayrollRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public PayrollService(
            IPayrollRepository repository,
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<PayrollDto> GeneratePayroll(Guid employeeId, int month, int year, PayrollGenerateDto dto)
        {
            var employee = await _employeeRepository.GetById(employeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{employeeId}' not found.");
            }

            var existing = await _repository.GetByEmployeeAndPeriod(employeeId, month, year);
            if (existing != null)
            {
                // Update existing payroll
                existing.Incentives = dto.Incentives;
                existing.Allowances = dto.Allowances;
                existing.AdvancePayments = dto.AdvancePayments;
                existing.BaseSalary = employee.SalaryPerMonth;
                existing.NetPayable = existing.BaseSalary + dto.Incentives + dto.Allowances - dto.AdvancePayments;
                existing.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                var updated = await _repository.Update(existing);
                updated.Employee = employee;
                return _mapper.Map<PayrollDto>(updated);
            }
            else
            {
                // Create new payroll
                var payroll = new EmsPayroll
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    Month = month,
                    Year = year,
                    BaseSalary = employee.SalaryPerMonth,
                    Incentives = dto.Incentives,
                    Allowances = dto.Allowances,
                    AdvancePayments = dto.AdvancePayments,
                    NetPayable = employee.SalaryPerMonth + dto.Incentives + dto.Allowances - dto.AdvancePayments,
                    IsPaid = false,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                };

                var created = await _repository.Add(payroll);
                created.Employee = employee;
                return _mapper.Map<PayrollDto>(created);
            }
        }

        public async Task<PayrollDto> GetPayrollById(Guid id)
        {
            var payroll = await _repository.GetById(id);
            if (payroll == null)
            {
                throw new KeyNotFoundException($"Payroll with Id '{id}' not found.");
            }
            return _mapper.Map<PayrollDto>(payroll);
        }

        public async Task<List<PayrollDto>> GetEmployeePayrollHistory(Guid employeeId)
        {
            var (employees, _) = await _employeeRepository.GetAll(1, 1000, null);
            var employee = employees.FirstOrDefault(e => e.Id == employeeId);

            // Fetch all payrolls and filter manually or via repository
            var list = new List<EmsPayroll>();
            for (int m = 1; m <= 12; m++)
            {
                var payroll = await _repository.GetByEmployeeAndPeriod(employeeId, m, TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")).Year);
                if (payroll != null)
                {
                    payroll.Employee = employee;
                    list.Add(payroll);
                }
            }

            return _mapper.Map<List<PayrollDto>>(list);
        }

        public async Task<PayrollSummaryDto> GetMonthlyPayrollSummary(int month, int year)
        {
            var list = (await _repository.GetMonthlyPayrolls(month, year)).ToList();

            var totalNet = list.Sum(p => p.NetPayable);
            var totalPaid = list.Where(p => p.IsPaid).Sum(p => p.NetPayable);
            var totalUnpaid = list.Where(p => !p.IsPaid).Sum(p => p.NetPayable);

            return new PayrollSummaryDto
            {
                Month = month,
                Year = year,
                TotalNetPayable = totalNet,
                TotalPaid = totalPaid,
                TotalUnpaid = totalUnpaid,
                TotalEmployees = list.Count,
                PaidCount = list.Count(p => p.IsPaid),
                UnpaidCount = list.Count(p => !p.IsPaid)
            };
        }

        public async Task<List<PayrollDto>> GetPayrollsByDateRange(int fromMonth, int fromYear, int toMonth, int toYear)
        {
            var list = (await _repository.GetPayrollsByDateRange(fromMonth, fromYear, toMonth, toYear)).ToList();
            return _mapper.Map<List<PayrollDto>>(list);
        }

        public async Task<PayrollDto> MarkAsPaid(Guid id)
        {
            var payroll = await _repository.GetById(id);
            if (payroll == null)
            {
                throw new KeyNotFoundException($"Payroll with Id '{id}' not found.");
            }

            payroll.IsPaid = true;
            payroll.PaidDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            var updated = await _repository.Update(payroll);
            var employee = await _employeeRepository.GetById(updated.EmployeeId);
            updated.Employee = employee;

            return _mapper.Map<PayrollDto>(updated);
        }

        public async Task<(byte[] FileContents, string FileName)> DownloadPayslipPdf(Guid id)
        {
            var payroll = await _repository.GetById(id);
            if (payroll == null)
            {
                throw new KeyNotFoundException($"Payroll with Id '{id}' not found.");
            }

            var employee = await _employeeRepository.GetById(payroll.EmployeeId);
            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with Id '{payroll.EmployeeId}' not found.");
            }

            // Generate a simple PDF layout as a text file for mockup, or a PDF format byte array
            var sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine("           EMPLOYEE MANAGEMENT SYSTEM             ");
            sb.AppendLine("                  PAYSLIP                         ");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Employee Code : {employee.EmployeeCode}");
            sb.AppendLine($"Name          : {employee.FirstName} {employee.LastName}");
            sb.AppendLine($"Period        : {payroll.Month:D2}/{payroll.Year}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Base Salary   : INR {payroll.BaseSalary:N2}");
            sb.AppendLine($"Incentives    : INR {payroll.Incentives:N2}");
            sb.AppendLine($"Allowances    : INR {payroll.Allowances:N2}");
            sb.AppendLine($"Advances Paid : INR {payroll.AdvancePayments:N2}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"NET PAYABLE   : INR {payroll.NetPayable:N2}");
            sb.AppendLine($"Status        : {(payroll.IsPaid ? "PAID" : "UNPAID")}");
            sb.AppendLine("==================================================");

            var text = sb.ToString();
            var bytes = Encoding.UTF8.GetBytes(text);
            var fileName = $"Payslip_{employee.EmployeeCode}_{payroll.Month}_{payroll.Year}.txt";

            return (bytes, fileName);
        }
    }
}
