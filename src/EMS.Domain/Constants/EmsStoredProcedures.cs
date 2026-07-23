namespace EMS.Domain.Constants
{
    public static class EmsStoredProcedures
    {
        public const string RegisterEmployeeBasic = "sp_EMS_RegisterEmployeeBasic";
        public const string SaveBankDetails = "sp_EMS_SaveBankDetails";
        public const string UpdateEmployee = "sp_EMS_UpdateEmployee";
        public const string SoftDeleteEmployee = "sp_EMS_SoftDeleteEmployee";
        public const string AssignShift = "sp_EMS_AssignShift";
        public const string MarkAttendance = "sp_EMS_MarkAttendance";
        public const string GeneratePayroll = "sp_EMS_GeneratePayroll";
        public const string UpdateSetting = "sp_EMS_UpdateSetting";
        public const string UpsertDepartment = "sp_EMS_UpsertDepartment";
        public const string DeleteDepartment = "sp_EMS_DeleteDepartment";
        public const string ApplyLeave = "sp_EMS_ApplyLeave";
        public const string UpdateLeaveStatus = "sp_EMS_UpdateLeaveStatus";
    }
}
