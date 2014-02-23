using System;
using System.Windows;
using Alteridem.GitHub.Annotations;
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
        }

        private void OnLogon(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Message.Text = string.Empty;
            var api = Factory.Get<GitHubApi>();
            api.Login(this);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
            Close();
        }

        public void OnError(Exception ex)
        {
            IsEnabled = true;
            Message.Text = ex.Message;
        }
    }
}
