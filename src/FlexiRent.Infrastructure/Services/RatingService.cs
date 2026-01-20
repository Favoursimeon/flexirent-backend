using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services
{
    public interface IRatingService
    {
        Task<double> GetAverageAsync(string targetType, Guid targetId);
        Task<int> GetCountAsync(string targetType, Guid targetId);
    }

    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _db;
        public RatingService(ApplicationDbContext db) { _db = db; }

        public async Task<double> GetAverageAsync(string targetType, Guid targetId)
        {
            var avg = await _db.Reviews.Where(r => r.TargetType == targetType && r.TargetId == targetId)
                .Select(r => (double?)r.Rating).AverageAsync();
            return avg ?? 0;
        }

        public async Task<int> GetCountAsync(string targetType, Guid targetId)
        {
            return await _db.Reviews.CountAsync(r => r.TargetType == targetType && r.TargetId == targetId);
        }
    }
}
