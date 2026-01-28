using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Activity = Android.App.Activity;

namespace Shiny;


public static class AndroidShinyHost
{
    static ILogger logger = null!;
    
    public static void Init(Android.App.Application app, IServiceProvider serviceProvider, Activity? currentActivity = null)
    {
        ShinyHost.Init(serviceProvider);
        AppContext = app;
        AppData = new DirectoryInfo(app.FilesDir!.AbsolutePath);
        logger = serviceProvider.GetRequiredService<ILogger<AndroidPlatform>>();

        if (lifecycleManager == null)
        {
            lifecycleManager = new AndroidLifecycleManager(
                app,
                foreground => { },
            // await serviceProvider
            //             .RunDelegates<IApplicationLifecycle>(x =>
            //             {
            //                 if (foreground)
            //                     x.OnForeground();
            //                 else
            //                     x.OnBackground();
            //
            //                 return Task.CompletedTask;
            //             }, logger)
                 activityChanged =>
                {
                    switch (activityChanged.State)
                    {
                        case ActivityState.Created:
                            // await serviceProvider
                            //     .RunDelegates<IAndroidLifecycle.IOnActivityOnCreate>(
                            //         x => x.ActivityOnCreate(activityChanged.Activity, activityChanged.StateBundle),
                            //        logger
                            //     )
                            //     .ConfigureAwait(false);
                            break;
                    }
                }
            );
        }

        // Set the current activity if provided (for when Init is called after activity creation)
        if (currentActivity != null)
        {
            lifecycleManager.SetCurrentActivity(currentActivity);
        }
    }


    static AndroidLifecycleManager? lifecycleManager;
    
    public static Android.App.Application AppContext
    {
        get
        {
            if (field == null)
                throw new InvalidOperationException("You must call AndroidShinyHost.Init in your Application class before accessing the AppContext");
            
            return field;
        }
        private set;
    }


    public static DirectoryInfo AppData
    {
        get
        {
            if (field == null)
                throw new InvalidOperationException("You must call AndroidShinyHost.Init in your Application class before accessing the AppData");
            
            return field;
        }
        private set;
    }
    
    public static Activity? CurrentActivity => lifecycleManager?.Activity;
   
    
    public static void OnActivityOnCreate(Activity activity, Bundle? savedInstanceState)
         => Execute<IAndroidLifecycle.IOnActivityOnCreate>(x => x.ActivityOnCreate(activity, savedInstanceState));

     public static void OnRequestPermissionsResult(Activity activity, int requestCode, string[] permissions, Permission[] grantResults)
         => Execute<IAndroidLifecycle.IOnActivityRequestPermissionsResult>(x => x.Handle(activity, requestCode, permissions, grantResults));

     public static void OnNewIntent(Activity activity, Intent? intent)
         => Execute<IAndroidLifecycle.IOnActivityNewIntent>(x => x.Handle(activity, intent));

     public static void OnActivityResult(Activity activity, int requestCode, Android.App.Result result, Intent? intent)
         => Execute<IAndroidLifecycle.IOnActivityResult>(x => x.Handle(activity, requestCode, result, intent));
    
     static void Execute<T>(Action<T> action)
     {
         // Skip if Shiny hasn't been initialized yet
         if (!ShinyHost.IsInitialized)
             return;

         var services = ShinyHost.ServiceProvider.GetServices<T>();
         var logger = ShinyHost.ServiceProvider.GetService<ILogger<AndroidPlatform>>();

         foreach (var handler in services)
         {
             try
             {
                 action(handler);
             }
             catch (Exception ex)
             {
                 logger?.LogError(ex, "Failed to execute lifecycle call");
             }
         }
     }
     
    public static T GetSystemService<T>(string key) where T : Java.Lang.Object
        => (T)AppContext.GetSystemService(key);

    
    public static AccessState GetCurrentPermissionStatus(string androidPermission)
     {
         var self = ContextCompat.CheckSelfPermission(AppContext, androidPermission);
         if (self == Permission.Granted)
             return AccessState.Available;

         // if (!this.HasRequestedPermission(androidPermission))
         //     return AccessState.Unknown;

         //var showRequest = ActivityCompat.ShouldShowRequestPermissionRationale(this.CurrentActivity!, androidPermission);
         //if (showRequest)
         //    return AccessState.Unknown;

         return AccessState.Denied;
     }


    public static void RegisterBroadcastReceiver<T>(bool exported, params string[] actions) where T : BroadcastReceiver, new()
    {
        var receiver = new T();
        var filter = new IntentFilter();
        foreach (var e in actions)
            filter.AddAction(e);

        if (OperatingSystem.IsAndroidVersionAtLeast(34))
        {
            var flags = exported ? ReceiverFlags.Exported : ReceiverFlags.NotExported;
            AppContext.RegisterReceiver(receiver, filter, flags);
        }
        else
        {
            AppContext.RegisterReceiver(new T(), filter);
        }
    }
}

//     public IObservable<ActivityChanged> WhenActivityStatusChanged() => Observable.Create<ActivityChanged>(ob =>
//     {
//         if (this.CurrentActivity != null)
//             ob.Respond(new ActivityChanged(this.CurrentActivity, ActivityState.Created, null));
//
//         return activityLifecycle
//             .ActivitySubject
//             .Subscribe(x => ob.Respond(x));
//     });
//
//
//     public async Task<AccessState> RequestForegroundServicePermissions()
//     {
//         if (OperatingSystem.IsAndroidVersionAtLeast(33))
//         {
//             var results = await this.RequestPermissions(
//                 Manifest.Permission.ForegroundService,
//                 Manifest.Permission.PostNotifications
//             );
//             if (results.IsSuccess())
//                 return AccessState.Available;
//
//             if (!results.IsGranted(Manifest.Permission.ForegroundService))
//                 return AccessState.NotSetup;
//
//             return AccessState.Restricted; // no post_notifications
//         }
//         else if (OperatingSystem.IsAndroidVersionAtLeast(31))
//         {
//             var results = await this.RequestPermissions(Manifest.Permission.ForegroundService);
//             if (results.IsSuccess())
//                 return AccessState.Available;
//
//             return AccessState.NotSetup;
//         }
//
//         return AccessState.Available;
//     }
//
//     public const string ActionServiceStart = "ACTION_START_FOREGROUND_SERVICE";
//     public const string ActionServiceStop = "ACTION_STOP_FOREGROUND_SERVICE";
//     public const string IntentActionStopWithTask = "StopWithTask";
//
//     public void StartService(Type serviceType, bool stopWithTask = true)
//     {
//         var intent = new Intent(this.AppContext, serviceType);
//         intent.SetAction(ActionServiceStart);
//         intent.PutExtra(IntentActionStopWithTask, stopWithTask);
//
//         if (OperatingSystemShim.IsAndroidVersionAtLeast(31))
//             this.AppContext.StartForegroundService(intent);
//         else
//             this.AppContext.StartService(intent);
//     }
//
//
//     public void StopService(Type serviceType)
//     {
//         var intent = new Intent(this.AppContext, serviceType);
//         intent.SetAction(ActionServiceStop);
//         this.AppContext.StartService(intent);
//         //this.AppContext.StopService(intent);
//     }
//
//     public int GetDrawableByName(string name) => this
//         .AppContext
//         .Resources!
//         .GetIdentifier(
//             name,
//             "drawable",
//             this.AppContext.PackageName
//         );
//
//     public IObservable<AccessState> RequestAccess(string androidPermissions)
//         => this.RequestPermissions(new[] { androidPermissions }).Select(x => x.IsSuccess() ? AccessState.Available : AccessState.Denied);
//
//
//     public IObservable<PermissionRequestResult> RequestPermissions(params string[] androidPermissions) => Observable.Create<PermissionRequestResult>(ob =>
//     {
//         var comp = new CompositeDisposable();
//
//         //https://developer.android.com/training/permissions/requesting
//         var allGood = androidPermissions.All(p => ContextCompat.CheckSelfPermission(this.AppContext, p) == Permission.Granted);
//         if (allGood)
//         {
//             // everything is already good
//             var grants = Enumerable.Repeat(Permission.Granted, androidPermissions.Length).ToArray();
//             ob.Respond(new PermissionRequestResult(0, androidPermissions, grants));
//         }
//         else
//         {
//             //if (this.Status == PlatformState.Background)
//             //    throw new ApplicationException("You cannot make permission requests while your application is in the background.  Please call RequestAccess in the Shiny library you are using while your app is in the foreground so your user can respond.  You are getting this message because your user has either not granted these permissions or has removed them.");
//             this.SetRequestedPermissions(androidPermissions);
//             var current = Interlocked.Increment(ref this.requestCode);
//             comp.Add(this
//                 .permissionSubject
//                 .Where(x => x.RequestCode == current)
//                 .Subscribe(x => ob.Respond(x))
//             );
//
//             comp.Add(this
//                 .WhenActivityStatusChanged()
//                 .Take(1)
//                 .Timeout(TimeSpan.FromSeconds(5))
//                 .Subscribe(
//                     x => ActivityCompat.RequestPermissions(
//                         x.Activity,
//                         androidPermissions,
//                         current
//                     ),
//                     ex => ob.OnError(new TimeoutException(
//                         "A current activity was not detected to be able to request permissions",
//                         ex
//                     ))
//                 )
//             );
//         }
//
//         return comp;
//     });
//
//     void SetRequestedPermissions(string[] androidPermissions)
//     {
//         lock (this.requestedPermissions)
//         {
//             var count = this.requestedPermissions.Count;
//             foreach (var p in androidPermissions)
//             {
//                 if (!this.requestedPermissions.Contains(p, StringComparer.InvariantCultureIgnoreCase))
//                     this.requestedPermissions.Add(p);
//             }
//             if (count != this.requestedPermissions.Count)
//                 this.store.Set(PermissionsKey, this.requestedPermissions);
//         }
//     }
//
//
//     bool HasRequestedPermission(string androidPermission)
//     {
//         lock (this.requestedPermissions)
//         {
//             return this.requestedPermissions.Contains(
//                 androidPermission,
//                 StringComparer.InvariantCultureIgnoreCase
//             );
//         }
//     }