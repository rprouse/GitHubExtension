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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Octokit;
using Issue = Octokit.Issue;

#endregion

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class IssueEditorViewModel : BaseGitHubViewModel
    {
        private readonly Repository _repository;
        private BindingList<LabelModel> _labels;
        private int _issueNumber;
        private Milestone _milestone;
        private User _assignee;
        private string _body = string.Empty;
        private string _title = string.Empty;
        private BindingList<Milestone> _milestones;
        private BindingList<User> _assignees;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public IssueEditorViewModel()
        {
            if (GitHubApi.Repository != null)
                _repository = GitHubApi.Repository.Repository;

            Assignees = new BindingList<User>();

            Labels = new BindingList<LabelModel>();
            foreach (var label in GitHubApi.Labels)
                Labels.Add(new LabelModel(label));

            Milestones = new BindingList<Milestone>();
            foreach (var milestone in GitHubApi.Milestones)
                Milestones.Add(milestone);

            // The lists contain non-items
            if (Labels.Count > 0) Labels.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);

            SaveCommand = new RelayCommand(Save, p => CanSave());
            CancelCommand = new RelayCommand(Cancel, p => true);
            ClearAssigneeCommand = new RelayCommand(p => Assignee = null, p => Assignee != null);
            ClearMilestoneCommand = new RelayCommand(p => Milestone = null, p => Milestone != null);
            SetLabelsCommand = new RelayCommand(SetLabels, p => true);
            CloseLabelPickerCommand = new RelayCommand(CloseLabelPicker, p => true);
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
            var labels = from l in Labels
                         where l.Url == label.Url
                         select l;
            labels.ToList().ForEach(l => l.Checked = true);
        }

        /// <summary>
        /// Milestones don't override Equals, so we need to find the one in
        /// the main collection and add it so that reference equals works
        /// for binding
        /// </summary>
        /// <param name="milestone"></param>
        private void SetMilestone(Milestone milestone)
        {
            if (milestone == null)
            {
                Milestone = null;
                return;
            }
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
        public ICommand SetLabelsCommand { get; private set; }
        public ICommand CloseLabelPickerCommand { get; private set; }

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

        public BindingList<LabelModel> Labels
        {
            get { return _labels; }
            set
            {
                if (Equals(value, _labels)) return;
                _labels = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<LabelModel> SelectedLabels
        {
            get { return _labels.Where(l => l.Checked); }
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

        public void Cancel(object o)
        {
            var closable = o as IClosable;
            if(closable != null)
                closable.Close();
        }

        public void Save(object o)
        {
            var editor = o as IIssueEditor;
            Action save = () =>
            {
                if (_issueNumber == 0) 
                    NewIssue();
                else
                    UpdateIssue();
            };

            if (editor == null)
            {
                try
                {
                    save.Invoke();
                }
                catch (Exception) { 
                    // TODO: Log
                }
                return;
            }

            editor.IsEnabled = false;
            try
            {
                save.Invoke();
                editor.Close();
            }
            catch (Exception e)
            {
                editor.IsEnabled = true;
                VisualStudioMessageBox.Show("Failed to save issue. " + e.Message);
            }
        }

        private void NewIssue()
        {
            var issue = new NewIssue(Title);
            issue.Body = Body;
            if (Assignee != null)
                issue.Assignees.Add(Assignee.Login);
            if (Milestone != null)
                issue.Milestone = Milestone.Number;

            foreach (var label in SelectedLabels)
                issue.Labels.Add(label.Name);

            GitHubApi.SaveIssue(_repository, issue);
        }

        private void UpdateIssue()
        {
            var issue = new IssueUpdate();
            issue.Title = Title;
            issue.Body = Body;
            if (Assignee != null) issue.Assignees.Add(Assignee.Login);
            issue.Milestone = Milestone != null ? (int?)Milestone.Number : null;

            foreach (var label in SelectedLabels)
                issue.AddLabel(label.Name);

            GitHubApi.UpdateIssue(_repository, _issueNumber, issue);
        }

        public bool CanSave()
        {
            return _repository != null && !string.IsNullOrWhiteSpace(Title);
        }

        private void SetLabels(object obj)
        {
            var dlg = Factory.Get<ILabelPicker>();
            dlg.SetViewModel(this);
            dlg.ShowModal();
            OnPropertyChanged("SelectedLabels");
        }

        private void CloseLabelPicker(object o)
        {
            var closable = o as IClosable;
            if (closable != null)
                closable.Close();
        }
    }
}