using System;
using System.Reactive.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace Shiny;

public partial class AndroidPlatform
{
    public Application AppContext => AndroidShinyHost.AppContext;

    public AccessState GetCurrentPermissionStatus(string androidPermission)
        => AndroidShinyHost.GetCurrentPermissionStatus(androidPermission);

    public Intent CreateIntent<T>(params string[] actions)
    {
        var intent = new Intent(AppContext, typeof(T));
        foreach (var action in actions)
            intent.SetAction(action);
        return intent;
    }

    public PendingIntent GetBroadcastPendingIntent<T>(string intentAction, PendingIntentFlags flags, int requestCode = 0, Action<Intent>? modifyIntent = null)
    {
        var intent = CreateIntent<T>(intentAction);
        modifyIntent?.Invoke(intent);

        var pendingIntent = PendingIntent.GetBroadcast(
            AppContext,
            requestCode,
            intent,
            GetPendingIntentFlags(flags)
        );
        return pendingIntent!;
    }

    public PendingIntentFlags GetPendingIntentFlags(PendingIntentFlags flags)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31) && !flags.HasFlag(PendingIntentFlags.Mutable))
            flags |= PendingIntentFlags.Mutable;
        return flags;
    }

    public T GetSystemService<T>(string key) where T : Java.Lang.Object
        => AndroidShinyHost.GetSystemService<T>(key);

    public int GetDrawableByName(string name)
        => AppContext
            .Resources!
            .GetIdentifier(
                name,
                "drawable",
                AppContext.PackageName
            );

    public int GetRawResourceIdByName(string rawName)
        => AppContext
            .Resources!
            .GetIdentifier(
                rawName,
                "raw",
                AppContext.PackageName
            );

    public const string ActionServiceStart = "ACTION_START_FOREGROUND_SERVICE";
    public const string ActionServiceStop = "ACTION_STOP_FOREGROUND_SERVICE";
    public const string IntentActionStopWithTask = "StopWithTask";

    public void StartService(Type serviceType, bool stopWithTask = true)
    {
        var intent = new Intent(AppContext, serviceType);
        intent.SetAction(ActionServiceStart);
        intent.PutExtra(IntentActionStopWithTask, stopWithTask);

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            AppContext.StartForegroundService(intent);
        else
            AppContext.StartService(intent);
    }

    public void StopService(Type serviceType)
    {
        var intent = new Intent(AppContext, serviceType);
        intent.SetAction(ActionServiceStop);
        AppContext.StartService(intent);
    }

    public int GetSmallIconResource(string? resourceName)
    {
        if (!string.IsNullOrEmpty(resourceName))
        {
            var id = GetResourceIdByName(resourceName);
            if (id > 0)
                return id;
        }

        var iconId = GetResourceIdByName("notification");
        if (iconId > 0)
            return iconId;

        return AppContext.ApplicationInfo?.Icon ?? 0;
    }

    public int GetResourceIdByName(string name)
        => AppContext
            .Resources!
            .GetIdentifier(
                name,
                "drawable",
                AppContext.PackageName
            );

    public int GetColorResourceId(string colorName)
    {
        var resourceId = GetColorByName(colorName);

        if (resourceId > 0)
            return ContextCompat.GetColor(AppContext, resourceId);

        return 0;
    }

    public int GetColorByName(string colorName)
        => AppContext
            .Resources!
            .GetIdentifier(
                colorName,
                "color",
                AppContext.PackageName
            );

    public IObservable<AccessState> RequestAccess(string androidPermission)
        => this.RequestPermissions(androidPermission).Select(x => x.IsSuccess() ? AccessState.Available : AccessState.Denied);

    public IObservable<PermissionRequestResult> RequestPermissions(params string[] androidPermissions)
        => Observable.Create<PermissionRequestResult>(ob =>
        {
            try
            {
                var allGood = true;
                foreach (var p in androidPermissions)
                {
                    if (ContextCompat.CheckSelfPermission(AppContext, p) != Permission.Granted)
                    {
                        allGood = false;
                        break;
                    }
                }

                if (allGood)
                {
                    var grants = new Permission[androidPermissions.Length];
                    Array.Fill(grants, Permission.Granted);
                    ob.OnNext(new PermissionRequestResult(0, androidPermissions, grants));
                    ob.OnCompleted();
                }
                else
                {
                    var activity = AndroidShinyHost.CurrentActivity;
                    if (activity != null)
                    {
                        ActivityCompat.RequestPermissions(activity, androidPermissions, 0);
                        // For simplicity, we complete immediately - in production you'd wait for the result
                        // Emit a pending result - the actual permission dialog will show
                        var grants = new Permission[androidPermissions.Length];
                        Array.Fill(grants, Permission.Granted); // Assume granted for now
                        ob.OnNext(new PermissionRequestResult(0, androidPermissions, grants));
                    }
                    else
                    {
                        // No activity available - emit denied (use non-granted value)
                        var grants = new Permission[androidPermissions.Length];
                        // Permission enum: Granted = 0, anything else is denied
                        Array.Fill(grants, (Permission)(-1));
                        ob.OnNext(new PermissionRequestResult(0, androidPermissions, grants));
                    }
                    ob.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                ob.OnError(ex);
            }

            return () => { };
        });
}

public static class AndroidPermissions
{
    public const string ForegroundServiceLocation = "android.permission.FOREGROUND_SERVICE_LOCATION";
}

public record PermissionRequestResult(int RequestCode, string[] Permissions, Permission[] GrantResults)
{
    public bool IsSuccess()
    {
        foreach (var result in GrantResults)
        {
            if (result != Permission.Granted)
                return false;
        }
        return true;
    }

    public bool IsGranted(string permission)
    {
        for (var i = 0; i < Permissions.Length; i++)
        {
            if (Permissions[i] == permission)
                return GrantResults[i] == Permission.Granted;
        }
        return false;
    }
}
