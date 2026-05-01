using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserVerification> UserVerifications => Set<UserVerification>();

    // Profiles
    public DbSet<Profile> Profiles => Set<Profile>();

    // Properties
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Bookings
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingMessage> BookingMessages => Set<BookingMessage>();
    public DbSet<ViewingSchedule> ViewingSchedules => Set<ViewingSchedule>();
    public DbSet<ProviderAvailability> ProviderAvailabilities => Set<ProviderAvailability>();

    // Documents
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentFolder> DocumentFolders => Set<DocumentFolder>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<SharedDocument> SharedDocuments => Set<SharedDocument>();

    // Payments
    public DbSet<RentalLease> RentalLeases => Set<RentalLease>();
    public DbSet<RentalPayment> RentalPayments => Set<RentalPayment>();
    public DbSet<PaymentSchedule> PaymentSchedules => Set<PaymentSchedule>();
    public DbSet<PaymentAccount> PaymentAccounts => Set<PaymentAccount>();

    //Portfolio Images
    public DbSet<PortfolioImage> PortfolioImages => Set<PortfolioImage>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    // Admin
 
    public DbSet<Currency> Currencies => Set<Currency>();

    //Reviews
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewVote> ReviewVotes => Set<ReviewVote>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

    }
}