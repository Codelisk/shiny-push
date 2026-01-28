#if PLATFORM || __ANDROID__ || __IOS__ || __MACCATALYST__
using Shiny.Push;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shiny;


public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Native Push Notification services without any background handling
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPush(this IServiceCollection services)
    {
#if APPLE || __IOS__ || __MACCATALYST__
        services.TryAddSingleton<IPushManager, PushManager>();
#elif ANDROID || __ANDROID__
        services.AddPush(new FirebaseConfig());
#endif
        return services;
    }


    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPush<TDelegate>(this IServiceCollection services) where TDelegate : class, IPushDelegate
    {
        services.TryAddSingleton<IPushDelegate, TDelegate>();
        return services.AddPush();
    }

#if ANDROID || __ANDROID__

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddPush(this IServiceCollection services, FirebaseConfig config)
    {
        services.TryAddSingleton(config);
        services.TryAddSingleton<IPushManager, PushManager>();
        return services;
    }


    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddPush<TDelegate>(this IServiceCollection services, FirebaseConfig config)
        where TDelegate : class, IPushDelegate
    {
        services.TryAddSingleton<IPushDelegate, TDelegate>();
        services.AddPush(config);
        return services;
    }
#endif
}
#endif