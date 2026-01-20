using Microsoft.Extensions.DependencyInjection;

namespace FlexiRent.Infrastructure
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext db, IServiceProvider services)
        {
            // Add seed data if necessary
            await Task.CompletedTask;
        }
    }
}
