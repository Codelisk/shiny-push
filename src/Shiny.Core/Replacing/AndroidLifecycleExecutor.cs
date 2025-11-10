// using System;
// using System.Collections.Generic;
// using Android.App;
// using Android.Content;
// using Android.Content.PM;
// using Android.OS;
// using Android.Runtime;
// using AndroidX.Lifecycle;
// using Java.Interop;
// using Microsoft.Extensions.Logging;
//
// namespace Shiny.Hosting;
//
//
// public class AndroidLifecycleExecutor : Java.Lang.Object, IShinyStartupTask, ILifecycleObserver, IDisposable
// {
//     readonly ILogger logger;
//     readonly AndroidPlatform platform;
//     readonly IEnumerable<IAndroidLifecycle.IApplicationLifecycle> appHandlers;
//     readonly IEnumerable<IAndroidLifecycle.IOnActivityOnCreate> onCreateHandlers;
//     readonly IEnumerable<IAndroidLifecycle.IOnActivityRequestPermissionsResult> permissionHandlers;
//     readonly IEnumerable<IAndroidLifecycle.IOnActivityNewIntent> newIntentHandlers;
//     readonly IEnumerable<IAndroidLifecycle.IOnActivityResult> activityResultHandlers;
//
//
//     public AndroidLifecycleExecutor(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) { }
//
//     public AndroidLifecycleExecutor(
//         ILogger<AndroidLifecycleExecutor> logger,
//         AndroidPlatform platform,
//         IEnumerable<IAndroidLifecycle.IApplicationLifecycle> appHandlers,
//         IEnumerable<IAndroidLifecycle.IOnActivityOnCreate> onCreateHandlers,
//         IEnumerable<IAndroidLifecycle.IOnActivityRequestPermissionsResult> permissionHandlers,
//         IEnumerable<IAndroidLifecycle.IOnActivityNewIntent> newIntentHandlers,
//         IEnumerable<IAndroidLifecycle.IOnActivityResult> activityResultHandlers
//     )
//     {
//         this.logger = logger;
//         this.platform = platform;
//         this.appHandlers = appHandlers;
//         this.onCreateHandlers = onCreateHandlers;
//         this.permissionHandlers = permissionHandlers;
//         this.newIntentHandlers = newIntentHandlers;
//         this.activityResultHandlers = activityResultHandlers;
//     }
//
//

// }
