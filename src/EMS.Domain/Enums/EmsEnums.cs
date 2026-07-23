namespace EMS.Domain.Enums
{
    public enum EmsDocumentType
    {
        AadharCard = 1,
        BankPassbook = 2,
        Other = 3
    }

    public enum EmsShiftType
    {
        Morning = 1,
        Night = 2
    }

    public enum EmsAttendanceStatus
    {
        Present = 1,
        Absent = 2
    }

    public enum EmsEmployeeStatus
    {
        Active = 1,
        Inactive = 2
    }

    public enum EmsLeaveType
    {
        Sick = 1,
        Casual = 2,
        Annual = 3,
        Maternity = 4,
        Paternity = 5
    }

    public enum EmsLeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
