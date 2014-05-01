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
using Alteridem.GitHub.Model;
using NUnit.Framework;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class RepositoryWrapperTest
    {
        private RepositoryWrapper _one;
        private RepositoryWrapper _two;
        private RepositoryWrapper _three;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _one = CreateWrapper(1, "test");
            _two = CreateWrapper(1, "test");
            _three = CreateWrapper(3, "three");
        }

        private RepositoryWrapper CreateWrapper(int id, string name)
        {
            var repository = new Repository
            {
                Id = id,
                Name = name
            };
            return new RepositoryWrapper(repository);
            
        }

        [Test]
        public void TestEquals()
        {
            Assert.That(_one, Is.EqualTo(_two));
            Assert.That(_one.Equals(_two), Is.True);
        }

        [Test]
        public void TestNotEquals()
        {
            Assert.That(_one, Is.Not.EqualTo(_three));
            Assert.That(_one.Equals(_three), Is.False);
        }
    }
}