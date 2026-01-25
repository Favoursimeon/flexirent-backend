using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FlexiRent.Infrastructure.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string? folder = null);
        Task<Stream> GetFileAsync(string relativePath);
        Task DeleteFileAsync(string relativePath);
    }

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        public LocalFileStorageService(IConfiguration config)
        {
            _basePath = config.GetValue<string>("FileStorage:BasePath") ?? "uploads";
            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string? folder = null)
        {
            var folderPath = folder == null ? _basePath : Path.Combine(_basePath, folder);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath).Replace("\\", "/");
        }

        public Task<Stream> GetFileAsync(string relativePath)
        {
            var fp = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (!File.Exists(fp)) throw new FileNotFoundException();
            return Task.FromResult<Stream>(new FileStream(fp, FileMode.Open, FileAccess.Read));
        }

        public Task DeleteFileAsync(string relativePath)
        {
            var fp = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
            if (File.Exists(fp)) File.Delete(fp);
            return Task.CompletedTask;
        }
    }
}