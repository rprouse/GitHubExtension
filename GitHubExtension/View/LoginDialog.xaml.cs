using System;
using System.Windows;
using System.Windows.Input;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window, ILogonView
    {
        public LoginDialog()
        {
            InitializeComponent();
            LogonCommand = new RelayCommand( p => Logon(), p => CanLogon() );
            DataContext = this;
        }

        [NotNull]
        public string Username { get { return UserText.Text; } }

        [NotNull]
        public string Password { get { return PassText.Password; } }

        public ICommand LogonCommand { get; private set; }

        public void OnLoggingIn()
        {
            IsEnabled = false;
            Message.Text = "Logging in...";
            // TODO: Display progress
        }

        public void OnSuccess()
        {
            Close();
        }

        public void OnError(Exception ex)
        {
            IsEnabled = true;
            Message.Text = ex.Message;
        }

        private bool CanLogon()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void Logon()
        {
            Message.Text = string.Empty;
            var api = Factory.Get<GitHubApi>( );
            api.Login( this );
        }
    }
}
