using System;

namespace FlexiRent.Domain.Entities;

public class SharedDocument
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid SharedByUserId { get; set; }
    public Guid SharedWithUserId { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDownload { get; set; }
    public DateTime SharedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation
    public Document Document { get; set; } = null!;
    public User SharedByUser { get; set; } = null!;
    public User SharedWithUser { get; set; } = null!;
}