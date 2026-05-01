using FlexiRent.Application.Models;

namespace FlexiRent.Application.DTOs;

public record CreateFolderDto(string Name, string? Description, Guid? ParentFolderId);
public record UpdateFolderDto(string Name, string? Description);

public record UploadDocumentDto(
    FileUpload File,
    string Title,
    string? Description,
    Guid? FolderId);

public record UpdateDocumentDto(string Title, string? Description, Guid? FolderId);

public record ShareDocumentDto(
    Guid SharedWithUserId,
    bool CanEdit,
    bool CanDownload,
    DateTime? ExpiresAt);

public record DocumentDto(
    Guid Id, Guid OwnerId, Guid? FolderId, string Title,
    string FileName, string FileUrl, string ContentType,
    long SizeBytes, bool IsShared, string? Description,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record DocumentVersionDto(
    Guid Id, Guid DocumentId, Guid UploadedById,
    int VersionNumber, string FileUrl, string FileName,
    long SizeBytes, string? ChangeNotes, DateTime CreatedAt);

public record DocumentFolderDto(
    Guid Id, Guid OwnerId, Guid? ParentFolderId,
    string Name, string? Description, int DocumentCount,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record SharedDocumentDto(
    Guid Id, Guid DocumentId, string DocumentTitle,
    Guid SharedByUserId, Guid SharedWithUserId,
    bool CanEdit, bool CanDownload,
    DateTime SharedAt, DateTime? ExpiresAt);