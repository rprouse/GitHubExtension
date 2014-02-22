using System.Windows;
using System.Windows.Controls;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void OnLogon(object sender, RoutedEventArgs e)
        {
            var api = Factory.Get<GitHubApi>();
            api.Login( Username.Text, Password.Password );
        }
    }
}
