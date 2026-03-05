using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ServiceProviderRegistration> ServiceProviderRegistrations { get; set; }
        public DbSet<VendorRegistration> VendorRegistrations { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ApprovedServiceProvider> ApprovedServiceProviders { get; set; }
        public DbSet<ApprovedVendor> ApprovedVendors { get; set; }
        public DbSet<VendorProduct> VendorProducts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<UserVerification> UserVerifications { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingRequest> BookingRequests { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<RentalLease> RentalLeases { get; set; }
        public DbSet<RentalPayment> RentalPayments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<DocumentFolder> DocumentFolders { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<PortfolioImage> PortfolioImages { get; set; }
        public DbSet<AccountDeletionRequest> AccountDeletionRequests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indexes, keys, relationships, and soft-delete patterns
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Property>().HasIndex(p => p.IsAvailable);
            modelBuilder.Entity<Booking>().HasIndex(b => b.Status);
            modelBuilder.Entity<Review>().HasIndex(r => new { r.TargetType, r.TargetId });

            // Example relationships
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite keys & constraints
            modelBuilder.Entity<Wishlist>().HasKey(w => new { w.UserId, w.TargetId });

            // Views: ApprovedServiceProviders / ApprovedVendors - map to tables to support queries (separable)
            modelBuilder.Entity<ApprovedServiceProvider>().ToTable("ApprovedServiceProviders");
            modelBuilder.Entity<ApprovedVendor>().ToTable("ApprovedVendors");

            base.OnModelCreating(modelBuilder);
        }
    }
}
