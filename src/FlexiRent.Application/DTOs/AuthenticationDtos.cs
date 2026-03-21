using System.ComponentModel.DataAnnotations;
using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MinLength(6)] public string Password { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        public AppRole Role { get; set; } = AppRole.User;
    }

    public class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Password { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class RefreshRequest
    {
        [Required] public string RefreshToken { get; set; }
    }
}
