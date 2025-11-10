using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Activity = Android.App.Activity;

namespace Shiny;


public static class AndroidShinyHost
{
    //         var app = (Application)Application.Context;
//         activityLifecycle ??= new(app);
//         this.AppContext = app;
//         this.AppData = new DirectoryInfo(this.AppContext.FilesDir.AbsolutePath);
// TODO: need top activity
    public static void Init(Application app)
    {
        AppContext = app;
        AppData = new DirectoryInfo(app.FilesDir!.AbsolutePath);
    }

    
    // TODO: getter error if not initialized
    public static Application AppContext { get; private set; }
    public static DirectoryInfo AppData { get; private set; }
   
    public static void OnActivityOnCreate(Activity activity, Bundle? savedInstanceState)
         => Execute<IAndroidLifecycle.IOnActivityOnCreate>(x => x.ActivityOnCreate(activity, savedInstanceState));

     public static void OnRequestPermissionsResult(Activity activity, int requestCode, string[] permissions, Permission[] grantResults)
         => Execute<IAndroidLifecycle.IOnActivityRequestPermissionsResult>(x => x.Handle(activity, requestCode, permissions, grantResults));

     public static void OnNewIntent(Activity activity, Intent? intent)
         => Execute<IAndroidLifecycle.IOnActivityNewIntent>(x => x.Handle(activity, intent));

     public static void OnActivityResult(Activity activity, int requestCode, Result result, Intent? intent)
         => Execute<IAndroidLifecycle.IOnActivityResult>(x => x.Handle(activity, requestCode, result, intent));
    
     static void Execute<T>(Action<T> action)
     {
         var services = ShinyHost.ServiceProvider.GetServices<T>();
         foreach (var handler in services)
         {
             try
             {
                 action(handler);
             }
             catch (Exception ex)
             {
                 //this.logger.LogError(ex, "Failed to execute lifecycle call");
             }
         }
     }
     
    //     public void Start()
//     {
//         // this is really only need for unit tests - it will passthrough under normal circumstances
//         this.platform.InvokeOnMainThread(() =>
//         {
//             try
//             {
//                 ProcessLifecycleOwner.Get().Lifecycle.AddObserver(this);
//             }
//             catch (Exception ex)
//             {
//                 this.logger.LogWarning(ex, "Could not attach lifecycle observer");
//             }
//         });
//     }
//
//
//     [Lifecycle.Event.OnResume]
//     [Export]
//     public void OnResume() => this.Execute(this.appHandlers, x => x.OnForeground());
//
//
//     [Lifecycle.Event.OnPause]
//     [Export]
//     public void OnPause() => this.Execute(this.appHandlers, x => x.OnBackground());
//
//     //[Lifecycle.Event.OnDestroy]
//     //[Export]
//     //public void OnDestroy()
//     //{
//     //    Console.WriteLine("LIFECYCLE: OnDestory");
//     //}
//

//
//     public new void Dispose()
//     {
//         // dispose is (should) only used by unit tests
//         // this is really only need for unit tests - it will passthrough under normal circumstances
//         this.platform.InvokeOnMainThread(() =>
//         {
//             try
//             {
//                 ProcessLifecycleOwner.Get().Lifecycle.RemoveObserver(this);
//             }
//             catch (Exception ex)
//             {
//                 this.logger.LogWarning(ex, "Could not remove lifecycle observer");
//             }
//         });
//         base.Dispose();
//     }
}