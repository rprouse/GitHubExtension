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

        #region Public Data

        public bool LoggedIn
        {
            get { return !string.IsNullOrWhiteSpace(Token); }
        }

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

        [NotNull]
        public BindingList<Issue> Issues { get; set; }

        [CanBeNull]
        private string Token
        {
            get { return _token; }
            set
            {
                if (Equals(value, _token)) return;
                _token = value;
                OnPropertyChanged();
                OnPropertyChanged("LoggedIn");
            }
        }

        #endregion

        public GitHubApi()
        {
            _github = new GitHubClient(new ProductHeaderValue("GitHubExtension"));
            _observableGitHub = new ObservableGitHubClient(_github);

            Repositories = new BindingList<Repository>();
            Organizations = new BindingList<Organization>();
            Issues = new BindingList<Issue>();

            // TODO: Get user and token from settings and log in using them
            if (!string.IsNullOrWhiteSpace(Token))
            {
                LoginWithToken();
            }
        }

        public void Logout()
        {
            Token = null;
            User = null;
            Repositories.Clear();
            Organizations.Clear();
            Issues.Clear();
        }

        private class LogonWatcher : ILogonObservable
        {
            private readonly GitHubApi _parent;

            public LogonWatcher([NotNull] GitHubApi parent)
            {
                _parent = parent;
            }

            public void OnLoggingIn() { }

            public void OnSuccess() { }

            public void OnError(Exception ex)
            {
                _parent.Token = null;
            }
        }

        private void LoginWithToken()
        {
            var view = new LogonWatcher(this);
            Login(new Credentials(Token), view);
        }

        public void Login([NotNull] ILogonView view)
        {
            Login(new Credentials(view.Username, view.Password), view);
        }

        private void Login([NotNull] Credentials credentials, [NotNull] ILogonObservable view)
        {
            _github.Credentials = credentials;
            Token = null;
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
            Token = auth.Token;
            // TODO: Persist token
        }

        private void LoadData()
        {
            GetUser();
            GetRepositories();
            GetIssues();
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

        private async void GetIssues()
        {
            var request = new RepositoryIssueRequest();
            var issues = await _github.Issue.GetForRepository("nunit", "nunit-framework");
            Issues.Clear();
            foreach (var issue in issues)
            {
                Issues.Add(issue);
            }
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
