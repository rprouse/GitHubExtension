using System.Windows;
using Alteridem.GitHub.Annotations;

namespace Alteridem.GitHub.Model
{
    public interface ILogonView : ILogonObservable
    {
        [CanBeNull]
        Window Owner { get; set; }

        [CanBeNull]
        string Username { get; }

        [CanBeNull]
        string Password { get; }

        bool? ShowDialog();
    }
}