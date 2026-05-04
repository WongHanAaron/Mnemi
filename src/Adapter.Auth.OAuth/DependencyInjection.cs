using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Auth.OAuth;

/// <summary>
/// Extension methods for registering OAuth authentication services with DI.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers OAuth authentication services.
    /// </summary>
    public static IServiceCollection AddOAuthServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
        return services;
    }
}
