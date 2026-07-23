using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IExcelExportService
    {
        Task<string> ExportAttendanceLogsAsync(IEnumerable<EmsAttendance> attendances, int month, int year);
    }
}
