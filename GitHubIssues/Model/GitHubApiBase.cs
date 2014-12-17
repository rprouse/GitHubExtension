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
using Octokit;

namespace Alteridem.GitHub.Model
{
    public abstract class GitHubApiBase : INotifyPropertyChanged
    {
        #region Members

        private string _token;
        private User _user;
        private RepositoryWrapper _repository;
        private IssueFilter _filter;
        private Label _label;
        private Milestone _milestone;
        private Issue _issue;
        private string _markdown = string.Empty;
        protected readonly Label _allLabels;
        protected readonly Milestone _noMilestone;
        protected readonly Milestone _allMilestones;

        #endregion

        protected GitHubApiBase(Cache settingsCache)
        {
            if (settingsCache == null)
                throw new ArgumentNullException("settingsCache");

            _allLabels = new Label { Color = "FFFFFFFF", Name = "All Labels" };
            _allMilestones = new Milestone { Number = 0, Title = "All Milestones", OpenIssues = 0 };
            _noMilestone = new Milestone { Number = -1, Title = "No Milestone", OpenIssues = 0 };

            SettingsCache = settingsCache;
            _filter = IssueFilter.Assigned;
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

                if (Label != null && Label != _allLabels)
                    issues = issues.Where(i => i.Labels.Any(l => l.Name == Label.Name));

                if (Milestone == _noMilestone)
                    issues = issues.Where(i => i.Milestone == null);
                else if (Milestone == _allMilestones)
                    issues = issues.Where(i => i.Milestone != null);
                else if (Milestone != null)
                    issues = issues.Where(i => i.Milestone != null && i.Milestone.Number == Milestone.Number);

                return issues;
            }
        }

        /// <summary>
        /// All of the open issues for a repository
        /// </summary>
        [NotNull]
        public BindingList<Issue> AllIssues { get; set; }

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

        [NotNull]
        protected Cache SettingsCache
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

        public abstract Task<bool> Login(string username, string password, string accessToken);

        public abstract void GetIssues();

        public abstract void GetComments(Issue issue);

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