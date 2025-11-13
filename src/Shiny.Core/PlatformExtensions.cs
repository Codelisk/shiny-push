using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shiny;


public static class PlatformExtensions
{
    public static async Task<T> InvokeTaskOnMainThread<T>(this IPlatform platform, Func<Task<T>> func, CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<T>();
        await using var _ = cancelToken.Register(() => tcs.TrySetCanceled());

        platform.InvokeOnMainThread(async () =>
        {
            try
            {
                var result = await func();
                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return await tcs.Task.ConfigureAwait(false);
    }


    public static async Task InvokeTaskOnMainThread(this IPlatform platform, Func<Task> func,
        CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancelToken.Register(() => tcs.TrySetCanceled()))
        {
            platform.InvokeOnMainThread(async () =>
            {
                try
                {
                    await func();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        await tcs.Task.ConfigureAwait(false);
    }


    public static async Task<T> InvokeOnMainThreadAsync<T>(this IPlatform platform, Func<T> func,
        CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<T>();
        using (cancelToken.Register(() => tcs.TrySetCanceled()))
        {
            platform.InvokeOnMainThread(() =>
            {
                try
                {
                    var result = func();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        return await tcs.Task.ConfigureAwait(false);
    }


    public static async Task InvokeOnMainThreadAsync(this IPlatform platform, Action action,
        CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancelToken.Register(() => tcs.TrySetCanceled()))
        {
            platform.InvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
        }

        await tcs.Task.ConfigureAwait(false);
    }
}