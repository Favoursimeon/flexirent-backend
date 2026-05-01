using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using FlexiRent.Application.Models;

namespace FlexiRent.Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(FileUpload file, string fileName);
    Task<Stream> GetFileAsync(string relativePath);
    Task DeleteFileAsync(string fileName);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration config)
    {
        _basePath = config.GetValue<string>("FileStorage:BasePath") ?? "uploads";
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(FileUpload file, string fileName)
    {
        var fullPath = Path.Combine(_basePath, fileName);
        var directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.Content.CopyToAsync(stream);
        return Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath)
            .Replace("\\", "/");
    }
    public Task<Stream> GetFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", relativePath);

        return Task.FromResult<Stream>(
            new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string fileName)
    {
        var fullPath = Path.Combine(_basePath, fileName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}