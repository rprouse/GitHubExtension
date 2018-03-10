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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Filters;
using Alteridem.GitHub.Logging;
using Octokit;
using EnvDTE;

using Alteridem.GitHub.Extensions;

#endregion

namespace Alteridem.GitHub.Model
{
    public class GitHubApi : GitHubApiBase
    {
        #region Members

        private readonly GitHubClient _github;
        private readonly IOutputWriter _log;
        private bool _gettingIssues;
        private bool _gettingMilestones;
        private bool _gettingLabels;

        #endregion

        public GitHubApi(GitHubClient github, IOutputWriter logWriter, ICache settingsCache)
            : base(settingsCache)
        {
            _github = github;
            _log = logWriter;

            // Get user and token from settings and log in using them
            LoginFromCache();
        }

        #region Public API

        public override bool HasClientId
        {
            get
            {
                return !string.IsNullOrEmpty(Secrets.CLIENT_ID);
            }
        }

        public override async Task Login(string username, string password, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
                await LoginWithAccessToken(new Credentials(accessToken));
            else
                await LoginWithUsernameAndPassword(new Credentials(username, password));
        }

        public override void Logout()
        {
            _log.Write(LogLevel.Info, "Logging out of GitHub");
            base.Logout();
            SettingsCache.Credentials = null;
            User = null;
            Repositories.Clear();
            Organizations.Clear();
            AllIssues.Clear();
            OnPropertyChanged("Issues");
        }

        public override async void GetIssues()
        {
            if (_gettingIssues)
                return;

            _gettingIssues = true;
            AllIssues.Clear();
            OnPropertyChanged("Issues");
            var wrapper = Repository;
            if (wrapper != null && wrapper.Repository != null)
            {
                var repository = wrapper.Repository;
                var request = new RepositoryIssueRequest();
                request.State = ItemStateFilter.Open;
                request.Filter = IssueFilter.All;
                await GetIssues(repository.Owner.Login, repository.Name, request);
            }
            _gettingIssues = false;
        }

        public override async void GetMilestones()
        {
            if (_gettingMilestones)
                return;

            _gettingMilestones = true;
            Milestones.Clear();
            Milestone = null;
            if (Repository != null && Repository.Repository != null)
            {
                Milestones.Add(AllMilestones);
                Milestones.Add(NoMilestone);
                var request = new MilestoneRequest();
                request.State = ItemStateFilter.Open;
                request.SortProperty = MilestoneSort.DueDate;
                request.SortDirection = SortDirection.Ascending;
                try
                {
                    var milestones = await _github.Issue.Milestone.GetAllForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name, request);
                    foreach (var milestone in milestones)
                        Milestones.Add(milestone);

                    Milestone = AllMilestones;
                }
                catch (Exception exception)
                {
                    _log.Write(LogLevel.Warn, "Failed to get milestones for repository", exception);
                }
            }
            _gettingMilestones = false;
        }

        public override async void GetLabels()
        {
            if (_gettingLabels)
                return;

            _gettingLabels = true;
            Labels.Clear();
            Label = null;
            if (Repository != null && Repository.Repository != null)
            {
                try
                {
                    var labels = await _github.Issue.Labels.GetAllForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name);
                    Labels.Add(_allLabels);
                    foreach (var label in labels.OrderBy(l => l.Name))
                        Labels.Add(label);

                    Label = _allLabels;
                }
                catch (Exception exception)
                {
                    _log.Write(LogLevel.Warn, "Failed to get labels for repository", exception);
                }
            }
            _gettingLabels = false;
        }

        protected override async void GetComments(Issue issue)
        {
            if (issue == null)
                return;

            IssueMarkdown = issue.Body;

            if (issue.Comments == 0 || Repository == null)
                return;

            try
            {
                IReadOnlyList<IssueComment> comments = await _github.Issue.Comment.GetAllForIssue(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number);
                AppendComments(comments);
            }
            catch ( Exception exception )
            {
                _log.Write( LogLevel.Error, "Failed to fetch comments for issue.", exception );
            }
        }

        public override async void AddComment(Issue issue, string comment)
        {
            if (Repository == null)
                return;

            try
            {
                var newComment = await _github.Issue.Comment.Create(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number, comment);

                // Append the current comment
                AppendComments(new[] { newComment });
            }
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Error, "Failed to add comment to issue.", exception );
            }
        }

        public override async void CloseIssue(Issue issue, string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
                AddComment(issue, comment);

            if (Repository == null)
                return;

            var update = new IssueUpdate();
            update.State = ItemState.Closed;
            try
            {
                var updatedIssue = await _github.Issue.Update(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number, update);
                if (updatedIssue.State == ItemState.Closed)
                {
                    AllIssues.Remove(issue);
                    Issue = null;
                    IssueMarkdown = string.Empty;
                }
            }
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Error, "Failed to update issue.", exception );
            }
        }

        public override async Task<IReadOnlyList<User>> GetAssignees(Repository repository)
        {
            try
            {
                return await _github.Issue.Assignee.GetAllForRepository(repository.Owner.Login, repository.Name);
            }
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Warn, "Failed to fetch assignees", exception);
                return new List<User>( );
            }
        }

        public override async void SaveIssue(Repository repository, NewIssue newIssue)
        {
            try
            {
                Issue issue = await _github.Issue.Create(repository.Owner.Login, repository.Name, newIssue);
                if (issue != null && Repository != null && repository.Id == Repository.Repository.Id)
                {
                    AllIssues.Insert(0, issue);;
                    Issue = issue;
                }
            }
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Error, "Failed to save issue.", exception);
            }
        }

        public override async void UpdateIssue(Repository repository, int id, IssueUpdate update)
        {
            try
            {
                Issue issueUpdate = await _github.Issue.Update(repository.Owner.Login, repository.Name, id, update);
                if (Repository != null && repository.Id == Repository.Repository.Id)
                {
                    foreach (var issue in AllIssues)
                    {
                        if (issue.Number == issueUpdate.Number)
                        {
                            AllIssues.Remove(issue);
                            AllIssues.Insert(0, issueUpdate);
                            Issue = issueUpdate;
                            break;
                        }
                    }
                }
            }
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Error, "Failed to save issue.", exception);
            }
        }

        #endregion

        #region Private members

        private async void LoginFromCache()
        {
            var credentials = SettingsCache.Credentials;
            if (credentials != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(credentials.AccessToken))
                        await LoginWithAccessToken(new Credentials(credentials.AccessToken));
                    else if (!string.IsNullOrEmpty(credentials.Logon))
                        await LoginWithUsernameAndPassword(new Credentials(credentials.Logon, credentials.Password));
                }
                catch (Exception exception)
                {
                    Logout();
                    _log.Write(LogLevel.Error, "Failed to log in with cached credentials.", exception);
                }
            }
        }

        private async Task LoginWithUsernameAndPassword([NotNull] Credentials credentials)
        {
            _log.Write(LogLevel.Debug, "Logging in with credentials");
            _github.Credentials = credentials;

            try
            {
                SettingsCache.SaveCredentials(credentials.Login, credentials.Password);
                var newAuth = new NewAuthorization
                {
                    Scopes = new[] { "user", "repo" },
                    Note = "GitHub Visual Studio Extension",
                    NoteUrl = "http://www.alteridem.net",
                    Fingerprint = AuthenticationHelpers.GetFingerprint()
                };

                var auth = await _github.Authorization.GetOrCreateApplicationAuthentication( Secrets.CLIENT_ID, Secrets.CLIENT_SECRET, newAuth );
                _log.Write( LogLevel.Info, "Successfully logged in to GitHub" );
                Token = auth.HashedToken;

                await LoadData();
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Warn, "Failed to login", ex);
                Logout();
                throw;
            }
        }

        private async Task LoginWithAccessToken( [NotNull] Credentials credentials )
        {
            _log.Write( LogLevel.Debug, "Logging in with access token" );
            _github.Credentials = credentials;

            try
            {
                SettingsCache.SaveToken( credentials.GetToken() );
                Token = credentials.GetToken();
                await LoadData();
            }
            catch ( Exception ex )
            {
                _log.Write( LogLevel.Warn, "Failed to login", ex );
                Logout();
                throw;
            }
        }

        private async Task LoadData()
        {
            await GetUser();
            await GetRepositories();
        }

        private async Task GetUser()
        {
            _log.Write(LogLevel.Debug, "Fetching current user");
            try
            {
                User = await _github.User.Current();
                _log.Write(LogLevel.Debug, "Finished fetching current user");
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Warn, "Failed to fetch current user", ex);
                throw;
            }
        }

        private async Task GetRepositories()
        {
            _log.Write(LogLevel.Debug, "Fetching repositories for current user");
            Repositories.Clear();
            Organizations.Clear();

            try
            {
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
            catch ( Exception exception )
            {
                _log.Write(LogLevel.Warn, "Failed to get repository data", exception);
            }
        }

        private void AddRepositories([NotNull] IEnumerable<Repository> repositories)
        {
            foreach (var repository in repositories.Filter())
            {
                _log.Write(LogLevel.Debug, "Fetched repository {0}", repository.FullName);
                Repositories.Add(new RepositoryWrapper(repository));
            }
        }

        private void OnRepositoriesComplete()
        {
            _log.Write(LogLevel.Debug, "Finished fetching organizations for current user");
            var id = SettingsCache.Repository;

            var dte = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService<DTE>();
            if(dte != null && dte.Solution != null)
            {
                if (SetRepositoryForSolution(dte.Solution.FullName))
                    return;
            }

            var wrapper = (from r in Repositories where r.Repository.Id == id select r).FirstOrDefault();
            if (wrapper != null)
                Repository = wrapper;
            else
                SetDefaultRepository();
        }

        private void SetDefaultRepository()
        {
            Repository = Repositories.Count > 0 ? Repositories[0] : null;
        }

        private async Task GetIssues(string owner, string name, RepositoryIssueRequest request)
        {
            _log.Write(LogLevel.Debug, "Fetching issues for {0}/{1}", owner, name);
            try
            {
                var issues = await _github.Issue.GetAllForRepository(owner, name, request);
                foreach (var issue in issues)
                {
                    AllIssues.Add(issue);
                }
            }
            catch (Exception ex)
            {
                _log.Write(LogLevel.Warn, "Failed to fetch issues", ex);
            }
            OnPropertyChanged("Issues");
        }

        private void AppendComments(IEnumerable<IssueComment> comments)
        {
            var builder = new StringBuilder(IssueMarkdown);
            foreach (var comment in comments)
            {
                string avatar = comment.User.AvatarUrl + "&size=22";

                builder.AppendFormat(
                    "{0}<div class=\"header\">{0}<div class=\"user\"><img align=\"left\" alt=\"{1}\" src=\"{2}\" width=\"22\" height=\"22\">&nbsp; {1}</div><div class=\"date\">{3:d}</div>{0}</div>{0}{0}{4}",
                    Environment.NewLine,
                    comment.User.Login,
                    avatar,
                    comment.CreatedAt,
                    comment.Body
                    );
            }
            IssueMarkdown = builder.ToString();
        }

        #endregion
    }
}
