using System.Net.Http.Headers;
using Alteridem.GitHub.Model;
using Ninject.Modules;
using Octokit;

namespace Alteridem.GitHub.Modules
{
    public class GitHubModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IGitHubClient>()
                .To<GitHubClient>()
                .WithConstructorArgument("productInformation", c => new ProductHeaderValue("GitHubExtension"));

            Bind<GitHubApi>()
                .To<GitHubApi>()
                .InSingletonScope()
                .WithConstructorArgument("github", c => Factory.Get<IGitHubClient>());
        }
    }
}