using System.Windows;
using Akavache;

namespace Alteridem.GitHub
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            BlobCache.ApplicationName = "GitHubExtension";
        }
    }
}
