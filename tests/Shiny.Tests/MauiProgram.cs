using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Shiny.Tests;


public static class MauiProgram
{
    public static IConfiguration Configuration { get; private set; } = null!;


    public static MauiApp CreateMauiApp()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        // Configuration = new ConfigurationBuilder()
        //     .AddJsonPlatformBundle(optional: false)
        //     .Build();

        var builder = MauiApp
            .CreateBuilder()
            .ConfigureLifecycleEvents(lc =>
            {
#if ANDROID
                lc.AddAndroid(x => x
                    .OnCreate((_, _) => DeviceDisplay.KeepScreenOn = true)
                );
#else
                DeviceDisplay.KeepScreenOn = true;
#endif
            })
            .ConfigureUITesting()
            .UseVisualTestRunner(conf => conf
                .AddXunit()
                .AddConsoleResultChannel()
                .AddTestAssembly(typeof(MauiProgram).Assembly)
#if MODE_NON_INTERACTIVE_VISUAL
				.EnableAutoStart(true)
				.AddTcpResultChannel(new TcpResultChannelOptions
				{
					HostNames = ["localhost", "10.0.2.2"],
					Port = 16384,
					Formatter = new TextResultChannelFormatter(),
					Required = false,
					Retries = 3,
					RetryTimeout = TimeSpan.FromSeconds(5),
					Timeout = TimeSpan.FromSeconds(30)
				})
#endif
            );
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        
            // .ConfigureTests(new TestOptions
            // {
            //     Assemblies =
            //     {
            //         typeof(MauiProgram).Assembly
            //     }
            // })
            // .UseShiny() // this is somewhat of a hack as it hooks the shiny events BUT to the current host provider
}