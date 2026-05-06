using Microsoft.Extensions.DependencyInjection;
using Mnemi.Application.Ports;

namespace Mnemi.Adapter.Auth.OAuth;

/// <summary>
/// Extension methods for registering OAuth authentication services with DI.
/// </summary>
public static class OAuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers OAuth authentication services including token encryption.
    /// </summary>
    public static IServiceCollection AddOAuthServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenEncryptionService, TokenEncryptionService>();
        return services;
    }
}
