using System;
using System.Reactive.Linq;
using Alteridem.GitHub.Logging;

namespace Alteridem.GitHub.Extensions
{
    public static class ObservableExtensions
    {
        private static IOutputWriter _log = Factory.Get<IOutputWriter>();

        public static IObservable<T> LoggedCatch<T>(this IObservable<T> This, LogLevel level = LogLevel.Warn, IObservable<T> next = null, string message = null)
        {
            next = next ?? Observable.Return<T>(default(T));
            return Observable.Catch<T, Exception>(This, ex =>
            {
                _log.Write(level, message ?? "", ex);
                return next;
            });
        }
    }
}
