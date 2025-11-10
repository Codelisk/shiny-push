using System;
using Foundation;
using UIKit;

namespace Shiny;


public abstract class ShinyAppDelegate : UIApplicationDelegate
{


    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        //this.CreateShinyHost().Run();
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