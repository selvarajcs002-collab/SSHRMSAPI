using System;
using System.IO;
using System.Threading.Tasks;
using EMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EMS.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _baseUploadPath;

        public FileStorageService(IConfiguration configuration)
        {
            // Read configured path or default to a directory inside the execution path
            var configuredPath = configuration["FileStorage:UploadPath"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                // Default to a folder in the project root
                _baseUploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
            }
            else
            {
                _baseUploadPath = configuredPath;
            }

            if (!Directory.Exists(_baseUploadPath))
            {
                Directory.CreateDirectory(_baseUploadPath);
            }
        }

        public async Task<string> SaveFile(string folderName, string fileName, Stream fileStream)
        {
            var destinationFolder = Path.Combine(_baseUploadPath, folderName);
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Clean filename and generate a unique name
            var cleanFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var destinationPath = Path.Combine(destinationFolder, cleanFileName);

            using (var writeStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.CopyToAsync(writeStream);
            }

            // Return relative path for database storage and future retrieval
            var relativePath = Path.Combine(folderName, cleanFileName);
            return relativePath;
        }

        public async Task<byte[]> GetFile(string filePath)
        {
            var absolutePath = Path.Combine(_baseUploadPath, filePath);
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException("The requested file was not found.", filePath);
            }

            return await File.ReadAllBytesAsync(absolutePath);
        }

        public Task<bool> DeleteFile(string filePath)
        {
            var absolutePath = Path.Combine(_baseUploadPath, filePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
