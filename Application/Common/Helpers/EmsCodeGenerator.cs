namespace EMS.Application.Common.Helpers
{
    public static class EmsCodeGenerator
    {
        public static string GenerateEmployeeCode(int sequenceNumber)
        {
            return $"EMS{sequenceNumber:D4}";
        }
    }
}
