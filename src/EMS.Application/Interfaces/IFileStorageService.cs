using System.IO;
using System.Threading.Tasks;

namespace EMS.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFile(string folderName, string fileName, Stream fileStream);
        Task<byte[]> GetFile(string filePath);
        Task<bool> DeleteFile(string filePath);
    }
}
