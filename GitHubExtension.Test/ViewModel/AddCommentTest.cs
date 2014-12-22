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

using System.ComponentModel.Composition.Hosting;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.Test.Mocks;
using Alteridem.GitHub.Extension.ViewModel;
using Alteridem.GitHub.Model;
using NUnit.Framework;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.Test.ViewModel
{
    [TestFixture]
    public class AddCommentTest
    {
        private AddCommentViewModel _gitHubViewModel;

        [SetUp]
        public void SetUp()
        {
            var issuesAssemblyCatalog = new AssemblyCatalog(typeof(Cache).Assembly);
            var mockServicesCatalog = new TypeCatalog(typeof(Model.CacheTest.MockServiceProvider));
            var catalog = new AggregateCatalog(issuesAssemblyCatalog, mockServicesCatalog);
            var exportProvider = new CompositionContainer(catalog);

            Factory.Rebind<ICache>().ToConstant(exportProvider.GetExportedValue<Cache>());
            Factory.Rebind<GitHubApiBase>().To<GitHubApiMock>().InScope(o => this);
            Factory.Rebind<IAddComment>().To<AddCommentMock>();
            _gitHubViewModel = Factory.Get<AddCommentViewModel>();
        }

        [Test]
        public void TestCanCloseIssue()
        {
            Assert.That(_gitHubViewModel.CanCloseIssue(), Is.True);
        }

        [Test]
        public void TestCanCommentOnIssue()
        {
            _gitHubViewModel.Comment = "";
            Assert.That(_gitHubViewModel.CanCommentOnIssue(), Is.False);
            _gitHubViewModel.Comment = "comment";
            Assert.That(_gitHubViewModel.CanCommentOnIssue(), Is.True);
        }

        [Test]
        public void TestCommentOnIssue()
        {
            var dlg = Factory.Get<IAddComment>();
            var api = Factory.Get<GitHubApiBase>();
            api.IssueMarkdown = "Issue body";

            _gitHubViewModel.Comment = "TestCommentOnIssue";
            _gitHubViewModel.OnCommentOnIssue(dlg);

            Assert.That(api.IssueMarkdown, Contains.Substring("TestCommentOnIssue"));
        }

        [Test]
        public void TestCloseIssue()
        {
            var dlg = Factory.Get<IAddComment>();
            var api = Factory.Get<GitHubApiBase>();
            api.Issue.State = ItemState.Open;
            api.IssueMarkdown = "Issue body";

            _gitHubViewModel.Comment = "TestCommentOnIssue";
            _gitHubViewModel.OnCloseIssue(dlg);

            Assert.That(api.Issue.State, Is.EqualTo(ItemState.Closed));
            Assert.That(api.IssueMarkdown, Contains.Substring("TestCommentOnIssue"));
        }
    }
}