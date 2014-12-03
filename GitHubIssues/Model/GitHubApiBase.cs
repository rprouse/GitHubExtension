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

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Alteridem.GitHub.Annotations;
using NLog;
using Octokit;

namespace Alteridem.GitHub.Model
{
    public abstract class GitHubApiBase : INotifyPropertyChanged
    {
        #region Members

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string _token;
        private User _user;
        private RepositoryWrapper _repository;
        private IssueFilter _filter;
        private Label _label;
        private Milestone _milestone;
        private Issue _issue;
        private string _markdown = string.Empty;

        #endregion

        public GitHubApiBase()
        {
            _filter = IssueFilter.Assigned;
            Repositories = new BindingList<RepositoryWrapper>();
            Organizations = new BindingList<Organization>();
            Issues = new BindingList<Issue>();
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
                    Cache.Repository = value.Repository.Id; // Save
                else
                    Cache.Repository = 0;   // Delete
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

        #endregion

        #region Abstract public API

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