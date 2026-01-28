using System;
using Android.App;
using Android.OS;
using AndroidX.Lifecycle;
using Java.Interop;

namespace Shiny;


public class AndroidLifecycleManager : Java.Lang.Object, Application.IActivityLifecycleCallbacks, ILifecycleObserver, IDisposable
{
    readonly Application app;
    readonly Action<bool> appLifecycleChanged;
    readonly Action<ActivityChanged> activityChanged;
    
    public AndroidLifecycleManager(
        Application app, 
        Action<bool> onAppLifecycleChanged,
        Action<ActivityChanged> onActivityChanged
    )
    {
        this.app = app;
        this.appLifecycleChanged = onAppLifecycleChanged;
        this.activityChanged = onActivityChanged;
        
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
    public void OnResume() => this.appLifecycleChanged(true);


    [Lifecycle.Event.OnPause]
    [Export]
    public void OnPause() => this.appLifecycleChanged(false);
    
    readonly WeakReference<Activity?> current = new(null);


    public Activity? Activity
    {
        get => this.current.TryGetTarget(out var a) ? a : null;
        private set => this.current.SetTarget(value);
    }

    /// <summary>
    /// Manually sets the current activity. Used when Init is called after the activity was already created.
    /// </summary>
    public void SetCurrentActivity(Activity activity) => this.Activity = activity;


    void Fire(Activity activity, ActivityState state, Bundle? bundle = null) => this.activityChanged.Invoke(new(activity, state, bundle));


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
}