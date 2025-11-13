using System;
using System.IO;
using System.Linq;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Shiny;


public static class IosShinyHost
{
    public static void Init(IServiceProvider serviceProvider)
    {
        ShinyHost.Init(serviceProvider);
        AppData = new DirectoryInfo(NSSearchPath.GetDirectories(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.User).First());
    }
    
    
    public static DirectoryInfo AppData { get; private set; }
    
    public static bool IsSimulator =>
#if MACCATALYST || MACOS
	    false;
#else
        ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;
#endif

    public static void OnRegisteredForRemoteNotifications(NSData deviceToken)
        => Execute<IIosLifecycle.IRemoteNotifications>(x => x.OnRegistered(deviceToken));
   
    public static void OnFailedToRegisterForRemoteNotifications(NSError error)
        => Execute<IIosLifecycle.IRemoteNotifications>(x => x.OnFailedToRegister(error));

    public static void OnDidReceiveRemoteNotification(NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        => Execute<IIosLifecycle.IRemoteNotifications>(x => x.OnDidReceive(userInfo, completionHandler));
    public static bool OnContinueUserActivity(NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
        => HandleExecute<IIosLifecycle.IContinueActivity>(x => x.Handle(userActivity, completionHandler));

    public static bool OnHandleEventsForBackgroundUrl(string sessionIdentifier, Action completionHandler)
        => HandleExecute<IIosLifecycle.IHandleEventsForBackgroundUrl>(x => x.Handle(sessionIdentifier, completionHandler));

    static bool HandleExecute<T>(Func<T, bool> func)
    {
        var services = ShinyHost.ServiceProvider.GetServices<T>();
        
        foreach (var handler in services)
        {
            try
            {
                if (func(handler))
                {
                    // this.logger.LifecycleInfo(handler!.GetType().FullName!, typeof(T).FullName!);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // this.logger.LifecycleError(ex, handler!.GetType().FullName!, typeof(T).FullName!);
            }
        }
        return false;
    }


    static void Execute<T>(Action<T> action)
    {
        var services = ShinyHost.ServiceProvider.GetServices<T>();
        
        foreach (var handler in services)
        {
            try
            {
                // this.logger.LifecycleInfo(handler!.GetType().FullName!, typeof(T).FullName!);
                action(handler);
            }
            catch (Exception ex)
            {
                // this.logger.LifecycleError(ex, handler!.GetType().FullName!, typeof(T).FullName!);
            }
        }
    }
}

/*
@ -1,186 +0,0 @@
   ﻿using System;
   using System.Linq;
   using System.Collections.Generic;
   using System.Reactive.Disposables;
   using Foundation;
   using Microsoft.Extensions.Logging;
   using UIKit;
   using UserNotifications;
   
   namespace Shiny.Hosting;
   
   
   public class IosLifecycleExecutor : IShinyStartupTask, IDisposable
   {
       readonly ILogger logger;
       readonly IEnumerable<IIosLifecycle.IApplicationLifecycle> appHandlers;
       readonly IEnumerable<IIosLifecycle.IOnFinishedLaunching> finishLaunchingHandlers;
       readonly IEnumerable<IIosLifecycle.IContinueActivity> activityHandlers;
       readonly IEnumerable<IIosLifecycle.IHandleEventsForBackgroundUrl> bgUrlHandlers;
       readonly IEnumerable<IIosLifecycle.IRemoteNotifications> remoteHandlers;
       readonly IEnumerable<IIosLifecycle.INotificationHandler> notificationHandlers;
       readonly CompositeDisposable disposer = new();
   
       public IosLifecycleExecutor(
           ILogger<IosLifecycleExecutor> logger,
           IEnumerable<IIosLifecycle.IApplicationLifecycle> appHandlers,
           IEnumerable<IIosLifecycle.IOnFinishedLaunching> finishLaunchingHandlers,
           IEnumerable<IIosLifecycle.IHandleEventsForBackgroundUrl> bgUrlHandlers,
           IEnumerable<IIosLifecycle.IContinueActivity> activityHandlers,
           IEnumerable<IIosLifecycle.IRemoteNotifications> remoteHandlers,
           IEnumerable<IIosLifecycle.INotificationHandler> notificationHandlers
       )
       {
           this.logger = logger;
           this.appHandlers = appHandlers;
           this.finishLaunchingHandlers = finishLaunchingHandlers;
           this.bgUrlHandlers = bgUrlHandlers;
           this.remoteHandlers = remoteHandlers;
           this.activityHandlers = activityHandlers;
           this.notificationHandlers = notificationHandlers;
       }
   
   
       public void Start()
       {
           UIApplication
               .Notifications
               .ObserveDidFinishLaunching((_, args) =>
                   this.Execute(this.finishLaunchingHandlers, handler => handler.Handle(args))
               )
               .DisposedBy(this.disposer);
   
           UIApplication
               .Notifications
               .ObserveWillEnterForeground((_, _) => this.Execute(this.appHandlers, x => x.OnForeground()))
               .DisposedBy(this.disposer);
   
           UIApplication
               .Notifications
               .ObserveDidEnterBackground((_, _) => this.Execute(this.appHandlers, x => x.OnBackground()))
               .DisposedBy(this.disposer);
   
   
           if (this.notificationHandlers != null && this.notificationHandlers.Any())
           {
               if (UNUserNotificationCenter.Current.Delegate != null)
               {
                   this.logger.LogWarning("UNUserNotificationCenter is already set.  Shiny will not be able to run its notification delegates");
               }
               else
               {
                   UNUserNotificationCenter.Current.Delegate = new ShinyUNUserNotificationCenterDelegate(
                       (response, completionHandler) => this.Execute(this.notificationHandlers, x => x.OnDidReceiveNotificationResponse(response, completionHandler)),
                       (notification, completionHandler) => this.Execute(this.notificationHandlers, x => x.OnWillPresentNotification(notification, completionHandler))
                   );
               }
           }
       }
   
       
   
   
       public void Dispose()
       {
           this.disposer.Dispose();
           if (UNUserNotificationCenter.Current.Delegate is ShinyUNUserNotificationCenterDelegate)
               UNUserNotificationCenter.Current.Delegate = null;
       }
   
   
       //public static bool ShinyHandleEventsForBackgroundUrl(this IUIApplicationDelegate _, string sessionUrl, Action completionHandler)
       //{
       //    var lifecycles = Host.Current.ServiceProvider.GetServices<IosLifecycle.IHandleEventsForBackgroundUrl>();
       //    var logger = Host.Current.Logging.CreateLogger<IIosLifecycle>();
   
       //    foreach (var lc in lifecycles)
       //    {
       //        try
       //        {
       //            // TODO: I'll need to pass in the completionhandler
       //            if (lc.Handle(sessionUrl))
       //                // TODO: handled, break loop and log
       //                return true;
       //        }
       //        catch (Exception ex)
       //        {
       //            logger.LogError("Failed to execute lifecycle", ex);
       //        }
       //    }
       //    return false;
       //}
   
   
       //public static bool ShinyContinueActivity(NSUserActivity activity, UIApplicationRestorationHandler handler)
       //{
       //    var lifecycles = Host.Current.ServiceProvider.GetServices<IIosLifecycle.IContinueActivity>();
       //    var logger = Host.Current.Logging.CreateLogger<IosLifecycle>();
   
       //    foreach (var lc in lifecycles)
       //    {
       //        try
       //        {
       //            // TODO: must pass in handler
       //            if (lc.Handle(activity))
       //                // TODO: handled, break loop and log
       //                return true;
       //        }
       //        catch (Exception ex)
       //        {
       //            logger.LogError("Failed to execute lifecycle", ex);
       //        }
       //    }
       //    return false;
       //}
   }
 */