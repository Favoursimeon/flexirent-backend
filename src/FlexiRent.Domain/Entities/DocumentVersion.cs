using System;

namespace FlexiRent.Domain.Entities;

public class DocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UploadedById { get; set; }
    public int VersionNumber { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? ChangeNotes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Document Document { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}