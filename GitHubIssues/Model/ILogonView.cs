using System;

namespace Alteridem.GitHub.Model
{
    public interface ILogonView
    {
        string Username { get; }
        string Password { get; }
        void OnLoggingIn();
        void OnSuccess();
        void OnError(Exception ex);
    }
}