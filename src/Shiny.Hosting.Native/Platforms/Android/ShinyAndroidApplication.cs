using System;
using Android.App;
using Android.Runtime;

namespace Shiny;


public abstract class ShinyAndroidApplication : Application
{
    protected ShinyAndroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}
    
    // TODO: user needs a builder with configuration, services, & logging - IHostApplication or something back?
    
    public override void OnCreate()
    {
        base.OnCreate();
        
        // TODO
        AndroidShinyHost.Init(this);
        //var host = this.CreateShinyHost();
        //host.Run();
    }
}