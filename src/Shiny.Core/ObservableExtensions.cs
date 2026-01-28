using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Shiny;

public static class ObservableExtensions
{
    public static Task<T> ToTask<T>(this IObservable<T> observable, CancellationToken cancellationToken = default)
        => observable.FirstAsync().ToTask(cancellationToken);
}
