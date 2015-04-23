using System;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Model;
using Octokit;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class IssueEditorMock : IIssueEditor
    {
        private Issue _issue;

        public void Close()
        {
        }

        public bool? ShowModal()
        {
            _issue = new Issue(null, null, _issue.Number, ItemState.Open, "new title", "new body", null, null, null, null, 1, null, null, DateTimeOffset.Now, null);

            var api = Factory.Get<GitHubApiBase>();
            api.Issue = _issue;

            return true;
        }

        public bool IsEnabled { get; set; }

        /// <summary>
        /// Sets the issue to add/edit
        /// </summary>
        /// <param name="issue">The issue.</param>
        public void SetIssue(Issue issue)
        {
            _issue = issue ?? new Issue();
        }
    }
}