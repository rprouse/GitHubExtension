using System;
using Alteridem.GitHub.Annotations;

namespace Alteridem.GitHub.Model
{
    public interface ILogonObservable
    {
        void OnLoggingIn();
        void OnSuccess();
        void OnError([NotNull] Exception ex);
    }
}
