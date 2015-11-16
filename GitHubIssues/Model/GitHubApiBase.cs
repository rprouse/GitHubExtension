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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Filters;
using Octokit;
using Alteridem.GitHub.Interfaces;

namespace Alteridem.GitHub.Model
{
    public abstract class GitHubApiBase : INotifyPropertyChanged
    {
        #region Members

        private bool _loggedIn;
        private User _user;
        private RepositoryWrapper _repository;
        private Label _label;
        private Milestone _milestone;
        private Issue _issue;
        private string _markdown = string.Empty;
        protected readonly Label _allLabels;
        private UserFilterType _userFilterType;
        private string _searchText;

        #endregion

        protected GitHubApiBase(ICache settingsCache)
        {
            if (settingsCache == null)
                throw new ArgumentNullException("settingsCache");

            _userFilterType = UserFilterType.All;
            UserFilters = new List<UserFilterType>();
            foreach (UserFilterType value in Enum.GetValues(typeof(UserFilterType)))
            {
                UserFilters.Add(value);
            }

            _allLabels = new Label(null, "All Labels", "FFFFFFFF");
            AllMilestones = new Milestone(null, 0, ItemState.All, "All Milestones", "", null, 0, 0, DateTimeOffset.UtcNow, null, null);
            NoMilestone = new Milestone(null, -1, ItemState.All, "No Milestone", "", null, 0, 0, DateTimeOffset.UtcNow, null, null);

            SettingsCache = settingsCache;
            Repositories = new BindingList<RepositoryWrapper>();
            Organizations = new BindingList<Organization>();
            AllIssues = new BindingList<Issue>();
            Labels = new BindingList<Label>();
            Milestones = new BindingList<Milestone>();
        }

        #region Public Data

        public bool LoggedIn
        {
            get { return _loggedIn; }
            set
            {
                _loggedIn = value;
                OnPropertyChanged();
            }
        }

        public Uri AuthorizeUrl => 
            new Uri(string.Format("https://github.com/login/oauth/authorize?client_id={0}&scope={1}",
                Secrets.CLIENT_ID, Scopes));

        public string Scopes => "user,repo,gist";

        [CanBeNull]
        public User User
        {
            get { return _user; }
            protected set
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
                    SettingsCache.Repository = value.Repository.Id; // Save
                else
                    SettingsCache.Repository = 0;   // Delete
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
                OnPropertyChanged();
                OnPropertyChanged("Issues");
            }
        }

        public Milestone Milestone
        {
            get { return _milestone; }
            set
            {
                if (Equals(value, _milestone)) return;
                _milestone = value;
                OnPropertyChanged();
                OnPropertyChanged("Issues");
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

        /// <summary>
        /// The filtered list of issues
        /// </summary>
        [NotNull]
        public IEnumerable<Issue> Issues
        {
            get
            {
                IEnumerable<Issue> issues = AllIssues;

                // Labels
                if (Label != null && Label != _allLabels)
                    issues = issues.Where(i => i.Labels.Any(l => l.Name == Label.Name));

                // Milestones
                if (Milestone == NoMilestone)
                    issues = issues.Where(i => i.Milestone == null);
                else if (Milestone != AllMilestones && Milestone != null)
                    issues = issues.Where(i => i.Milestone != null && i.Milestone.Number == Milestone.Number);

                // Users
                issues = issues.Filter(User, UserFilter);

                // Search Text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    issues = issues.Where(i => i.Title.ToLower().Contains(search) || i.Body.ToLower().Contains(search) );
                }

                return issues;
            }
        }

        /// <summary>
        /// All of the open issues for a repository
        /// </summary>
        [NotNull]
        public BindingList<Issue> AllIssues { get; private set; }

        /// <summary>
        /// Gets a list of the possible user filters
        /// </summary>
        public IList<UserFilterType> UserFilters { get; private set; } 

        /// <summary>
        /// Should we fetch issues assigned to the user, unassigned, etc.
        /// </summary>
        public UserFilterType UserFilter
        {
            get { return _userFilterType; }
            set
            {
                if (value == _userFilterType) return;
                _userFilterType = value;
                OnPropertyChanged();
                OnPropertyChanged("Issues");
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

        /// <summary>
        /// Text to search the issues for
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged("Issues");
            }
        }

        [NotNull]
        protected ICache SettingsCache
        {
            get;
            private set;
        }

        /// <summary>
        /// The milestone that indicates we search on all milestones.
        /// </summary>
        /// <remarks>Public for testing</remarks>
        public Milestone AllMilestones { get; private set; }

        /// <summary>
        /// The milestone that indicates we search only issues with no milestone.
        /// </summary>
        /// <remarks>Public for testing</remarks>
        public Milestone NoMilestone { get; private set; }

        #endregion

        #region Abstract public API

        public abstract bool HasClientId
        {
            get;
        }

        public virtual void Logout()
        {
            LoggedIn = false;
        }

        public abstract Task Login(string accessToken);

        public abstract void GetIssues();

        public abstract void GetMilestones();

        public abstract void GetLabels();

        protected abstract void GetComments(Issue issue);

        public abstract void AddComment(Issue issue, string comment);

        public abstract void CloseIssue(Issue issue, string comment);

        public abstract Task<IReadOnlyList<User>> GetAssignees(Repository repository);

        public abstract void SaveIssue(Repository repository, NewIssue newIssue);

        public abstract void UpdateIssue(Repository repository, int id, IssueUpdate update);
        
        public bool SetRepositoryForSolution(string solutionName)
        {
            if (string.IsNullOrWhiteSpace(solutionName))
                return false;
            
            var options = Factory.Get<IOptionsProvider>();
            if (options != null && options.Options != null && options.Options.DisableAutoSelectRepository)
            {
                return false;
            }

            var remotes = RepositoryHelper.GetRemotes(solutionName);
            var repo = Repositories.Where(r => r.HasRemote(remotes))
                                   .FirstOrDefault();
            if(repo != null)
            {
                Repository = repo;
                return true;
            }
            return false;
        }

        #endregion

        #region Abstract proteced API

        protected virtual void GetRepositoryInfo()
        {
            GetLabels();
            GetMilestones();
            GetIssues();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull, CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}