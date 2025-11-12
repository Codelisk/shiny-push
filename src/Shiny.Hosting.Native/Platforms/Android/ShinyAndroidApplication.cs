using System;
using Android.Runtime;
using Microsoft.Extensions.Hosting;
using Shiny.Hosting.Native;

namespace Shiny;


public abstract class ShinyAndroidApplication : Android.App.Application
{
    protected ShinyAndroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {}
    
    // TODO: user needs a builder with configuration, services, & logging - IHostApplication or something back?

    /// <summary>
    /// Wireup all of your dependencies here
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected abstract IHost CreateHost(IHostApplicationBuilder builder);
    
    
    public override void OnCreate()
    {
        base.OnCreate();
        
        var builder = new ShinyHostApplicationBuilder();
        var host = this.CreateHost(builder);
        
        // TODO
        AndroidShinyHost.Init(this, host.Services);
    }
}