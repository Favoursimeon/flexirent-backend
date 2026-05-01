using FlexiRent.Application.DTOs;
using FlexiRent.Application.Models;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services;

public interface IDocumentService
{
    Task<DocumentFolderDto> CreateFolderAsync(Guid userId, CreateFolderDto dto);
    Task<DocumentFolderDto> UpdateFolderAsync(Guid userId, Guid folderId, UpdateFolderDto dto);
    Task DeleteFolderAsync(Guid userId, Guid folderId);
    Task<IEnumerable<DocumentFolderDto>> GetFoldersAsync(Guid userId);

    Task<DocumentDto> UploadDocumentAsync(Guid userId, UploadDocumentDto dto);
    Task<DocumentDto> UpdateDocumentAsync(Guid userId, Guid documentId, UpdateDocumentDto dto);
    Task DeleteDocumentAsync(Guid userId, Guid documentId);
    Task<IEnumerable<DocumentDto>> GetDocumentsAsync(Guid userId, Guid? folderId = null);
    Task<DocumentDto> GetDocumentAsync(Guid userId, Guid documentId);

    Task<DocumentVersionDto> UploadVersionAsync(Guid userId, Guid documentId, FileUpload file, string? changeNotes);
    Task<IEnumerable<DocumentVersionDto>> GetVersionsAsync(Guid userId, Guid documentId);

    Task<SharedDocumentDto> ShareDocumentAsync(Guid userId, Guid documentId, ShareDocumentDto dto);
    Task RevokeShareAsync(Guid userId, Guid shareId);
    Task<IEnumerable<SharedDocumentDto>> GetSharedWithMeAsync(Guid userId);
    Task<IEnumerable<SharedDocumentDto>> GetSharedByMeAsync(Guid userId);
}

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public DocumentService(AppDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<DocumentFolderDto> CreateFolderAsync(Guid userId, CreateFolderDto dto)
    {
        var folder = new DocumentFolder
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            ParentFolderId = dto.ParentFolderId,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };
        _db.DocumentFolders.Add(folder);
        await _db.SaveChangesAsync();
        return MapFolderToDto(folder, 0);
    }

    public async Task<DocumentFolderDto> UpdateFolderAsync(Guid userId, Guid folderId, UpdateFolderDto dto)
    {
        var folder = await _db.DocumentFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OwnerId == userId)
            ?? throw new ApplicationException("Folder not found.");
        folder.Name = dto.Name;
        folder.Description = dto.Description;
        folder.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var count = await _db.Documents.CountAsync(d => d.FolderId == folderId);
        return MapFolderToDto(folder, count);
    }

    public async Task DeleteFolderAsync(Guid userId, Guid folderId)
    {
        var folder = await _db.DocumentFolders
            .FirstOrDefaultAsync(f => f.Id == folderId && f.OwnerId == userId)
            ?? throw new ApplicationException("Folder not found.");
        _db.DocumentFolders.Remove(folder);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentFolderDto>> GetFoldersAsync(Guid userId)
    {
        var folders = await _db.DocumentFolders
            .Where(f => f.OwnerId == userId)
            .ToListAsync();
        var result = new List<DocumentFolderDto>();
        foreach (var folder in folders)
        {
            var count = await _db.Documents.CountAsync(d => d.FolderId == folder.Id);
            result.Add(MapFolderToDto(folder, count));
        }
        return result;
    }

    public async Task<DocumentDto> UploadDocumentAsync(Guid userId, UploadDocumentDto dto)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
        var fileUrl = await _fileStorage.SaveFileAsync(dto.File, fileName);
        var document = new Document
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            FolderId = dto.FolderId,
            Title = dto.Title,
            Description = dto.Description,
            FileName = dto.File.FileName,
            FileUrl = fileUrl,
            ContentType = dto.File.ContentType,
            SizeBytes = dto.File.Length,
            IsShared = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Documents.Add(document);

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            UploadedById = userId,
            VersionNumber = 1,
            FileUrl = fileUrl,
            FileName = dto.File.FileName,
            SizeBytes = dto.File.Length,
            ChangeNotes = "Initial version",
            CreatedAt = DateTime.UtcNow
        };
        _db.DocumentVersions.Add(version);
        await _db.SaveChangesAsync();
        return MapToDto(document);
    }

    public async Task<DocumentDto> UpdateDocumentAsync(Guid userId, Guid documentId, UpdateDocumentDto dto)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OwnerId == userId)
            ?? throw new ApplicationException("Document not found.");
        document.Title = dto.Title;
        document.Description = dto.Description;
        document.FolderId = dto.FolderId;
        document.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(document);
    }

    public async Task DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OwnerId == userId)
            ?? throw new ApplicationException("Document not found.");
        await _fileStorage.DeleteFileAsync(document.FileUrl);
        _db.Documents.Remove(document);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<DocumentDto>> GetDocumentsAsync(Guid userId, Guid? folderId = null)
    {
        var query = _db.Documents.Where(d => d.OwnerId == userId);
        if (folderId.HasValue)
            query = query.Where(d => d.FolderId == folderId);
        var documents = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
        return documents.Select(MapToDto);
    }

    public async Task<DocumentDto> GetDocumentAsync(Guid userId, Guid documentId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId &&
                (d.OwnerId == userId ||
                 d.SharedWith.Any(s => s.SharedWithUserId == userId)))
            ?? throw new ApplicationException("Document not found.");
        return MapToDto(document);
    }

    public async Task<DocumentVersionDto> UploadVersionAsync(Guid userId, Guid documentId, FileUpload file, string? changeNotes)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OwnerId == userId)
            ?? throw new ApplicationException("Document not found.");
        var latestVersion = await _db.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .MaxAsync(v => (int?)v.VersionNumber) ?? 0;
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fileUrl = await _fileStorage.SaveFileAsync(file, fileName);
        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            UploadedById = userId,
            VersionNumber = latestVersion + 1,
            FileUrl = fileUrl,
            FileName = file.FileName,
            SizeBytes = file.Length,
            ChangeNotes = changeNotes,
            CreatedAt = DateTime.UtcNow
        };
        _db.DocumentVersions.Add(version);
        document.FileUrl = fileUrl;
        document.FileName = file.FileName;
        document.SizeBytes = file.Length;
        document.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapVersionToDto(version);
    }

    public async Task<IEnumerable<DocumentVersionDto>> GetVersionsAsync(Guid userId, Guid documentId)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OwnerId == userId)
            ?? throw new ApplicationException("Document not found.");
        var versions = await _db.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
        return versions.Select(MapVersionToDto);
    }

    public async Task<SharedDocumentDto> ShareDocumentAsync(Guid userId, Guid documentId, ShareDocumentDto dto)
    {
        var document = await _db.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && d.OwnerId == userId)
            ?? throw new ApplicationException("Document not found.");
        var share = new SharedDocument
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            SharedByUserId = userId,
            SharedWithUserId = dto.SharedWithUserId,
            CanEdit = dto.CanEdit,
            CanDownload = dto.CanDownload,
            SharedAt = DateTime.UtcNow,
            ExpiresAt = dto.ExpiresAt
        };
        document.IsShared = true;
        _db.SharedDocuments.Add(share);
        await _db.SaveChangesAsync();
        return MapShareToDto(share, document.Title);
    }

    public async Task RevokeShareAsync(Guid userId, Guid shareId)
    {
        var share = await _db.SharedDocuments
            .Include(s => s.Document)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.Document.OwnerId == userId)
            ?? throw new ApplicationException("Share not found.");

        _db.SharedDocuments.Remove(share);
        await _db.SaveChangesAsync();

        var remainingShares = await _db.SharedDocuments
            .CountAsync(s => s.DocumentId == share.DocumentId);

        if (remainingShares == 0)
        {
            share.Document.IsShared = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<SharedDocumentDto>> GetSharedWithMeAsync(Guid userId)
    {
        var shares = await _db.SharedDocuments
            .Include(s => s.Document)
            .Where(s => s.SharedWithUserId == userId &&
                (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow))
            .ToListAsync();
        return shares.Select(s => MapShareToDto(s, s.Document.Title));
    }

    public async Task<IEnumerable<SharedDocumentDto>> GetSharedByMeAsync(Guid userId)
    {
        var shares = await _db.SharedDocuments
            .Include(s => s.Document)
            .Where(s => s.SharedByUserId == userId)
            .ToListAsync();
        return shares.Select(s => MapShareToDto(s, s.Document.Title));
    }

    private static DocumentDto MapToDto(Document d) => new(
        d.Id, d.OwnerId, d.FolderId, d.Title, d.FileName,
        d.FileUrl, d.ContentType, d.SizeBytes, d.IsShared,
        d.Description, d.CreatedAt, d.UpdatedAt);

    private static DocumentFolderDto MapFolderToDto(DocumentFolder f, int count) => new(
        f.Id, f.OwnerId, f.ParentFolderId, f.Name,
        f.Description, count, f.CreatedAt, f.UpdatedAt);

    private static DocumentVersionDto MapVersionToDto(DocumentVersion v) => new(
        v.Id, v.DocumentId, v.UploadedById, v.VersionNumber,
        v.FileUrl, v.FileName, v.SizeBytes, v.ChangeNotes, v.CreatedAt);

    private static SharedDocumentDto MapShareToDto(SharedDocument s, string title) => new(
        s.Id, s.DocumentId, title, s.SharedByUserId,
        s.SharedWithUserId, s.CanEdit, s.CanDownload, s.SharedAt, s.ExpiresAt);
}