using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AppRole Role { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}