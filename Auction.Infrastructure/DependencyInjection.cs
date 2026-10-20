using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.BackgroundJobs;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Repositories;
using Auction.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auction.Infrastructure;

// Extension method that registers all Infrastructure services into the DI container.
// Called once from Program.cs: builder.Services.AddInfrastructure(builder.Configuration)
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AuctionDbContext with the SQL Server connection string.
        // The connection string comes from appsettings.json — never hardcoded here.
        services.AddDbContext<AuctionDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AuctionDbContext).Assembly.FullName)
            ));

        // --- Repositories ---
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IWatchListRepository, WatchListRepository>();

        // --- Services ---
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuctionService, AuctionService>();
        services.AddScoped<IBidService, BidService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWatchListService, WatchListService>();

        // --- Background Jobs ---
        services.AddScoped<AuctionLifecycleJob>();

        return services;
    }
}
