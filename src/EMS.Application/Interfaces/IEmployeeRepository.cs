using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EMS.Domain.Entities;

namespace EMS.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<EmsEmployee?> GetById(Guid id);
        Task<EmsEmployee?> GetWithFullDetails(Guid id);
        Task<EmsEmployee?> GetByAadhaarNumber(string aadhaarNumber);
        Task<(IEnumerable<EmsEmployee> Items, int TotalCount)> GetAll(int pageNumber, int pageSize, string? searchTerm);
        Task<EmsEmployee> Add(EmsEmployee employee);
        Task<EmsEmployee> Update(EmsEmployee employee);
        Task<bool> SoftDelete(Guid id);
        Task<bool> HardDelete(Guid id);
        Task<int> GetNextSequenceNumber();
        Task<bool> DeleteDocument(Guid docId);
    }
}
