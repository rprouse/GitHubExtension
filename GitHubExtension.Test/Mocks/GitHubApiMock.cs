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
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Alteridem.GitHub.Model;
using NUnit.Framework;
using Octokit;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class GitHubApiMock : GitHubApiBase
    {
        public GitHubApiMock(ICache settingsCache)
            : base(settingsCache)
        {
            // Log in
            Login("test", "test", null);

            // Set up a repository
            var repository = MockData.CreateTestRepository(1, "test");
            var wrapper = new RepositoryWrapper(repository);
            Repository = wrapper;
            Repositories.Add(wrapper);

            GetLabels();
            GetMilestones();
        }

        public override bool HasClientId => true;

        public override Task Login(string username, string password, string accessToken)
        {
            Token = username + password;
            User = MockData.CreateTestUser("nobody@nobody.com", username);
            return new Task(() => Console.WriteLine("Logging In"));
        }

        public override void GetIssues()
        {
            var issue = MockData.CreateTestIssue(1, ItemState.Open, "title", "##body##", MockData.CreateTestUser("nobody@nobody.com", "user"), DateTimeOffset.Now);
            AllIssues.Add(issue);
            Issue = issue;
            IssueMarkdown = issue.Body;
        }

        public override void GetLabels()
        {
            var label = new Label(null, "Bug", "FF0000");
            Label = label;
            Labels.Add(label);
        }

        public override void GetMilestones()
        {
            var milestone = MockData.CreateTestMilestone(1, ItemState.Open, "v1.0", "The first release", DateTimeOffset.Now, null);
            Milestone = milestone;
            Milestones.Add(milestone);
        }

        public override void UpdateIssue(Repository repository, int id, IssueUpdate update)
        {
            Issue = MockData.CreateTestIssue(id, ItemState.Open, update.Title, update.Body, null, DateTimeOffset.Now);
        }

        public override void SaveIssue(Repository repository, NewIssue newIssue)
        {
            var issue = MockData.CreateTestIssue(69, ItemState.Open, newIssue.Title, newIssue.Body, null, DateTimeOffset.Now);
            AllIssues.Add(issue);
            Issue = issue;
        }

        public override Task<IReadOnlyList<User>> GetAssignees(Repository repository)
        {
            IReadOnlyList<User> list = new List<User>
            {
                MockData.CreateTestUser("nobody@nobody.com", "user")
            };
            return Task.FromResult(list);
        }

        public override void CloseIssue(Issue issue, string comment)
        {
            AddComment(issue, comment);
            Issue = MockData.CreateTestIssue(Issue.Number, ItemState.Closed, Issue.Title, Issue.Body, null, Issue.CreatedAt);
        }

        public override void AddComment(Issue issue, string comment)
        {
            IssueMarkdown += comment;
        }

        protected override void GetComments(Issue issue)
        {
            IssueMarkdown += "\r\nComment";
        }
    }
}