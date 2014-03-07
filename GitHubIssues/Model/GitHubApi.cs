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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        private bool _gettingIssues;
        private User _user;
        private RepositoryWrapper _repository;
        private IssueFilter _filter;
        private Label _label;
        private Milestone _milestone;
        private Issue _issue;
        private string _markdown = string.Empty;
        private readonly Label _emptyLabel;
        private readonly Milestone _noMilestone;
        private readonly Milestone _allMilestones;

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
                GetRepositoryInfo();
                OnPropertyChanged();
            }
        }

        public Label Label
        {
            get { return _label; }
            set
            {
                if (Equals(value, _label)) return;
                _label = value;
                GetIssues();
                OnPropertyChanged();
            }
        }

        public Milestone Milestone
        {
            get { return _milestone; }
            set
            {
                if (Equals(value, _milestone)) return;
                _milestone = value;
                GetIssues();
                OnPropertyChanged();
            }
        }

        [NotNull]
        public BindingList<RepositoryWrapper> Repositories { get; set; }

        [NotNull]
        public BindingList<Label> Labels { get; set; }

        [NotNull]
        public BindingList<Milestone> Milestones { get; set; }
            
        [NotNull]
        public BindingList<Organization> Organizations { get; set; }

        [NotNull]
        public BindingList<Issue> Issues { get; set; }

        /// <summary>
        /// Should we fetch open, closed, assigned, etc issues
        /// </summary>
        public IssueFilter Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(value, _filter)) return;
                _filter = value;
                GetIssues();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the markdown text for an issue.
        /// </summary>
        public string IssueMarkdown
        {
            get { return _markdown; }
            set
            {
                if(Equals(value, _markdown)) return;
                _markdown = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The currently selected issue
        /// </summary>
        public Issue Issue
        {
            get { return _issue; }
            set
            {
                if(Equals(value, _issue)) return;
                _issue = value;
                GetComments( _issue );
                OnPropertyChanged();
            }
        }

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

        public GitHubApi(GitHubClient github)
        {
            _github = github;

            _filter = IssueFilter.Assigned;
            Repositories = new BindingList<RepositoryWrapper>();
            Organizations = new BindingList<Organization>();
            Issues = new BindingList<Issue>();
            Labels = new BindingList<Label>();
            Milestones = new BindingList<Milestone>();
            _emptyLabel = new Label {Color = "00000000", Name = "(none)"};
            _allMilestones = new Milestone { Number = 0, Title = "All Milestones", OpenIssues = 0 };
            _noMilestone = new Milestone { Number = -1, Title = "No Milestone", OpenIssues = 0 };

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

        private void GetRepositoryInfo()
        {
            GetLabels();
            GetMilestones();
            GetIssues();
        }

        private async void GetLabels()
        {
            Labels.Clear();
            Label = null;
            if (Repository != null && Repository.Repository != null)
            {
                var labels = await _github.Issue.Labels.GetForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name);
                Labels.Add( _emptyLabel );
                foreach (var label in labels)
                    Labels.Add(label);

                Label = _emptyLabel;
            }
        }

        private async void GetMilestones()
        {
            Milestones.Clear();
            Milestone = null;
            if (Repository != null && Repository.Repository != null)
            {
                Milestones.Add( _allMilestones );
                Milestones.Add( _noMilestone );
                var milestones = await _github.Issue.Milestone.GetForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name);
                foreach (var milestone in milestones)
                    Milestones.Add(milestone);

                Milestone = _allMilestones;
            }
        }

        private async void GetIssues()
        {
            if (_gettingIssues)
                return;

            _gettingIssues = true;
            Issues.Clear();
            var wrapper = Repository;
            if (wrapper != null && wrapper.Repository != null)
            {
                var repository = wrapper.Repository;
                // TODO: Filter issues
                var request = new RepositoryIssueRequest();
                request.State = ItemState.Open;
                request.Filter = IssueFilter.All;
                if (Label != null && Label != _emptyLabel)
                    request.Labels.Add(Label.Name);

                if ( Milestone == _noMilestone )
                    request.Milestone = "none";
                else if ( Milestone == _allMilestones )
                    request.Milestone = "*";
                else if (Milestone != null)
                    request.Milestone = Milestone.Number.ToString();

                await GetIssues(repository.Owner.Login, repository.Name, request);
            }
            _gettingIssues = false;
        }

        private async Task GetIssues(string owner, string name, RepositoryIssueRequest request)
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

        public async void GetComments( Issue issue )
        {
            if (issue == null)
                return;

            IssueMarkdown = issue.Body;

            if ( issue.Comments == 0 )
                return;

            IReadOnlyList<IssueComment> comments = await _github.Issue.Comment.GetForIssue( Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number );
            AppendComments(comments);
        }

        private void AppendComments(IEnumerable<IssueComment> comments)
        {
            var builder = new StringBuilder(_markdown);
            foreach (var comment in comments)
            {
                string gravatar =
                    string.Format(
                        "https://www.gravatar.com/avatar/{0}?s={1}&r=x",
                        comment.User.GravatarId,
                        22);

                builder.AppendFormat(
                    "{0}<div class=\"header\">{0}<div class=\"user\"><img align=\"left\" alt=\"{1}\" src=\"{2}\">&nbsp; {1}</div><div class=\"date\">{3:d}</div>{0}</div>{0}{0}{4}",
                    Environment.NewLine,
                    comment.User.Login,
                    gravatar,
                    comment.CreatedAt,
                    comment.Body
                    );
            }
            IssueMarkdown = builder.ToString();
        }

        public async void AddComment(Issue issue, string comment)
        {
            var newComment = await _github.Issue.Comment.Create( Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number, comment );

            // Append the current comment
            AppendComments(new []{ newComment });
        }

        public async void CloseIssue(Issue issue, string comment)
        {
            if ( !string.IsNullOrWhiteSpace(comment))
                AddComment(issue, comment);

            var update = new IssueUpdate();
            update.State = ItemState.Closed;
            var updatedIssue = await _github.Issue.Update(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number, update);
            if (updatedIssue.State == ItemState.Closed)
            {
                Issue = null;
                Issues.Remove(issue);
                IssueMarkdown = string.Empty;
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
