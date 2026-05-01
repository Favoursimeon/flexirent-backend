using System;

namespace FlexiRent.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? FolderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsShared { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Owner { get; set; } = null!;
    public DocumentFolder? Folder { get; set; }
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<SharedDocument> SharedWith { get; set; } = new List<SharedDocument>();
}