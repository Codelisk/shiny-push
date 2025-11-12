using System;
using Android.App;
using Android.OS;
using AndroidX.Lifecycle;
using Java.Interop;

namespace Shiny;


public class AndroidLifecycleManager : Java.Lang.Object, Application.IActivityLifecycleCallbacks, ILifecycleObserver, IDisposable
{
    readonly Application app;
    readonly IServiceProvider services;
    
    public AndroidLifecycleManager(Application app, IServiceProvider services)
    {
        this.app = app;
        this.services = services;
        
        this.app.RegisterActivityLifecycleCallbacks(this);
        ProcessLifecycleOwner.Get().Lifecycle.AddObserver(this);
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.app.UnregisterActivityLifecycleCallbacks(this);
            ProcessLifecycleOwner.Get().Lifecycle.RemoveObserver(this);
        }   
    }


    [Lifecycle.Event.OnResume]
    [Export]
    public void OnResume()
    {
    }
    //=> this.Execute(this.appHandlers, x => x.OnForeground());


    [Lifecycle.Event.OnPause]
    [Export]
    public void OnPause()
    {
        
    }
    
    //=> this.Execute(this.appHandlers, x => x.OnBackground());
    
    // public Subject<ActivityChanged> ActivitySubject { get; } = new();
    readonly WeakReference<Activity?> current = new(null);


    public Activity? Activity
    {
        get => this.current.TryGetTarget(out var a) ? a : null;
        private set => this.current.SetTarget(value);
    }


    void Fire(Activity activity, ActivityState state, Bundle? bundle = null)
    {
        
    }
//        => this.ActivitySubject.OnNext(new ActivityChanged(activity, state, bundle));


    public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
    {
        this.Activity = activity;
        this.Fire(activity, ActivityState.Created, savedInstanceState);
    }


    public void OnActivityPaused(Activity activity)
    {
        this.Activity = activity;
        this.Fire(activity, ActivityState.Paused);
    }


    public void OnActivityResumed(Activity activity)
    {
        this.Activity = activity;
        this.Fire(activity, ActivityState.Resumed);
    }


    public void OnActivityDestroyed(Activity activity)
        => this.Fire(activity, ActivityState.Destroyed);


    public void OnActivitySaveInstanceState(Activity activity, Bundle outState) => this.Fire(activity, ActivityState.SaveInstanceState, outState);
    public void OnActivityStarted(Activity activity) => this.Fire(activity, ActivityState.Started);
    public void OnActivityStopped(Activity activity) => this.Fire(activity, ActivityState.Stopped);
    
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
    
}