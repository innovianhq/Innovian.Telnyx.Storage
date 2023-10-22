//  -------------------------------------------------------------
//  Copyright (c) 2023 Innovian Corporation. All rights reserved.
//  -------------------------------------------------------------

using Innovian.Telnyx.Storage.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Innovian.Telnyx.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelynxClient(this IServiceCollection services)
    {
        services.Configure<TelnyxClientOptions>(options =>
        {
            //Set the API key with the values from IOptions<TelnyxClientOptiosn>
            options.TelnyxApiKey = options.Value.TelnyxApiKey;
        });

        return AddTelnyxClient(null);
    }

    /// <summary>
    /// Registers the Telnyx storage service with dependency injection.
    /// </summary>
    /// <param name="services">DI service collection instance.</param>
    /// <param name="setupAction">Provides a setup action for obtaining options.</param>
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
            .AddSingleton<ITelnyxStorageService, TelnyxStorageService>();
    }

    /// <summary>
    /// Registers the Telnyx storage service with dependency injection.
    /// </summary>
    /// <param name="services">DI service collection instance.</param>
    /// <param name="namedConfigurationSection">The named configuration section to pull the options from.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddTelnyxClient(this IServiceCollection services,
        IConfiguration namedConfigurationSection)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        services.Configure<TelnyxClientOptions>(namedConfigurationSection);

        return services
            .AddHttpClient()
            .AddSingleton<ITelnyxStorageService, TelnyxStorageService>();
    }
    
    /// <summary>
    /// Registers the Telnyx storage service with dependency injection.
    /// </summary>
    /// <param name="services">The dependency injection service collection.</param>
    /// <param name="options">An injected instance of <see cref="IOptions{TelnyxClientOptions}"/> to perform the configuration with.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddTelnyxClient(this IServiceCollection services,
        IOptions<TelnyxClientOptions> options)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddOptions<TelnyxClientOptions>()
            .Configure(opt =>
            {
                opt.TelnyxApiKey = options.Value.TelnyxApiKey;
            });

        return services
            .AddHttpClient()
            .AddSingleton<ITelnyxStorageService, TelnyxStorageService>();
    }
}