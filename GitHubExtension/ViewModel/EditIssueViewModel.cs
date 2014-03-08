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

using System.ComponentModel;
using System.Windows.Input;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Octokit;

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class EditIssueViewModel : BaseViewModel
    {
        private readonly IIssueEditor _editor;
        private readonly Repository _repository;
        private Issue _issue;
        private BindingList<Label> _labels;

        public EditIssueViewModel(IIssueEditor editor)
        {
            _editor = editor;
            if (GitHubApi.Repository != null)
                _repository = GitHubApi.Repository.Repository;

            Labels = new BindingList<Label>(GitHubApi.Labels);
            Milestones = new BindingList<Milestone>(GitHubApi.Milestones);

            // The lists contain non-items
            if (Labels.Count > 0) Labels.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);
            if (Milestones.Count > 0) Milestones.RemoveAt(0);

            SaveCommand = new RelayCommand(p => Save(), p => CanSave());
            CancelCommand = new RelayCommand(p => _editor.Close(), p => true);
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public Issue Issue
        {
            get { return _issue; }
            set
            {
                if (Equals(value, _issue)) return;
                _issue = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// All the labels available
        /// </summary>
        public BindingList<Label> Labels { get; set; }

        /// <summary>
        /// All Milestones available
        /// </summary>
        public BindingList<Milestone> Milestones { get; set; }

        public void Save()
        {
            // TODO:
        }

        public bool CanSave()
        {
            return Issue != null && _repository != null && !string.IsNullOrWhiteSpace(Issue.Title);
        }
    }
}