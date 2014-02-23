using System;
using Alteridem.GitHub.Annotations;

namespace Alteridem.GitHub.Model
{
    public interface ILogonView : ILogonObservable
    {
        [CanBeNull]
        string Username { get; }
        [CanBeNull]
        string Password { get; }
        bool? ShowDialog();
    }
}