using System;
using System.Windows;
using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl, ILogonView
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void OnLogon(object sender, RoutedEventArgs e)
        {
            Message.Text = string.Empty;
            var api = Factory.Get<GitHubApi>();
            api.Login(this);
        }

        [NotNull]
        public string Username { get { return UserText.Text; } }

        [NotNull]
        public string Password { get { return PassText.Password; } }

        public void OnLoggingIn()
        {
            IsEnabled = false;
            // TODO: Display progress
        }

        public void OnSuccess()
        {
            IsEnabled = true;
            Message.Text = "Success";
            // TODO: Succeeded, close?
        }

        public void OnError([NotNull] Exception ex)
        {
            IsEnabled = true;
            Message.Text = ex.Message;
        }
    }
}
