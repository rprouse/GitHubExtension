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
using NLog;
using Octokit;

#endregion

namespace Alteridem.GitHub.Model
{
    public class GitHubApi : GitHubApiBase
    {
        #region Members

        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly GitHubClient _github;
        private bool _gettingIssues;
        private readonly Label _allLabels;
        private readonly Milestone _noMilestone;
        private readonly Milestone _allMilestones;

        #endregion

        public GitHubApi(GitHubClient github)
        {
            _github = github;

            _allLabels = new Label { Color = "00000000", Name = "All Labels" };
            _allMilestones = new Milestone { Number = 0, Title = "All Milestones", OpenIssues = 0 };
            _noMilestone = new Milestone { Number = -1, Title = "No Milestone", OpenIssues = 0 };

            // Get user and token from settings and log in using them
            LoginFromCache();
        }

        #region Public API

        public override async Task<bool> Login(string username, string password)
        {
            return await Login(new Credentials(username, password));
        }

        public override void Logout()
        {
            log.Info("Logging out of GitHub");
            base.Logout();
            Cache.Credentials = null;
            User = null;
            Repositories.Clear();
            Organizations.Clear();
            Issues.Clear();
        }

        public override async void GetIssues()
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
                if (Label != null && Label != _allLabels)
                    request.Labels.Add(Label.Name);

                if (Milestone == _noMilestone)
                    request.Milestone = "none";
                else if (Milestone == _allMilestones)
                    request.Milestone = "*";
                else if (Milestone != null)
                    request.Milestone = Milestone.Number.ToString(CultureInfo.InvariantCulture);

                await GetIssues(repository.Owner.Login, repository.Name, request);
            }
            _gettingIssues = false;
        }

        public override async void GetComments(Issue issue)
        {
            if (issue == null)
                return;

            IssueMarkdown = issue.Body;

            if (issue.Comments == 0 || Repository == null)
                return;

            IReadOnlyList<IssueComment> comments = await _github.Issue.Comment.GetForIssue(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number);
            AppendComments(comments);
        }

        public override async void AddComment(Issue issue, string comment)
        {
            if (Repository == null)
                return;

            var newComment = await _github.Issue.Comment.Create(Repository.Repository.Owner.Login, Repository.Repository.Name, issue.Number, comment);

            // Append the current comment
            AppendComments(new[] { newComment });
        }

        public override async void CloseIssue(Issue issue, string comment)
        {
            if (!string.IsNullOrWhiteSpace(comment))
                AddComment(issue, comment);

            if (Repository == null)
                return;

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

        public override async Task<IReadOnlyList<User>> GetAssignees(Repository repository)
        {
            return await _github.Issue.Assignee.GetForRepository(repository.Owner.Login, repository.Name);
        }

        public override async void SaveIssue(Repository repository, NewIssue newIssue)
        {
            Issue issue = await _github.Issue.Create(repository.Owner.Login, repository.Name, newIssue);
            if (issue != null && Repository != null && repository.Id == Repository.Repository.Id)
            {
                Issues.Insert(0, issue);
                Issue = issue;
            }
        }

        public override async void UpdateIssue(Repository repository, int id, IssueUpdate update)
        {
            Issue issueUpdate = await _github.Issue.Update(repository.Owner.Login, repository.Name, id, update);
            if (Repository != null && repository.Id == Repository.Repository.Id)
            {
                foreach (var issue in Issues)
                {
                    if (issue.Number == issueUpdate.Number)
                    {
                        Issues.Remove(issue);
                        Issues.Insert(0, issueUpdate);
                        Issue = issueUpdate;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Private members

        private async void LoginFromCache()
        {
            var credentials = Cache.Credentials;
            if ( credentials != null )
                await Login(new Credentials(credentials.Logon, credentials.Password));
        }

        private async Task<bool> Login([NotNull] Credentials credentials)
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

            try
            {
                var auth = await _github.Authorization.GetOrCreateApplicationAuthentication(Secrets.CLIENT_ID, Secrets.CLIENT_SECRET, newAuth);
                log.Info("Successfully logged in to GitHub");
                Token = auth.Token;
                LoadData();
                return true;
            }
            catch (Exception ex)
            {
                log.WarnException("Failed to login", ex);
                Logout();
                throw;
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
            int id = Cache.Repository;

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

        protected override async void GetLabels()
        {
            Labels.Clear();
            Label = null;
            if (Repository != null && Repository.Repository != null)
            {
                var labels = await _github.Issue.Labels.GetForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name);
                Labels.Add(_allLabels);
                foreach (var label in labels)
                    Labels.Add(label);

                Label = _allLabels;
            }
        }

        protected override async void GetMilestones()
        {
            Milestones.Clear();
            Milestone = null;
            if (Repository != null && Repository.Repository != null)
            {
                Milestones.Add(_allMilestones);
                Milestones.Add(_noMilestone);
                var request = new MilestoneRequest();
                request.State = ItemState.Open;
                request.SortProperty = MilestoneSort.DueDate;
                request.SortDirection = SortDirection.Ascending;
                var milestones = await _github.Issue.Milestone.GetForRepository(Repository.Repository.Owner.Login, Repository.Repository.Name, request);
                foreach (var milestone in milestones)
                    Milestones.Add(milestone);

                Milestone = _allMilestones;
            }
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
