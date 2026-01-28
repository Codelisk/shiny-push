using System;
using Foundation;
using Microsoft.Extensions.Hosting;
using Shiny.Hosting.Native;
using UIKit;

namespace Shiny;


public abstract class ShinyAppDelegate : UIApplicationDelegate
{
    /// <summary>
    /// Wireup all of your dependencies here
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected abstract IHost CreateHost(IHostApplicationBuilder builder);

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // TODO: I need to pass launchOptions for event
        var builder = new ShinyHostApplicationBuilder();
        var host = this.CreateHost(builder);
        IosShinyHost.Init(host.Services);

        return base.FinishedLaunching(application, launchOptions);
    }

    public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        => IosShinyHost.OnContinueUserActivity(userActivity,  completionHandler);

    public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
        => IosShinyHost.OnHandleEventsForBackgroundUrl(sessionIdentifier, completionHandler);

    public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        => IosShinyHost.OnRegisteredForRemoteNotifications(deviceToken);

    public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        => IosShinyHost.OnFailedToRegisterForRemoteNotifications(error);

    public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        => IosShinyHost.OnDidReceiveRemoteNotification(userInfo, completionHandler);
}