using System.Windows;
using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Model;
using Octokit;

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
            GitHubApi.ToggleLogin();
        }
    }
}
