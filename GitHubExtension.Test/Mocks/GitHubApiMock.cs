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
            var repository = new Repository(null, null, null, null, null, null, null, 1, null, "test", "test\\test",
                "Test repository",
                null, null, false, false, 1, 1, 1, "master", 1, DateTimeOffset.Now.AddDays(-1),
                DateTimeOffset.Now.AddDays(-2), DateTimeOffset.Now.AddDays(-1),
                null,
                new User(null, null, null, 1, null, DateTimeOffset.Now, 1, "nobody@nobody.com", 1, 1, true, null, 1, 1,
                    null, "test", "test", 1, null, 0, 0, 0, null, false),
                null, null, true, false, true);
            var wrapper = new RepositoryWrapper(repository);
            Repository = wrapper;
            Repositories.Add(wrapper);

            GetLabels();
            GetMilestones();
        }

        public override bool HasClientId
        {
            get
            {
                return true;
            }
        }

        public override Task Login(string username, string password, string accessToken)
        {
            Token = username + password;
            User = new User("https://avatars.githubusercontent.com/u/493828?v=1", null, null, 1, null, DateTimeOffset.Now, 1, "nobody@nobody.com", 1, 1, true, null, 1,
                1, null, username, username, 1, null, 0, 0, 0, null, false);
            return new Task(() => Console.WriteLine("Logging In"));
        }

        public override void GetIssues()
        {
            var issue = new Issue(null, null, 1, ItemState.Open, "title", "##body##",
                new User("https://avatars.githubusercontent.com/u/493828?v=1", null, null, 1, null, DateTimeOffset.Now,
                    1, "nobody@nobody.com", 1, 1, true, null, 1,
                    1, null, "user", "name", 1, null, 0, 0, 0, null, false), null, null, null, 1, null, null,
                DateTimeOffset.Now, null);
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
            var milestone = new Milestone(null, 1, ItemState.All, "v1.0", "The first release", null, 1, 0, DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
            Milestone = milestone;
            Milestones.Add(milestone);
        }

        public override void UpdateIssue(Repository repository, int id, IssueUpdate update)
        {
            Issue = new Issue(null, null, 0, ItemState.Open, update.Title, update.Body, null, null, null, null, 0, null, null, DateTimeOffset.Now, null );
        }

        public override void SaveIssue(Repository repository, NewIssue newIssue)
        {
            var issue = new Issue(null, null, 69, ItemState.Open, newIssue.Title, newIssue.Body, null, null, null, null, 0, null, null, DateTimeOffset.Now, null);
            AllIssues.Add(issue);
            Issue = issue;
        }

        public override Task<IReadOnlyList<User>> GetAssignees(Repository repository)
        {
            IReadOnlyList<User> list = new List<User>
            {
                new User("https://avatars.githubusercontent.com/u/493828?v=1", null, null, 1, null, DateTimeOffset.Now,
                    1, "nobody@nobody.com", 1, 1, true, null, 1,
                    1, null, "user", "name", 1, null, 0, 0, 0, null, false)
            };
            return Task.FromResult(list);
        }

        public override void CloseIssue(Issue issue, string comment)
        {
            AddComment(issue, comment);
            Issue = new Issue(null, null, Issue.Number, ItemState.Closed, Issue.Title, Issue.Body, null, null, null, null, 1, null, null, Issue.CreatedAt, null);
            //issue.State = ItemState.Closed;
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