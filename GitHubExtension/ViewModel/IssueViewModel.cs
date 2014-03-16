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

using System.Windows.Input;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Octokit;
using Issue = Octokit.Issue;

#endregion

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class IssueViewModel : BaseViewModel
    {
        private ICommand _addCommentCommand;
        private ICommand _editIssueCommand;

        public IssueViewModel()
        {
            AddCommentCommand = new RelayCommand(p => AddComment(), p => CanAddComment());
            EditIssueCommand = new RelayCommand(p => EditIssue(), p => CanEditIssue());
        }

        public Issue Issue { get { return GitHubApi.Issue; } }

        public string IssueMarkdown { get { return GitHubApi.IssueMarkdown; } }

        public ICommand AddCommentCommand
        {
            get { return _addCommentCommand; }
            private set
            {
                if (Equals(value, _addCommentCommand)) return;
                _addCommentCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand EditIssueCommand
        {
            get { return _editIssueCommand; }
            set
            {
                if (Equals(value, _editIssueCommand)) return;
                _editIssueCommand = value;
                OnPropertyChanged();
            }
        }

        public void AddComment()
        {
            var dlg = Factory.Get<IAddComment>();
            dlg.ShowModal();
        }

        public bool CanAddComment()
        {
            return GitHubApi.Issue != null;
        }

        public void EditIssue()
        {
            var dlg = Factory.Get<IIssueEditor>();
            dlg.SetIssue(GitHubApi.Issue);
            dlg.ShowModal();
        }

        public bool CanEditIssue()
        {
            return GitHubApi.Issue != null;
        }
    }
}