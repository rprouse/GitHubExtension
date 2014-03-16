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
            _issue.Title = "new title";
            _issue.Body = "new body";

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