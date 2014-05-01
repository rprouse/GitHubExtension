using Alteridem.GitHub.Model;
using Ninject.Modules;
using Octokit;

namespace Alteridem.GitHub.Modules
{
    public class GitHubModule : NinjectModule
    {
        public override void Load()
        {
            Bind<GitHubClient>()
                .To<GitHubClient>()
                .WithConstructorArgument("productInformation", c => new ProductHeaderValue("GitHubExtension"));


            Bind<GitHubApiBase>()
                .To<GitHubApi>()
                .InSingletonScope();
        }
    }
}