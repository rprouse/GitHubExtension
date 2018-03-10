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
using System.Linq;
using Alteridem.GitHub.Extension.Test.Mocks;
using Alteridem.GitHub.Model;
using Moq;
using NUnit.Framework;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class GitHubApiBaseTest
    {
        private Mock<GitHubApiBase> _moq;
        private GitHubApiBase _api;
        private Issue _issue;
        private Label _label;
        private Milestone _milestone;

        [SetUp]
        public void SetUp()
        {
            var cache = new Mock<ICache>().Object;
            _moq = new Mock<GitHubApiBase>(cache);
            _api = _moq.Object;

            _label = new Label(null, "One", "");

            _milestone = MockData.CreateTestMilestone(1, ItemState.Open, "1.0", "", DateTimeOffset.Now, null);

            _issue = MockData.CreateTestIssue(1, ItemState.Open, "Title one", "Body one", null, DateTimeOffset.Now, _milestone, new List<Label> { _label });
            _api.AllIssues.Add(_issue);

            var issue2 = MockData.CreateTestIssue(2, ItemState.Open, "Two", "Two", null, DateTimeOffset.Now, MockData.CreateTestMilestone(2, ItemState.Open, "2.0", "", DateTimeOffset.Now, null), new List<Label>());
            _api.AllIssues.Add(issue2);

            var issue3 = MockData.CreateTestIssue(2, ItemState.Open, "Three", "Three", null, DateTimeOffset.Now, null, new List<Label>());
            _api.AllIssues.Add(issue3);
        }

        [Test]
        public void IssuesAreFilteredByLabels()
        {
            _api.Label = _label;
            Assert.That(_api.Issues.Count(), Is.EqualTo(1));
            Assert.That(_api.Issues, Contains.Item(_issue));
        }

        [Test]
        public void IssuesAreSearchedByTitleCaseInsensitive()
        {
            _api.SearchText = "title";
            Assert.That(_api.Issues.Count(), Is.EqualTo(1));
            Assert.That(_api.Issues, Contains.Item(_issue));
        }

        [Test]
        public void IssuesAreSearchedByBodyCaseInsensitive()
        {
            _api.SearchText = "body";
            Assert.That(_api.Issues.Count(), Is.EqualTo(1));
            Assert.That(_api.Issues, Contains.Item(_issue));
        }

        [Test]
        public void IssuesCanBeFilteredByAllMilestones()
        {
            _api.Milestone = _api.AllMilestones;
            Assert.That(_api.Issues.Count(), Is.EqualTo(3));
        }

        [Test]
        public void IssuesCanBeFilteredByMilestone()
        {
            _api.Milestone = _milestone;
            Assert.That(_api.Issues.Count(), Is.EqualTo(1));
            Assert.That(_api.Issues, Contains.Item(_issue));
        }

        [Test]
        public void IssuesCanBeFilteredByNoMilestone()
        {
            _api.Milestone = _api.NoMilestone;
            Assert.That(_api.Issues.Count(), Is.EqualTo(1));
            var issue = _api.Issues.FirstOrDefault();
            Assert.That(issue, Is.Not.Null);
            Assert.That(issue.Milestone, Is.Null);
            Assert.That(issue.Title, Is.EqualTo("Three"));
        }
    }
}