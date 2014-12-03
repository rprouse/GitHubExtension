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
    public class CredentialCacheTest
    {
        [Test]
        public void TestRoundTrip()
        {
            var credentials = new CredentialCache { Logon = "user", Password = "password", AccessToken = string.Empty };
            var cache = credentials.ToString();
            Assert.That(cache, Is.Not.Null);
            Assert.That(cache, Is.StringContaining("user"));
            Assert.That(cache, Is.Not.StringContaining("password"));

            var parsed = CredentialCache.FromString(cache);
            Assert.That(parsed, Is.Not.Null);
            Assert.That(parsed.Logon, Is.EqualTo("user"));
            Assert.That(parsed.Password, Is.EqualTo("password"));
            Assert.That(parsed.AccessToken, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FromNullStringReturnsNull()
        {
            Assert.That(CredentialCache.FromString(null), Is.Null);
        }

        [Test]
        public void FromEmptyStringReturnsNull()
        {
            Assert.That(CredentialCache.FromString(string.Empty), Is.Null);
        }

        [Test]
        public void FromBlankStringReturnsNull()
        {
            Assert.That(CredentialCache.FromString("   "), Is.Null);
        }
    }
}