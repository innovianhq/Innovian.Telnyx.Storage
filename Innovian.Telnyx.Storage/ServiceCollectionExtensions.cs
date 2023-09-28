//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using Innovian.Telnyx.Storage.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Innovian.Telnyx.Storage;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Telnyx storage service with dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddTelnyxClient(this IServiceCollection services,
        Action<TelnyxClientOptions>? setupAction = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (setupAction == null)
            throw new Exception("The storage API key is required to interact with the Telnyx API");

        services.AddOptions();
        services.Configure(setupAction);

        return services
            .AddHttpClient()
            .AddSingleton<TelnyxStorageService>();
    }
}