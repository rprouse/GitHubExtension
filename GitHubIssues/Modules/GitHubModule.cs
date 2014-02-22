using Alteridem.GitHub.Model;
using Ninject.Modules;

namespace Alteridem.GitHub.Modules
{
    public class GitHubModule : NinjectModule
    {
        public override void Load()
        {
            Bind<GitHubApi>().To<GitHubApi>().InSingletonScope();
        }
    }
}