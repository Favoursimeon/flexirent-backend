using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<AvatarUploadResultDto> UploadAvatarAsync(Guid userId, IFormFile file);
    Task UpdateEmergencyContactAsync(Guid userId, EmergencyContactDto dto);
    Task UpdatePrivacySettingsAsync(Guid userId, PrivacySettingsDto dto);
    Task UpdatePreferencesAsync(Guid userId, UserPreferencesDto dto);
    Task RequestAccountDeletionAsync(Guid userId, DeleteAccountRequestDto dto);
}

public class ProfileService : IProfileService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public ProfileService(AppDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        return MapToDto(profile);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.FirstName = dto.FirstName;
        profile.LastName = dto.LastName;
        profile.Phone = dto.Phone;
        profile.Bio = dto.Bio;
        profile.DateOfBirth = dto.DateOfBirth;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<AvatarUploadResultDto> UploadAvatarAsync(Guid userId, IFormFile file)
    {
        if (file.Length == 0)
            throw new ApplicationException("File is empty.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            throw new ApplicationException("Only JPEG, PNG and WebP images are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            throw new ApplicationException("Avatar must be under 5MB.");

        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        var fileName = $"avatars/{userId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var avatarUrl = await _fileStorage.SaveFileAsync(file, fileName);

        profile.AvatarUrl = avatarUrl;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new AvatarUploadResultDto { AvatarUrl = avatarUrl };
    }

    public async Task UpdateEmergencyContactAsync(Guid userId, EmergencyContactDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.EmergencyContactName = dto.Name;
        profile.EmergencyContactPhone = dto.Phone;
        profile.EmergencyContactRelation = dto.Relation;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task UpdatePrivacySettingsAsync(Guid userId, PrivacySettingsDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.ShowOnlineStatus = dto.ShowOnlineStatus;
        profile.IsSearchable = dto.IsSearchable;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task UpdatePreferencesAsync(Guid userId, UserPreferencesDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        profile.PropertyAlerts = dto.PropertyAlerts;
        profile.PreferredRegion = dto.PreferredRegion;
        profile.MinPrice = dto.MinPrice;
        profile.MaxPrice = dto.MaxPrice;
        profile.MinBedrooms = dto.MinBedrooms;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task RequestAccountDeletionAsync(Guid userId, DeleteAccountRequestDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new ApplicationException("Profile not found.");

        if (profile.DeletionRequested)
            throw new ApplicationException("Account deletion already requested.");

        profile.DeletionRequested = true;
        profile.DeletionReason = dto.Reason;
        profile.DeletionRequestedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static UserProfileDto MapToDto(Profile profile) => new()
    {
        UserId = profile.UserId,
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        Phone = profile.Phone,
        AvatarUrl = profile.AvatarUrl,
        Bio = profile.Bio,
        DateOfBirth = profile.DateOfBirth,
        IsVerified = profile.IsVerified,
        ShowOnlineStatus = profile.ShowOnlineStatus,
        IsSearchable = profile.IsSearchable
    };
}