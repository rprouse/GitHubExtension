using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Alteridem.GitHub.Annotations;
using Octokit;
using Octokit.Reactive;

namespace Alteridem.GitHub.Model
{
    public class GitHubApi : INotifyPropertyChanged
    {
        private readonly GitHubClient _github;
        private readonly IObservableGitHubClient _observableGitHub;
        private string _token;
        private User _user;

        [CanBeNull]
        public User User
        {
            get { return _user; }
            private set
            {
                if (Equals(value, _user)) return;
                _user = value;
                OnPropertyChanged();
            }
        }

        [NotNull]
        public BindingList<Repository> Repositories { get; set; }

        [NotNull]
        public BindingList<Organization> Organizations { get; set; }

        public GitHubApi()
        {
            _github = new GitHubClient(new ProductHeaderValue("GitHubExtension"));
            _observableGitHub = new ObservableGitHubClient(_github);

            Repositories = new BindingList<Repository>();
            Organizations = new BindingList<Organization>();

            // TODO: Get user and token from settings and log in using them

            //if(!string.IsNullOrWhiteSpace(_token))
            //{
            //    Login();
            //}
        }

        public void Login([NotNull] ILogonView view)
        {
            Login(new Credentials(view.Username, view.Password), view);
        }

        private void Login([NotNull] Credentials credentials, [NotNull] ILogonView view)
        {
            _github.Credentials = credentials;
            _token = null;
            var newAuth = new NewAuthorization
            {
                Scopes = new[] {"user", "repo"},
                Note = "GitHub Visual Studio Extension"
            };

            view.OnLoggingIn();
            _observableGitHub.Authorization
                .GetOrCreateApplicationAuthentication(Secrets.CLIENT_ID, Secrets.CLIENT_SECRET, newAuth)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnAuthorized, view.OnError, () =>
                {
                    view.OnSuccess();
                    LoadData();
                });
        }

        private void OnAuthorized([NotNull] Authorization auth)
        {
            _token = auth.Token;
            // TODO: Persist token
        }

        private void LoadData()
        {
            GetUser();
            GetRepositories();
        }

        private void GetUser()
        {
            _observableGitHub.User
                .Current()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(user =>
                {
                    User = user;
                }, exception => { } );
        }

        private void GetRepositories()
        {
            Repositories.Clear();
            Organizations.Clear();
            _observableGitHub.Repository
                .GetAllForCurrent()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(NextRepository, exception => { }, () => { /* Select default */ });

            _observableGitHub.Organization
                .GetAllForCurrent()
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(NextOrganization, exception => { }, () => { /* Select default */ });
        }

        private void NextRepository([NotNull] Repository repository)
        {
            Repositories.Add(repository);
        }

        private void NextOrganization([NotNull] Organization org)
        {
            Organizations.Add(org);

            _observableGitHub.Repository
                .GetAllForOrg(org.Login)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(NextRepository, exception => { }, () => { /* Select default */ });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull,CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
