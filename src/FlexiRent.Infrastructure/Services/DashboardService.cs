using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services
{
    public interface IDashboardService
    {
        Task<object> GetAdminStatsAsync();
        Task<object> GetClientStatsAsync(Guid userId);
        Task<object> GetVendorStatsAsync(Guid userId);
        Task<object> GetProviderStatsAsync(Guid userId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;
        public DashboardService(ApplicationDbContext db) { _db = db; }

        public Task<object> GetAdminStatsAsync()
        {
            var stats = new {
                Users = _db.Users.Count(),
                Properties = _db.Properties.Count(),
                Bookings = _db.Bookings.Count()
            };
            return Task.FromResult<object>(stats);
        }

        public async Task<object> GetClientStatsAsync(Guid userId)
        {
            var bookings = await _db.Bookings.CountAsync(b => b.UserId == userId);
            return new { bookings };
        }
        public async Task<object> GetVendorStatsAsync(Guid userId)
        {
            var products = await _db.VendorProducts.CountAsync(v => v.VendorId == userId);
            return new { products };
        }
        public async Task<object> GetProviderStatsAsync(Guid userId)
        {
            var services = await _db.ServiceProviderRegistrations.CountAsync(s => s.ApplicantUserId == userId);
            return new { services };
        }
    }
}
