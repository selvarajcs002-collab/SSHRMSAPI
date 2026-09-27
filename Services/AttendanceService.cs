using EMS.API.DTOs;
using EMS.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMS.API.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repository;

        public AttendanceService(IAttendanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<AttendanceSummaryResponse> GetAttendanceAsync(DateTime? attendanceDate, string? shift, int? employeeId)
        {
            var date = attendanceDate?.Date ?? DateTime.UtcNow.Date;
            var result = await _repository.GetAttendanceAsync(date, shift, employeeId);

            return new AttendanceSummaryResponse
            {
                AttendanceDate = date,
                Shift = shift ?? string.Empty,
                TotalEmployees = result.TotalEmployees,
                PresentCount = result.PresentCount,
                LeaveCount = result.LeaveCount,
                HalfDayCount = result.HalfDayCount,
                Employees = result.Data.ToList()
            };
        }

        public async Task<AttendanceSummaryResponse> SaveAttendanceAsync(BulkAttendanceRequest request, int userId)
        {
            if (request.Attendance == null || !request.Attendance.Any())
            {
                throw new ArgumentException("Attendance data is required.");
            }

            foreach (var att in request.Attendance)
            {
                if (string.IsNullOrWhiteSpace(att.Status) || !IsValidStatus(att.Status))
                {
                    throw new ArgumentException($"Invalid status for EmployeeId {att.EmployeeId}");
                }

                // Call upsert for each employee
                await _repository.UpsertAttendanceAsync(new AttendanceRequest
                {
                    EmployeeId = att.EmployeeId,
                    AttendanceDate = request.AttendanceDate.Date,
                    Shift = request.Shift,
                    Status = att.Status,
                    Remarks = att.Remarks
                }, userId);
            }

            // Return updated grid
            return await GetAttendanceAsync(request.AttendanceDate, request.Shift, null);
        }

        public async Task<AttendanceResponse> UpdateAttendanceAsync(int employeeId, AttendanceRequest request, int userId)
        {
            if (request.EmployeeId != employeeId)
            {
                throw new ArgumentException("Employee ID mismatch.");
            }
            if (string.IsNullOrWhiteSpace(request.Status) || !IsValidStatus(request.Status))
            {
                throw new ArgumentException("Invalid status.");
            }

            var result = await _repository.UpsertAttendanceAsync(new AttendanceRequest
            {
                EmployeeId = request.EmployeeId,
                AttendanceDate = request.AttendanceDate.Date,
                Shift = request.Shift,
                Status = request.Status,
                Remarks = request.Remarks
            }, userId);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to update attendance.");
            }

            return result;
        }

        public async Task<IEnumerable<AttendancePeriodSummaryDto>> GetAttendanceSummaryAsync(DateTime fromDate, DateTime toDate, string? shift)
        {
            ValidateDateRange(fromDate, toDate);
            return await _repository.GetAttendanceSummaryAsync(fromDate.Date, toDate.Date, shift);
        }

        public async Task<AttendanceEmployeeDetailDto> GetEmployeeAttendanceDetailsAsync(int employeeId, DateTime fromDate, DateTime toDate)
        {
            if (employeeId <= 0)
                throw new ArgumentException("EmployeeId must be greater than zero.");

            ValidateDateRange(fromDate, toDate);

            IEnumerable<AttendanceEmployeeDetailRow> rows;
            try
            {
                rows = await _repository.GetEmployeeAttendanceDetailsAsync(employeeId, fromDate.Date, toDate.Date);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50002)
            {
                throw new KeyNotFoundException("Employee not found.");
            }

            var detailRows = rows.ToList();
            if (!detailRows.Any())
                throw new KeyNotFoundException("Employee not found.");

            var first = detailRows[0];
            return new AttendanceEmployeeDetailDto
            {
                EmployeeId = first.EmployeeId,
                EmployeeCode = first.EmployeeCode,
                EmployeeName = first.EmployeeName,
                Shift = first.Shift,
                FromDate = fromDate.Date,
                ToDate = toDate.Date,
                PresentDays = detailRows.Count(x => x.Status == "Present"),
                AbsentDays = detailRows.Count(x => x.Status == "Absent"),
                HalfDays = detailRows.Count(x => x.Status == "Half Day"),
                NotMarkedDays = detailRows.Count(x => x.Status == "Not Marked"),
                AbsentDates = detailRows.Where(x => x.Status == "Absent").Select(x => x.AttendanceDate.Date).ToList(),
                HalfDayDates = detailRows.Where(x => x.Status == "Half Day").Select(x => x.AttendanceDate.Date).ToList(),
                NotMarkedDates = detailRows.Where(x => x.Status == "Not Marked").Select(x => x.AttendanceDate.Date).ToList(),
                AllDates = detailRows.Select(x => new AttendanceDateStatusDto
                {
                    AttendanceDate = x.AttendanceDate.Date,
                    Status = x.Status,
                    Shift = x.Shift,
                    Remarks = x.Remarks
                }).ToList()
            };
        }

        public async Task<AttendanceResponse> SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest request, int userId)
        {
            if (request == null)
                throw new ArgumentException("Attendance request is required.");

            if (request.EmployeeId <= 0)
                throw new ArgumentException("EmployeeId must be greater than zero.");

            if (request.AttendanceDate == default)
                throw new ArgumentException("AttendanceDate is required.");

            if (string.IsNullOrWhiteSpace(request.Status) || !IsValidStatus(request.Status))
                throw new ArgumentException("Status must be Present, Leave, or Half-Day.");

            var result = await _repository.SaveEmployeeAttendanceAsync(request, userId);
            if (result == null)
                throw new InvalidOperationException("Failed to save attendance.");

            return result;
        }

        private static void ValidateDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate == default || toDate == default)
                throw new ArgumentException("FromDate and ToDate are required.");

            if (fromDate.Date > toDate.Date)
                throw new ArgumentException("FromDate cannot be greater than ToDate.");
        }

        private bool IsValidStatus(string status)
        {
            return status == "Present" || status == "Leave" || status == "Half-Day";
        }
    }
}
