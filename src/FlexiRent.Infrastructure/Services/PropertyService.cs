using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using FlexiRent.Application.Models;

namespace FlexiRent.Infrastructure.Services;

public interface IPropertyService
{
    Task<PropertyDto> CreateAsync(Guid ownerId, CreatePropertyDto dto, List<FileUpload>? images);
    Task<PropertyDto> UpdateAsync(Guid propertyId, Guid requesterId, UpdatePropertyDto dto);
    Task DeleteAsync(Guid propertyId, Guid requesterId, bool isAdmin);
    Task<PropertyDto> GetByIdAsync(Guid propertyId);
    Task<PagedResult<PropertyDto>> GetAllAsync(PropertySearchDto search);
    Task<PagedResult<PropertyDto>> GetByOwnerAsync(Guid ownerId, int pageSize, Guid? cursor);
    Task<PagedResult<PropertyDto>> SearchAsync(PropertySearchDto search);
    Task UpdateStatusAsync(Guid propertyId, UpdatePropertyStatusDto dto);
    Task AddToWishlistAsync(Guid userId, Guid propertyId);
    Task RemoveFromWishlistAsync(Guid userId, Guid propertyId);
    Task<PagedResult<PropertyDto>> GetWishlistAsync(Guid userId, int pageSize, Guid? cursor);
}

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly ICacheService _cache;
    private readonly INotificationService _notificationService;

    public PropertyService(
        AppDbContext db,
        IFileStorageService fileStorage,
        ICacheService cache,
        INotificationService notificationService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _cache = cache;
        _notificationService = notificationService;
    }

    public async Task<PropertyDto> CreateAsync(Guid ownerId, CreatePropertyDto dto, List<FileUpload>? images)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = dto.Title,
            Description = dto.Description,
            PricePerMonth = dto.PricePerMonth,
            Region = dto.Region,
            Address = dto.Address,
            Type = dto.Type,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AreaSqft = dto.AreaSqft,
            Status = PropertyStatus.Pending,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Properties.Add(property);

        if (images is { Count: > 0 })
        {
            var order = 0;
            foreach (var image in images)
            {
                var fileName = $"properties/{property.Id}/{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var url = await _fileStorage.SaveFileAsync(image, fileName);
                _db.PropertyImages.Add(new PropertyImage
                {
                    Id = Guid.NewGuid(),
                    PropertyId = property.Id,
                    Url = url,
                    IsPrimary = order == 0,
                    DisplayOrder = order++,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await _cache.RemoveAsync($"properties:all::");
        await _db.SaveChangesAsync();
        return await GetByIdAsync(property.Id);
    }
    public async Task<PropertyDto> UpdateAsync(Guid propertyId, Guid requesterId, UpdatePropertyDto dto)
    {
        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new ApplicationException("Property not found.");

        if (property.OwnerId != requesterId)
            throw new ApplicationException("You are not authorised to update this property.");

        property.Title = dto.Title;
        property.Description = dto.Description;
        property.PricePerMonth = dto.PricePerMonth;
        property.Region = dto.Region;
        property.Address = dto.Address;
        property.Type = dto.Type;
        property.Bedrooms = dto.Bedrooms;
        property.Bathrooms = dto.Bathrooms;
        property.AreaSqft = dto.AreaSqft;
        property.IsAvailable = dto.IsAvailable;
        property.UpdatedAt = DateTime.UtcNow;

        await _cache.RemoveAsync($"properties:all::");
        await _db.SaveChangesAsync();
        return await GetByIdAsync(property.Id);
    }

    public async Task DeleteAsync(Guid propertyId, Guid requesterId, bool isAdmin)
    {
        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new ApplicationException("Property not found.");

        if (!isAdmin && property.OwnerId != requesterId)
            throw new ApplicationException("You are not authorised to delete this property.");

        _db.Properties.Remove(property);
        await _db.SaveChangesAsync();
    }

    public async Task<PropertyDto> GetByIdAsync(Guid propertyId)
    {
        var property = await _db.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new ApplicationException("Property not found.");

        return MapToDto(property);
    }

    public async Task<PagedResult<PropertyDto>> GetAllAsync(PropertySearchDto search)
    {
        var cacheKey = $"properties:all:{search.Cursor}:{search.PageSize}";

        var cached = await _cache.GetAsync<PagedResult<PropertyDto>>(cacheKey);
        if (cached is not null) return cached;

        var query = _db.Properties
            .Include(p => p.Images)
            .Where(p => p.Status == PropertyStatus.Approved)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        var result = await PaginateAsync(query, search.PageSize, search.Cursor);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
    public async Task<PagedResult<PropertyDto>> SearchAsync(PropertySearchDto search)
    {
        var cacheKey = $"properties:search:{search.Region}:{search.Type}:{search.MinPrice}:" +
                       $"{search.MaxPrice}:{search.MinBedrooms}:{search.SearchTerm}:" +
                       $"{search.Cursor}:{search.PageSize}";

        var cached = await _cache.GetAsync<PagedResult<PropertyDto>>(cacheKey);
        if (cached is not null) return cached;

        var query = _db.Properties
            .Include(p => p.Images)
            .Where(p => p.Status == PropertyStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Region))
            query = query.Where(p => p.Region.ToLower().Contains(search.Region.ToLower()));

        if (search.Type.HasValue)
            query = query.Where(p => p.Type == search.Type.Value);

        if (search.MinPrice.HasValue)
            query = query.Where(p => p.PricePerMonth >= search.MinPrice.Value);

        if (search.MaxPrice.HasValue)
            query = query.Where(p => p.PricePerMonth <= search.MaxPrice.Value);

        if (search.MinBedrooms.HasValue)
            query = query.Where(p => p.Bedrooms >= search.MinBedrooms.Value);

        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var term = search.SearchTerm.Trim();
            query = query.Where(p =>
                EF.Functions.ToTsVector("english", p.Title + " " + p.Description)
                    .Matches(EF.Functions.ToTsQuery("english", term)));
        }

        query = query.OrderByDescending(p => p.IsFeatured)
                     .ThenByDescending(p => p.CreatedAt);

        var result = await PaginateAsync(query, search.PageSize, search.Cursor);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
    public async Task<PagedResult<PropertyDto>> GetByOwnerAsync(Guid ownerId, int pageSize, Guid? cursor)
    {
        var query = _db.Properties
            .Include(p => p.Images)
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        return await PaginateAsync(query, pageSize, cursor);
    }

    public async Task UpdateStatusAsync(Guid propertyId, UpdatePropertyStatusDto dto)
    {
        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new ApplicationException("Property not found.");

        property.Status = dto.Status;
        property.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Notify owner
        var notificationType = dto.Status == PropertyStatus.Approved
            ? NotificationType.PropertyApproved
            : NotificationType.PropertyRejected;

        var title = dto.Status == PropertyStatus.Approved
            ? "Property Approved"
            : "Property Rejected";

        var message = dto.Status == PropertyStatus.Approved
            ? $"Your property '{property.Title}' has been approved and is now live."
            : $"Your property '{property.Title}' was rejected. Reason: {dto.Reason}";

        await _notificationService.SendAsync(
            property.OwnerId,
            notificationType,
            title,
            message,
            actionUrl: $"/properties/{property.Id}",
            referenceId: property.Id);

        await _cache.RemoveAsync($"properties:all::");
    }


    public async Task AddToWishlistAsync(Guid userId, Guid propertyId)
    {
        var exists = await _db.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.PropertyId == propertyId);

        if (exists)
            throw new ApplicationException("Property already in wishlist.");

        _db.WishlistItems.Add(new WishlistItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PropertyId = propertyId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task RemoveFromWishlistAsync(Guid userId, Guid propertyId)
    {
        var item = await _db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PropertyId == propertyId)
            ?? throw new ApplicationException("Property not in wishlist.");

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<PropertyDto>> GetWishlistAsync(Guid userId, int pageSize, Guid? cursor)
    {
        var query = _db.Properties
            .Include(p => p.Images)
            .Where(p => p.WishlistItems.Any(w => w.UserId == userId))
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();

        return await PaginateAsync(query, pageSize, cursor);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<PagedResult<PropertyDto>> PaginateAsync(
        IQueryable<Property> query,
        int pageSize,
        Guid? cursor)
    {
        if (cursor.HasValue)
        {
            var cursorDate = await query
                .Where(p => p.Id == cursor.Value)
                .Select(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(p => p.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;

        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<PropertyDto>
        {
            Items = items.Select(MapToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    private static PropertyDto MapToDto(Property p) => new()
    {
        Id = p.Id,
        OwnerId = p.OwnerId,
        Title = p.Title,
        Description = p.Description,
        PricePerMonth = p.PricePerMonth,
        Region = p.Region,
        Address = p.Address,
        Type = p.Type,
        Status = p.Status,
        Bedrooms = p.Bedrooms,
        Bathrooms = p.Bathrooms,
        AreaSqft = p.AreaSqft,
        IsAvailable = p.IsAvailable,
        IsFeatured = p.IsFeatured,
        CreatedAt = p.CreatedAt,
        ImageUrls = p.Images
            .OrderBy(i => i.DisplayOrder)
            .Select(i => i.Url)
            .ToList()
    };
}