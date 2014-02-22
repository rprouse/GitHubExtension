using System;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Alteridem.GitHub.View;
using Octokit;
using Octokit.Reactive;

namespace Alteridem.GitHub.Model
{
    public class GitHubApi
    {
        private string _token;

        public User User { get; private set; }

        private readonly GitHubClient _github;
        private readonly IObservableGitHubClient _observableGitHub;

        public GitHubApi()
        {
            _github = new GitHubClient(new ProductHeaderValue("GitHubExtension"));
            _observableGitHub = new ObservableGitHubClient(_github);

            // TODO: Get user and token from settings and log in using them

            //if(!string.IsNullOrWhiteSpace(_token))
            //{
            //    Login();
            //}
        }

        public void Login(ILogonView view)
        {
            Login(new Credentials(view.Username, view.Password), view);
        }

        private void Login(Credentials credentials, ILogonView view)
        {
            _github.Credentials = credentials;
            _token = null;
            var newAuth = new NewAuthorization
            {
                Scopes = new[] { "user", "repo" },
                Note = "GitHub Visual Studio Extension"
            };

            view.OnLoggingIn();
            _observableGitHub.Authorization
                .GetOrCreateApplicationAuthentication(Secrets.CLIENT_ID, Secrets.CLIENT_SECRET, newAuth)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnAuthorized, view.OnError, view.OnSuccess);
        }

        private void OnAuthorized(Authorization auth)
        {
            _token = auth.Token;
            // TODO: Persist token
        }

        private void GetUser()
        {
            _observableGitHub.User
                .Current()
                .Subscribe( user => User = user, exception => { } );
        }
    }
}
