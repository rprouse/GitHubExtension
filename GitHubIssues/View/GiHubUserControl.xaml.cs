using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Model;
using Octokit;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for GiHubUserControl.xaml
    /// </summary>
    public partial class GiHubUserControl : UserControl
    {
        public GiHubUserControl()
        {
            InitializeComponent();
        }

        [NotNull]
        public GitHubApi GitHubApi
        {
            get { return Factory.Get<GitHubApi>(); }
        }
    }
}
