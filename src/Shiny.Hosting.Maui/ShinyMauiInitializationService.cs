using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace Shiny;


public class ShinyMauiInitializationService : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        ShinyHost.Init(services, loggerFactory);
    }
}
