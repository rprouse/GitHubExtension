using System.Windows;
using System.Windows.Controls;

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
            var app = App.Current as App;
            if ( app != null )
                app.GitHubApi.Login( Username.Text, Password.Password );
        }
    }
}
