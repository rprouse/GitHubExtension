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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class EditIssueViewModel : BaseViewModel
    {
        private readonly IIssueEditor _editor;
        private readonly Repository _repository;
        private BindingList<Label> _labels;
        private int _issueNumber;
        private Milestone _milestone;
        private User _assignee;
        private string _body = string.Empty;
        private string _title = string.Empty;
        private BindingList<Milestone> _milestones;
        private BindingList<Label> _allLabels;
        private BindingList<User> _assignees;

        public EditIssueViewModel(IIssueEditor editor)
        {
            _editor = editor;
            if (GitHubApi.Repository != null)
                _repository = GitHubApi.Repository.Repository;

            Assignees = new BindingList<User>();

            Labels = new BindingList<Label>();
            AllLabels = new BindingList<Label>();
            foreach (var label in GitHubApi.Labels)
                AllLabels.Add(label);
            Milestones = new BindingList<Milestone>();
            foreach (var milestone in GitHubApi.Milestones)
                Milestones.Add(milestone);

            // The lists contain non-items
            if (AllLabels.Count > 0) AllLabels.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);

            SaveCommand = new RelayCommand(p => Save(), p => CanSave());
            CancelCommand = new RelayCommand(p => _editor.Close(), p => true);
            ClearAssigneeCommand = new RelayCommand(p => Assignee = null, p => Assignee != null);
            ClearMilestoneCommand = new RelayCommand(p => Milestone = null, p => Milestone != null);
        }

        /// <summary>
        /// Sets the issue to add/edit. If null, we are adding, if set, we edit
        /// </summary>
        /// <param name="issue">The issue.</param>
        public void SetIssue(Issue issue)
        {
            LoadAssignees(issue);

            if(issue == null)
                return;

            _issueNumber = issue.Number;
            Title = issue.Title;
            Body = issue.Body;
            Assignee = issue.Assignee;
            if (issue.Labels != null)
                foreach (var label in issue.Labels)
                    AddLabel(label);
            SetMilestone(issue.Milestone);
        }

        /// <summary>
        /// Labels don't override Equals, so we need to find the ones in
        /// the main collection and add them so that reference equals works
        /// for binding
        /// </summary>
        /// <param name="label"></param>
        private void AddLabel(Label label)
        {
            var labels = from l in AllLabels
                         where l.Url == label.Url
                         select l;
            labels.ToList().ForEach(l => Labels.Add(l));
        }

        /// <summary>
        /// Milestones don't override Equals, so we need to find the one in
        /// the main collection and add it so that reference equals works
        /// for binding
        /// </summary>
        /// <param name="milestone"></param>
        private void SetMilestone(Milestone milestone)
        {
            Milestone = (from m in Milestones
                         where m.Url == milestone.Url
                         select m).FirstOrDefault();
        }

        /// <summary>
        /// Users don't override Equals, so we need to find the one in
        /// the main collection and add it so that reference equals works
        /// for binding
        /// </summary>
        /// <param name="assignee"></param>
        private void SetAssignee(User assignee)
        {
            Assignee = (from u in Assignees
                        where u.Url == assignee.Url
                        select u).FirstOrDefault();
        }

        private async void LoadAssignees(Issue issue)
        {
            if (GitHubApi.Repository != null)
            {
                IReadOnlyList<User> assignees = await GitHubApi.GetAssignees(_repository);
                foreach (var assignee in assignees)
                    Assignees.Add(assignee);

                if (issue != null && issue.Assignee != null)
                    SetAssignee(issue.Assignee);
            }
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand ClearAssigneeCommand { get; private set; }
        public ICommand ClearMilestoneCommand { get; private set; }

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Body
        {
            get { return _body; }
            set
            {
                if (value == _body) return;
                _body = value;
                OnPropertyChanged();
            }
        }

        public User Assignee
        {
            get { return _assignee; }
            set
            {
                if (Equals(value, _assignee)) return;
                _assignee = value;
                OnPropertyChanged();
            }
        }

        public BindingList<Label> Labels
        {
            get { return _labels; }
            set
            {
                if (Equals(value, _labels)) return;
                _labels = value;
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
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All the labels available
        /// </summary>
        public BindingList<Label> AllLabels
        {
            get { return _allLabels; }
            set
            {
                if (Equals(value, _allLabels)) return;
                _allLabels = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All Milestones available
        /// </summary>
        public BindingList<Milestone> Milestones
        {
            get { return _milestones; }
            set
            {
                if (Equals(value, _milestones)) return;
                _milestones = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All people that can be assigned to this issue
        /// </summary>
        public BindingList<User> Assignees
        {
            get { return _assignees; }
            set
            {
                if (Equals(value, _assignees)) return;
                _assignees = value;
                OnPropertyChanged();
            }
        }

        public void Save()
        {
            _editor.IsEnabled = false;
            try
            {
                if (_issueNumber == 0) // New
                {
                    var issue = new NewIssue(Title);
                    issue.Body = Body;
                    issue.Assignee = Assignee != null ? Assignee.Login : string.Empty;
                    issue.Milestone = Milestone != null ? Milestone.Number : 0;

                    foreach (var label in Labels)
                        issue.Labels.Add(label.Name);

                    GitHubApi.SaveIssue(_repository, issue);
                }
                else  // Edit
                {
                    var issue = new IssueUpdate();
                    issue.Title = Title;
                    issue.Body = Body;
                    issue.Assignee = Assignee != null ? Assignee.Login : string.Empty;
                    issue.Milestone = Milestone != null ? Milestone.Number : 0;

                    foreach (var label in Labels)
                        issue.Labels.Add(label.Name);

                    GitHubApi.UpdateIssue(_repository, _issueNumber, issue);
                }
                _editor.Close();
            }
            catch (Exception e)
            {
                _editor.IsEnabled = true;
                MessageBox.Show(_editor.Window, "Failed to save issue. " + e.Message, "GitHub", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public bool CanSave()
        {
            return _repository != null && !string.IsNullOrWhiteSpace(Title);
        }
    }
}