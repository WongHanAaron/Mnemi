using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Features.PinnedDecks;
using Mnemi.Application.Ports;

namespace Mnemi.Application;

/// <summary>
/// Extension methods for registering Application layer services with DI.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services.
    /// Host projects must register concrete implementations of
    /// service interfaces (e.g., IHomeDashboardService) themselves.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Application-layer services
        services.AddSingleton<IGetPinnedDecks, GetPinnedDecksHandler>();

        return services;
    }
}
