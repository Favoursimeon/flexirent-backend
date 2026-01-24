// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
//
// namespace FlexiRent.Domain.Entities
// {
//     // Core user & auth
//     public class User
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         [Required, MaxLength(255)] public string Email { get; set; }
//         [Required] public string PasswordHash { get; set; }
//         public bool EmailConfirmed { get; set; }
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//
//         public Profile Profile { get; set; }
//         public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
//     }
//
//     public class UserRole
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         [Required] public string Role { get; set; } // Admin, Client, Vendor, ServiceProvider
//         [Required] public Guid UserId { get; set; }
//         public User User { get; set; }
//     }
//
//     public class RefreshToken
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public string Token { get; set; }
//         public Guid UserId { get; set; }
//         public DateTime ExpiresAt { get; set; }
//         public bool Revoked { get; set; }
//     }
//
//     // Registrations
//     public class ServiceProviderRegistration
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid ApplicantUserId { get; set; }
//         public string ServiceType { get; set; }
//         public string Details { get; set; }
//         public string Status { get; set; } = "pending"; // pending/approved/rejected
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     public class VendorRegistration
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid ApplicantUserId { get; set; }
//         public string BusinessName { get; set; }
//         public string Details { get; set; }
//         public string Status { get; set; } = "pending";
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     // Properties
//     public class Property
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid OwnerId { get; set; }
//         public string Title { get; set; }
//         public string Description { get; set; }
//         public decimal PricePerMonth { get; set; }
//         public bool IsAvailable { get; set; } = true;
//         public string Address { get; set; }
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     public class ApprovedServiceProvider
//     {
//         [Key] public Guid Id { get; set; }
//         public Guid UserId { get; set; }
//         public string ServiceType { get; set; }
//     }
//
//     public class ApprovedVendor
//     {
//         [Key] public Guid Id { get; set; }
//         public Guid UserId { get; set; }
//         public string BusinessName { get; set; }
//     }
//
//     public class VendorProduct
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid VendorId { get; set; }
//         public string Name { get; set; }
//         public string Description { get; set; }
//         public decimal Price { get; set; }
//     }
//
//     // Profile and preferences
//     public class Profile
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid UserId { get; set; }
//         public string FullName { get; set; }
//         public string PhoneNumber { get; set; }
//         public string AvatarUrl { get; set; }
//         public User User { get; set; }
//     }
//
//     public class UserVerification
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid UserId { get; set; }
//         public bool IsVerified { get; set; } = false;
//         public string VerificationToken { get; set; }
//         public DateTime? VerifiedAt { get; set; }
//     }
//
//     public class UserPreference
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid UserId { get; set; }
//         public string PreferencesJson { get; set; } // small JSON blob
//     }
//
//     // Bookings
//     public class Booking
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid UserId { get; set; }
//         public Guid ProviderId { get; set; }
//         public Guid PropertyId { get; set; }
//         public DateTime StartAt { get; set; }
//         public DateTime EndAt { get; set; }
//         public string Status { get; set; } = "pending"; // pending/confirmed/completed/cancelled
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     public class BookingRequest
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid BookingId { get; set; }
//         public DateTime RequestedStart { get; set; }
//         public DateTime RequestedEnd { get; set; }
//         public string Status { get; set; } = "pending";
//     }
//
//     public class Wishlist
//     {
//         public Guid UserId { get; set; }
//         public Guid TargetId { get; set; } // property or product
//         public string TargetType { get; set; }
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     public class RentalLease
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid PropertyId { get; set; }
//         public Guid TenantId { get; set; }
//         public DateTime StartAt { get; set; }
//         public DateTime EndAt { get; set; }
//         public string Terms { get; set; }
//     }
//
//     public class RentalPayment
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid LeaseId { get; set; }
//         public decimal Amount { get; set; }
//         public DateTime PaidAt { get; set; } = DateTime.UtcNow;
//         public string PaymentMethod { get; set; }
//     }
//
//     public class Review
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public string TargetType { get; set; } // vendor | service_provider | property
//         public Guid TargetId { get; set; }
//         public Guid AuthorId { get; set; }
//         public int Rating { get; set; } // 1-5
//         public string Comment { get; set; }
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//     }
//
//     public class Message
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid BookingId { get; set; }
//         public Guid SenderId { get; set; }
//         public string Body { get; set; }
//         public DateTime SentAt { get; set; } = DateTime.UtcNow;
//         public bool IsRead { get; set; } = false;
//     }
//
//     public class DocumentFolder
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid OwnerId { get; set; }
//         public string Name { get; set; }
//     }
//
//     public class DocumentVersion
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid DocumentId { get; set; }
//         public string FilePath { get; set; }
//         public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
//         public Guid UploadedBy { get; set; }
//     }
//
//     public class Document
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid FolderId { get; set; }
//         public string FileName { get; set; }
//         public string ContentType { get; set; }
//         public long Size { get; set; }
//     }
//
//     public class PortfolioImage
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid ServiceProviderId { get; set; }
//         public string Url { get; set; }
//     }
//
//     public class AccountDeletionRequest
//     {
//         [Key] public Guid Id { get; set; } = Guid.NewGuid();
//         public Guid UserId { get; set; }
//         public string Reason { get; set; }
//         public string Status { get; set; } = "pending";
//         public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
//     }
// }
