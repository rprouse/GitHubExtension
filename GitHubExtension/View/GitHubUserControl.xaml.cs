using System.Windows;
using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Extensions;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for GiHubUserControl.xaml
    /// </summary>
    public partial class GitHubUserControl : UserControl
    {
        public GitHubUserControl()
        {
            InitializeComponent();
        }

        [NotNull]
        public GitHubApi GitHubApi
        {
            get { return Factory.Get<GitHubApi>(); }
        }

        private void OnLogin(object sender, RoutedEventArgs e)
        {
            if(GitHubApi.LoggedIn)
            {
                GitHubApi.Logout();
                return;
            }
            var view = Factory.Get<ILogonView>();
            view.Owner = this.GetParentWindow();
            view.ShowDialog();
        }
    }
}
