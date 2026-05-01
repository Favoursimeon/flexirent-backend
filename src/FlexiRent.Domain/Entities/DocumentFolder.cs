using System;

namespace FlexiRent.Domain.Entities;

public class DocumentFolder
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Owner { get; set; } = null!;
    public DocumentFolder? ParentFolder { get; set; }
    public ICollection<DocumentFolder> SubFolders { get; set; } = new List<DocumentFolder>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}