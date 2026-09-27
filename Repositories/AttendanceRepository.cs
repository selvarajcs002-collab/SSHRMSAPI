using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using EMS.API.DTOs;
using System.Linq;

namespace EMS.API.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AttendanceRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<(IEnumerable<AttendanceResponse> Data, int TotalEmployees, int PresentCount, int LeaveCount, int HalfDayCount)> GetAttendanceAsync(DateTime attendanceDate, string? shift, int? employeeId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@AttendanceDate", attendanceDate, DbType.Date);
            parameters.Add("@Shift", shift, DbType.String);
            parameters.Add("@EmployeeId", employeeId, DbType.Int32);

            var result = await db.QueryAsync<AttendanceResponse>(
                "usp_Attendance_Get",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            int total = result.Count();
            int present = result.Count(x => x.Status == "Present");
            int leave = result.Count(x => x.Status == "Leave");
            int halfDay = result.Count(x => x.Status == "Half-Day");

            return (result, total, present, leave, halfDay);
        }

        public async Task<AttendanceResponse?> UpsertAttendanceAsync(AttendanceRequest request, int userId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeId", request.EmployeeId, DbType.Int32);
            parameters.Add("@AttendanceDate", request.AttendanceDate, DbType.Date);
            parameters.Add("@Shift", request.Shift, DbType.String);
            parameters.Add("@Status", request.Status, DbType.String);
            parameters.Add("@Remarks", request.Remarks, DbType.String);
            parameters.Add("@UserId", userId, DbType.Int32);

            var result = await db.QuerySingleOrDefaultAsync<AttendanceResponse>(
                "usp_Attendance_Upsert",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<IEnumerable<AttendancePeriodSummaryDto>> GetAttendanceSummaryAsync(DateTime fromDate, DateTime toDate, string? shift)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", fromDate.Date, DbType.Date);
            parameters.Add("@ToDate", toDate.Date, DbType.Date);
            parameters.Add("@Shift", string.IsNullOrWhiteSpace(shift) ? null : shift, DbType.String);

            return await db.QueryAsync<AttendancePeriodSummaryDto>(
                "dbo.usp_GetAttendanceSummary",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<IEnumerable<AttendanceEmployeeDetailRow>> GetEmployeeAttendanceDetailsAsync(int employeeId, DateTime fromDate, DateTime toDate)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeId", employeeId, DbType.Int32);
            parameters.Add("@FromDate", fromDate.Date, DbType.Date);
            parameters.Add("@ToDate", toDate.Date, DbType.Date);

            return await db.QueryAsync<AttendanceEmployeeDetailRow>(
                "dbo.usp_GetEmployeeAttendanceDetails",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<AttendanceResponse?> SaveEmployeeAttendanceAsync(SaveEmployeeAttendanceRequest request, int userId)
        {
            using IDbConnection db = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeId", request.EmployeeId, DbType.Int32);
            parameters.Add("@AttendanceDate", request.AttendanceDate.Date, DbType.Date);
            parameters.Add("@Status", request.Status, DbType.String);
            parameters.Add("@Remarks", request.Remarks, DbType.String);
            parameters.Add("@UserId", userId, DbType.Int32);

            return await db.QuerySingleOrDefaultAsync<AttendanceResponse>(
                "dbo.usp_SaveEmployeeAttendance",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
