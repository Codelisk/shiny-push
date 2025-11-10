namespace Shiny;


public interface IAndroidForegroundServiceDelegate
{
    void Configure(AndroidX.Core.App.NotificationCompat.Builder builder);
}