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

#endregion

namespace Alteridem.GitHub.Extension.Test.Model
{
    [TestFixture]
    public class CacheTest
    {
        [Test]
        public void TestRepository()
        {
            Cache.Repository = 0;
            Assert.That(Cache.Repository, Is.EqualTo(0));

            Cache.Repository = 2;
            Assert.That(Cache.Repository, Is.EqualTo(2));
        }

        [Test]
        public void TestCredentials()
        {
            Cache.Credentials = new CredentialCache { Logon = "user", Password = "password" };
            var cached = Cache.Credentials;
            Assert.That(cached, Is.Not.Null);
            Assert.That(cached.Logon, Is.EqualTo("user"));
            Assert.That(cached.Password, Is.EqualTo("password"));

            Cache.Credentials = null;
            Assert.That(Cache.Credentials, Is.Null);
        }
    }
}