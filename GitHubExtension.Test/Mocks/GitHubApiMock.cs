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
using System.Threading.Tasks;
using Alteridem.GitHub.Model;
using NUnit.Framework;
using Octokit;

namespace Alteridem.GitHub.Extension.Test.Mocks
{
    public class GitHubApiMock : GitHubApiBase
    {
        public GitHubApiMock(Cache settingsCache)
            : base(settingsCache)
        {
            // Log in
            Login("test", "test", null);

            // Set up a repository
            var repository = new Repository
            {
                CreatedAt = DateTime.Now,
                Description = "Test repository",
                FullName = "test\test",
                HasIssues = true,
                Id = 1,
                Name = "test",
                OpenIssuesCount = 1,
                Organization = new User
                {
                    Login = "test"
                }
            };
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

        public override Task<bool> Login(string username, string password, string accessToken)
        {
            Token = username + password;
            User = new User
            {
                Login = username,
                Name = username,
                AvatarUrl = "https://avatars.githubusercontent.com/u/493828?v=1"
            };
            return new Task<bool>(() => true);
        }

        public override void GetIssues()
        {
            var issue = new Issue
            {
                Body = "##body##",
                Number = 1,
                State = ItemState.Open,
                Title = "title",
                User = new User
                {
                    Login = "user",
                    Name = "name",
                    AvatarUrl = "https://avatars.githubusercontent.com/u/493828?v=1"
                }
            };
            Issue = issue;
            IssueMarkdown = issue.Body;
            Issues.Add(issue);
        }

        protected override void GetLabels()
        {
            var label = new Label
            {
                Color = "FF0000",
                Name = "Bug"
            };
            Label = label;
            Labels.Add(label);
        }

        protected override void GetMilestones()
        {
            var milestone = new Milestone
            {
                Title = "v1.0",
                Description = "The first release",
                DueOn = DateTime.Now.AddDays(1),
                Number = 1,
                OpenIssues = 1
            };
            Milestone = milestone;
            Milestones.Add(milestone);
        }

        public override void UpdateIssue(Repository repository, int id, IssueUpdate update)
        {
            Issue.Title = update.Title;
            Issue.Body = update.Body;
        }

        public override void SaveIssue(Repository repository, NewIssue newIssue)
        {
            var issue = new Issue
            {
                Number = 69,
                Title = newIssue.Title,
                Body = newIssue.Body
            };
            Issue = issue;
            Issues.Add(issue);
        }

        public override Task<IReadOnlyList<User>> GetAssignees(Repository repository)
        {
            return new Task<IReadOnlyList<User>>(null);
        }

        public override void CloseIssue(Issue issue, string comment)
        {
            AddComment(issue, comment);
            issue.State = ItemState.Closed;
        }

        public override void AddComment(Issue issue, string comment)
        {
            IssueMarkdown += comment;
        }

        public override void GetComments(Issue issue)
        {
            IssueMarkdown += "\r\nComment";
        }
    }
}