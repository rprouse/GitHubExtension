using System.Windows;
using Alteridem.GitHub.Model;

namespace Alteridem.GitHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly GitHubApi _gitHub = new GitHubApi();

        public GitHubApi GitHubApi { get { return _gitHub; } }
    }
}
