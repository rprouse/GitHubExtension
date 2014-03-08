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

using System.ComponentModel;
using System.Windows.Input;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Model;
using Microsoft.VisualStudio.Shell;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class IssueListViewModel : BaseViewModel
    {
        public IssueListViewModel()
        {
            RefreshCommand = new RelayCommand(p => Refresh(), p => CanRefresh());
        }

        public ICommand RefreshCommand { get; private set; }

        public RepositoryWrapper Repository
        {
            get { return GitHubApi.Repository; }
            set { GitHubApi.Repository = value; }
        }

        public Label Label
        {
            get { return GitHubApi.Label; }
            set { GitHubApi.Label = value; }
        }

        public Milestone Milestone
        {
            get { return GitHubApi.Milestone; }
            set { GitHubApi.Milestone = value; }
        }

        public Issue Issue
        {
            get { return GitHubApi.Issue; }
            set { GitHubApi.Issue = value; }
        }

        [NotNull]
        public BindingList<RepositoryWrapper> Repositories { get { return GitHubApi.Repositories; } }

        [NotNull]
        public BindingList<Label> Labels { get { return GitHubApi.Labels; } }

        [NotNull]
        public BindingList<Milestone> Milestones { get { return GitHubApi.Milestones; } }

        [NotNull]
        public BindingList<Organization> Organizations { get { return GitHubApi.Organizations; } }

        [NotNull]
        public BindingList<Issue> Issues { get { return GitHubApi.Issues; } }

        public void OpenIssueViewer()
        {
            var viewer = ServiceProvider.GlobalProvider.GetService(typeof(IIssueToolWindow)) as IIssueToolWindow;
            if (viewer != null)
            {
                viewer.Show();
            }
        }

        private void Refresh()
        {
            GitHubApi.GetIssues();
        }

        private bool CanRefresh()
        {
            return true;
        }
    }
}