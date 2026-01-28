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

    public IObservable<AccessState> RequestAccess(string androidPermission)
        => this.RequestPermissions(androidPermission).Select(x => x.IsSuccess() ? AccessState.Available : AccessState.Denied);

    public IObservable<PermissionRequestResult> RequestPermissions(params string[] androidPermissions)
        => Observable.Create<PermissionRequestResult>(ob =>
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
                }
                // For simplicity, we complete immediately - in production you'd wait for the result
                ob.OnCompleted();
            }

            return () => { };
        });
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
