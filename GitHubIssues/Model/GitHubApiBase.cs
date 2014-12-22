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

namespace Alteridem.GitHub.Model
{
    public abstract class GitHubApiBase : INotifyPropertyChanged
    {
        #region Members

        private string _token;
        private User _user;
        private RepositoryWrapper _repository;
        private Label _label;
        private Milestone _milestone;
        private Issue _issue;
        private string _markdown = string.Empty;
        protected readonly Label _allLabels;
        protected readonly Milestone _noMilestone;
        protected readonly Milestone _allMilestones;
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

            _allLabels = new Label { Color = "FFFFFFFF", Name = "All Labels" };
            _allMilestones = new Milestone { Number = 0, Title = "All Milestones", OpenIssues = 0 };
            _noMilestone = new Milestone { Number = -1, Title = "No Milestone", OpenIssues = 0 };

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
            get { return !string.IsNullOrWhiteSpace(Token); }
        }

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

        [CanBeNull]
        protected string Token
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
                if (Milestone == _noMilestone)
                    issues = issues.Where(i => i.Milestone == null);
                else if (Milestone != null && Milestone != _allMilestones)
                    issues = issues.Where(i => i.Milestone != null && i.Milestone.Number == Milestone.Number);

                // Users
                issues = issues.Filter(User, UserFilter);

                // Search Text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string search = SearchText.ToLower();
                    issues = issues.Where(i => i.Title.ToLower().Contains(search) || i.Body.ToLower().Contains(search));
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

        #endregion

        #region Abstract public API

        public abstract bool HasClientId
        {
            get;
        }

        public virtual void Logout()
        {
            Token = string.Empty;
        }

        public abstract Task Login(string username, string password, string accessToken);

        public abstract void GetIssues();

        protected abstract void GetComments(Issue issue);

        public abstract void AddComment(Issue issue, string comment);

        public abstract void CloseIssue(Issue issue, string comment);

        public abstract Task<IReadOnlyList<User>> GetAssignees(Repository repository);

        public abstract void SaveIssue(Repository repository, NewIssue newIssue);

        public abstract void UpdateIssue(Repository repository, int id, IssueUpdate update);

        #endregion

        #region Abstract proteced API
        
        protected virtual void GetRepositoryInfo()
        {
            GetLabels();
            GetMilestones();
            GetIssues();
        }

        protected abstract void GetLabels();

        protected abstract void GetMilestones();

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