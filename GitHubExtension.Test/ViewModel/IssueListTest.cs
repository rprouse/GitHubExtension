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

#endregion

namespace Alteridem.GitHub.Extension.Test.ViewModel
{
    [TestFixture]
    public class IssueListTest
    {
        private IssueListViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            var issuesAssemblyCatalog = new AssemblyCatalog(typeof(Cache).Assembly);
            var mockServicesCatalog = new TypeCatalog(typeof(Model.CacheTest.MockServiceProvider));
            var catalog = new AggregateCatalog(issuesAssemblyCatalog, mockServicesCatalog);
            var exportProvider = new CompositionContainer(catalog);

            Factory.Rebind<ICache>().ToConstant(exportProvider.GetExportedValue<Cache>());
            Factory.Rebind<GitHubApiBase>().To<GitHubApiMock>().InScope(o => this);
            Factory.Rebind<IIssueEditor>().To<IssueEditorMock>();
            _viewModel = Factory.Get<IssueListViewModel>();
        }

        [Test]
        public void CanAddIssueTest()
        {
            var save = _viewModel.Repository;
            _viewModel.Repository = null;
            Assert.That(_viewModel.CanAddIssue(), Is.False);

            _viewModel.Repository = save;
            Assert.That(_viewModel.CanAddIssue(), Is.True);
        }

        [Test]
        public void AddIssueTest()
        {
            Assert.That(_viewModel.Issue, Is.Not.Null);
            Assert.That(_viewModel.Issue.Title, Is.Not.EqualTo("new title"));
            Assert.That(_viewModel.Issue.Body, Is.Not.EqualTo("new body"));

            _viewModel.AddIssue();

            Assert.That(_viewModel.Issue, Is.Not.Null);
            Assert.That(_viewModel.Issue.Title, Is.EqualTo("new title"));
            Assert.That(_viewModel.Issue.Body, Is.EqualTo("new body"));
        }
    }
}