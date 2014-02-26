using System.Windows.Controls;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub.View
{
    /// <summary>
    /// Interaction logic for IssueListControl.xaml
    /// </summary>
    public partial class IssueListControl : UserControl
    {
        public IssueListControl()
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
