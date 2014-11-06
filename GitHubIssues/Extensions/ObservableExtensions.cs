using System;
using System.Reactive.Linq;
using NLog;

namespace Alteridem.GitHub.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> LoggedCatch<T, TObj>(this IObservable<T> This, TObj logClass, IObservable<T> next = null, string message = null)
        {
            next = next ?? Observable.Return<T>(default(T));
            return Observable.Catch<T, Exception>(This, ex =>
            {
                Logger log = LogManager.GetLogger(typeof(TObj).FullName);
                log.Warn(message ?? "", ex);
                return next;
            });
        }
    }
}
