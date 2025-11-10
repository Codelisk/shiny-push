using System;
using Microsoft.Extensions.Logging;

namespace Shiny;


public static class ShinyHost
{
    // TODO: I do need an appdata path here for ios, android, & windows
    // TODO: need foreground/background app status for ios, android, & windows
    // TODO: need mainthread invoker? 
    
    public static void Init(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        // //Current = new Host(serviceProvider, loggerFactory);
        //
        // var tasks = this.Services.GetServices<IShinyStartupTask>();
        // //var logger = this.Logging.CreateLogger<Host>();
        // var logger = this.Services.GetRequiredService<ILogger<Host>>();
        //
        // foreach (var task in tasks)
        // {
        //     var tn = task.GetType().FullName;
        //     logger.LogDebug($"Startup task '{tn}' ran successfully");
        //     task.Start();
        // }
        // Host.Current = this;
    }
    
    
    public static bool IsInitialized => false;
    public static IServiceProvider ServiceProvider { get; private set; }
    public static ILoggerFactory LoggingFactory { get; private set; }
}