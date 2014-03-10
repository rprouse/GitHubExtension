using Alteridem.GitHub.Model;
using Ninject.Modules;
using Octokit;

namespace Alteridem.GitHub.Modules
{
    public class GitHubModule : NinjectModule
    {
        public override void Load()
        {
            Bind<GitHubApi>()
                .To<GitHubApi>()
                .InSingletonScope();
        }
    }
}