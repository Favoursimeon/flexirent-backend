using FlexiRent.Infrastructure.Repositories;
using FlexiRent.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FlexiRent.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Generic repository registration for DI
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Domain specific services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRatingService, RatingService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
