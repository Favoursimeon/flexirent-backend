namespace FlexiRent.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile? Profile { get; set; }
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}