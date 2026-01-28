using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shiny;

public static class ObservableExtensions
{
    public static Task<T> ToTask<T>(this IObservable<T> observable, CancellationToken cancellationToken = default)
        => System.Reactive.Threading.Tasks.TaskObservableExtensions.ToTask(observable.FirstAsync(), cancellationToken);
}
