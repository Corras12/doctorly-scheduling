using DoctorScheduling.Data;
using DoctorScheduling.Api.Middleware;
using DoctorScheduling.Services;
using DoctorScheduling.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduling.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

        return services;
    }

    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
