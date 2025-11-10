using System;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shiny;


public abstract class ShinyBroadcastReceiver : BroadcastReceiver
{
    protected T Resolve<T>() => ShinyHost.ServiceProvider.GetRequiredService<T>()!;
    protected virtual void LogError<T>(Exception exception, string message)
        => ShinyHost.LoggingFactory.CreateLogger<T>().LogError(exception, message);


    protected abstract Task OnReceiveAsync(Context? context, Intent? intent);
    public override void OnReceive(Context? context, Intent? intent)
    {
        var pendingResult = this.GoAsync();
        this.OnReceiveAsync(context, intent).ContinueWith(x =>
        {
            if (x.IsFaulted)
            {
                this.LogError<ShinyBroadcastReceiver>(x.Exception!, "Error in broadcast receiver");
            }
            pendingResult!.Finish();
        });
    }
}
