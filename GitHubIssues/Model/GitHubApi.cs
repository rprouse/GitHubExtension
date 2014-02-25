// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2014 Rob Prouse
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// **********************************************************************************

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Alteridem.GitHub.Annotations;
using NLog;
using Octokit;

#endregion

namespace Alteridem.GitHub.Model
{
    public class GitHubApi : INotifyPropertyChanged
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly GitHubClient _github;
        private string _token;
        private User _user;
        private RepositoryWrapper _repository;

        #region LogonWatcher Class

        private class LogonWatcher : ILogonObservable
        {
            private readonly GitHubApi _parent;

            public LogonWatcher([NotNull] GitHubApi parent)
            {
                _parent = parent;
            }

            public void OnLoggingIn()
            {
                log.Info("Logging in with authentication token");
            }

            public void OnSuccess()
            {
                log.Info("Successfully logged in with authentication token");
            }

            public void OnError(Exception ex)
            {
                log.ErrorException("Failed to login to GitHub using token", ex);
                _parent.Logout();
            }
        }

        #endregion

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

        /// <summary>
        /// The currently selected repository.
        /// </summary>
        [CanBeNull]
        public RepositoryWrapper Repository
        {
            get { return _repository; }
            set
            {
                if (Equals(value, _repository)) return;
                if (value != null)
                    Cache.SaveRepository(value.Repository);
                else
                    Cache.DeleteRepository();
                _repository = value;
                GetIssues();
                OnPropertyChanged();
            }
        }

        [NotNull]
        public BindingList<RepositoryWrapper> Repositories { get; set; }

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

            Repositories = new BindingList<RepositoryWrapper>();
            Organizations = new BindingList<Organization>();
            Issues = new BindingList<Issue>();

            // Get user and token from settings and log in using them
            LoginFromCache();
        }

        public void Logout()
        {
            log.Info("Logging out of GitHub");
            Token = string.Empty;
            Cache.DeleteCredentials();
            User = null;
            Repositories.Clear();
            Organizations.Clear();
            Issues.Clear();
        }

        private void LoginFromCache()
        {
            Cache.GetCredentials()
                .Subscribe(credentials =>
                {
                    var view = new LogonWatcher(this);
                    Login(new Credentials(credentials.Logon, credentials.Password), view);
                });
        }

        public void Login([NotNull] ILogonView view)
        {
            Login(new Credentials(view.Username, view.Password), view);
        }

        private async void Login([NotNull] Credentials credentials, [NotNull] ILogonObservable view)
        {
            log.Info("Logging in with credentials");
            _github.Credentials = credentials;
            Cache.SaveCredentials(credentials.Login, credentials.Password);
            var newAuth = new NewAuthorization
            {
                Scopes = new[] { "user", "repo" },
                Note = "GitHub Visual Studio Extension",
                NoteUrl = "http://www.alteridem.net"
            };

            view.OnLoggingIn();
            try
            {
                var auth = await _github.Authorization.GetOrCreateApplicationAuthentication(Secrets.CLIENT_ID, Secrets.CLIENT_SECRET, newAuth);
                log.Info("Successfully logged in to GitHub");
                Token = auth.Token;
                view.OnSuccess();
                LoadData();
            }
            catch (Exception ex)
            {
                log.WarnException("Failed to login", ex);
                Logout();
                view.OnError(ex);
            }
        }

        private void LoadData()
        {
            GetUser();
            GetRepositories();
        }

        private async void GetUser()
        {
            log.Info("Fetching current user");
            try
            {
                User = await _github.User.Current();
                    log.Info("Finished fetching current user");
            }
            catch (Exception ex)
            {
                log.ErrorException("Failed to fetch current user", ex);
            }
        }

        private async void GetRepositories()
        {
            log.Info("Fetching repositories for current user");
            Repositories.Clear();
            Organizations.Clear();

            var repositories = await _github.Repository.GetAllForCurrent();
            AddRepositories(repositories);
            var organizations = await _github.Organization.GetAllForCurrent();
            foreach (var organization in organizations)
            {
                repositories = await _github.Repository.GetAllForOrg(organization.Login);
                AddRepositories(repositories);
            }
            OnRepositoriesComplete();
        }

        private void AddRepositories([NotNull] IEnumerable<Repository> repositories)
        {
            foreach (var repository in repositories)
            {
                log.Debug("Fetched repository {0}", repository.FullName);
                if (repository.HasIssues)
                    Repositories.Add(new RepositoryWrapper(repository));
            }
        }

        private void OnRepositoriesComplete()
        {
            log.Info("Finished fetching organizations for current user");
            Cache.GetRepository()
                 .Subscribe(r =>
                 {
                     var wrapper = new RepositoryWrapper(r);
                     if (Repositories.Contains(wrapper))
                         Repository = wrapper;
                     else
                         SetDefaultRepository();
                 }, ex => SetDefaultRepository());
        }

        private void SetDefaultRepository()
        {
            Repository = Repositories.Count > 0 ? Repositories[0] : null;
        }

        private void GetIssues()
        {
            Issues.Clear();
            var wrapper = Repository;
            if (wrapper != null && wrapper.Repository != null)
            {
                var repository = wrapper.Repository;
                // TODO: Filter issues
                var request = new RepositoryIssueRequest();
                request.State = ItemState.Open;
                GetIssues(repository.Owner.Login, repository.Name, request);
            }
        }

        private async void GetIssues(string owner, string name, RepositoryIssueRequest request)
        {
            log.Info("Fetching repositories for {0}/{1}", owner, name);
            try
            {
                var issues = await _github.Issue.GetForRepository(owner, name, request);
                foreach (var issue in issues)
                {
                    Issues.Add(issue);
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("Failed to fetch issues", ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull, CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
