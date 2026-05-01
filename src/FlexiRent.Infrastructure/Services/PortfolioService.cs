using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Application.Services;

public interface IPortfolioService
{
    Task<PortfolioImageDto> UploadImageAsync(Guid userId, UploadPortfolioImageDto dto);
    Task<IEnumerable<PortfolioImageDto>> GetImagesAsync(Guid userId);
    Task<PortfolioImageDto> UpdateImageAsync(Guid userId, Guid imageId, UpdatePortfolioImageDto dto);
    Task DeleteImageAsync(Guid userId, Guid imageId);
    Task ReorderAsync(Guid userId, List<Guid> orderedIds);
}

public class PortfolioService : IPortfolioService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public PortfolioService(AppDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<PortfolioImageDto> UploadImageAsync(Guid userId, UploadPortfolioImageDto dto)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
        var imageUrl = await _fileStorage.SaveFileAsync(dto.File, fileName);
        var image = new PortfolioImage
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = imageUrl,
            ContentType = dto.File.ContentType,
            SizeBytes = dto.File.Length,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };
        _db.PortfolioImages.Add(image);
        await _db.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task<IEnumerable<PortfolioImageDto>> GetImagesAsync(Guid userId)
    {
        var images = await _db.PortfolioImages
            .Where(i => i.OwnerId == userId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();
        return images.Select(MapToDto);
    }

    public async Task<PortfolioImageDto> UpdateImageAsync(Guid userId, Guid imageId, UpdatePortfolioImageDto dto)
    {
        var image = await _db.PortfolioImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.OwnerId == userId)
            ?? throw new ApplicationException("Image not found.");
        image.Title = dto.Title;
        image.Description = dto.Description;
        image.DisplayOrder = dto.DisplayOrder;
        image.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(image);
    }

    public async Task DeleteImageAsync(Guid userId, Guid imageId)
    {
        var image = await _db.PortfolioImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.OwnerId == userId)
            ?? throw new ApplicationException("Image not found.");
        await _fileStorage.DeleteFileAsync(image.ImageUrl);
        _db.PortfolioImages.Remove(image);
        await _db.SaveChangesAsync();
    }

    public async Task ReorderAsync(Guid userId, List<Guid> orderedIds)
    {
        var images = await _db.PortfolioImages
            .Where(i => i.OwnerId == userId && orderedIds.Contains(i.Id))
            .ToListAsync();
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var image = images.FirstOrDefault(x => x.Id == orderedIds[i]);
            if (image != null) image.DisplayOrder = i;
        }
        await _db.SaveChangesAsync();
    }

    private static PortfolioImageDto MapToDto(PortfolioImage i) => new(
        i.Id, i.OwnerId, i.Title, i.Description, i.ImageUrl,
        i.ContentType, i.SizeBytes, i.DisplayOrder, i.CreatedAt, i.UpdatedAt);
}