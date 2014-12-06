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
using System.Net;
using Alteridem.GitHub.Extension.Test.Mocks;
using Alteridem.GitHub.Logging;
using NUnit.Framework;
using Octokit;

#endregion

namespace Alteridem.GitHub.Extension.Test
{
    [TestFixture]
    public class OutputWriterTests
    {
        private OutputWriterMock _log = Factory.Get<IOutputWriter>() as OutputWriterMock;

        [SetUp]
        public void SetUp()
        {
            Assert.That(_log, Is.Not.Null);
            _log.WriteMessageCalled = false;
            _log.ShowMessageCalled = false;
            _log.LastMessageShown = string.Empty;
            _log.LastMessageWritten = string.Empty;
        }

        [TestCase(LogLevel.Debug, false, false)]
        [TestCase(LogLevel.Info, true, false)]
        [TestCase(LogLevel.Warn, true, false)]
        [TestCase(LogLevel.Error, true, true)]
        [TestCase(LogLevel.Fatal, true, true)]
        public void WriteMessageGoesToCorrectOutput(LogLevel level, bool wasWritten, bool wasShown)
        {
            _log.Write(level, "Test message");
            Assert.That(_log.WriteMessageCalled, Is.EqualTo(wasWritten));
            Assert.That(_log.ShowMessageCalled, Is.EqualTo(wasShown));
        }

        [TestCase(LogLevel.Debug, false, false)]
        [TestCase(LogLevel.Info, true, false)]
        [TestCase(LogLevel.Warn, true, false)]
        [TestCase(LogLevel.Error, true, true)]
        [TestCase(LogLevel.Fatal, true, true)]
        public void WriteMessageGoesToCorrectOutputWithParams(LogLevel level, bool wasWritten, bool wasShown)
        {
            _log.Write(level, "Test {0}", "message");
            Assert.That(_log.WriteMessageCalled, Is.EqualTo(wasWritten));
            Assert.That(_log.ShowMessageCalled, Is.EqualTo(wasShown));
        }

        [TestCase(LogLevel.Debug, false, false)]
        [TestCase(LogLevel.Info, true, false)]
        [TestCase(LogLevel.Warn, true, false)]
        [TestCase(LogLevel.Error, true, true)]
        [TestCase(LogLevel.Fatal, true, true)]
        public void WriteMessageGoesToCorrectOutputWithException(LogLevel level, bool wasWritten, bool wasShown)
        {
            _log.Write(level, "Test message", new ArgumentException());
            Assert.That(_log.WriteMessageCalled, Is.EqualTo(wasWritten));
            Assert.That(_log.ShowMessageCalled, Is.EqualTo(wasShown));
        }

        [Test]
        public void WritenMessageContainsMessage()
        {
            _log.Write(LogLevel.Info, "Test message");
            Assert.That(_log.LastMessageWritten, Is.StringEnding("Test message"));
        }

        [Test]
        public void ShownMessageContainsMessage()
        {
            _log.Write(LogLevel.Error, "Test message");
            Assert.That(_log.LastMessageShown, Is.StringContaining("Test message"));
        }

        [Test]
        public void WritenMessageContainsLogLevel()
        {
            _log.Write(LogLevel.Info, "Test message");
            Assert.That(_log.LastMessageWritten, Is.StringContaining("Info"));
        }

        [Test]
        public void WritenMessageContainsDate()
        {
            _log.Write(LogLevel.Info, "Test message");
            Assert.That(_log.LastMessageWritten, Is.StringStarting(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'")));
        }

        [Test]
        public void WritenMessageWithArgsContainsLogLevel()
        {
            _log.Write(LogLevel.Info, "Test {0}", "message");
            Assert.That(_log.LastMessageWritten, Is.StringContaining("Info"));
        }

        [Test]
        public void WritenMessageWithArgsContainsDate()
        {
            _log.Write(LogLevel.Info, "Test {0}", "message");
            Assert.That(_log.LastMessageWritten, Is.StringStarting(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'")));
        }

        [Test]
        public void WritenMessageWithExceptionContainsLogLevel()
        {
            _log.Write(LogLevel.Info, "Test message", new ApplicationException("Exception message"));
            Assert.That(_log.LastMessageWritten, Is.StringContaining("Info"));
        }

        [Test]
        public void WritenMessageWithExceptionContainsDate()
        {
            _log.Write(LogLevel.Info, "Test message", new ApplicationException("Exception message"));
            Assert.That(_log.LastMessageWritten, Is.StringStarting(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'")));
        }

        [Test]
        public void WritenMessageWithParamsContainsMessage()
        {
            _log.Write(LogLevel.Info, "Test {0}", "message");
            Assert.That(_log.LastMessageWritten, Is.StringEnding("Test message"));
        }

        [Test]
        public void WritenMessageWithExceptionContainsExceptionNameAndMessage()
        {
            _log.Write(LogLevel.Info, "Test message", new ApplicationException("Exception message"));
            Assert.That(_log.LastMessageWritten, Is.StringContaining("ApplicationException"));
            Assert.That(_log.LastMessageWritten, Is.StringContaining("Exception message"));
        }

        [Test]
        public void WritenMessageWithApiExceptionContainsDetailedApiInfo()
        {
            var response = new ResponseMock
            {
                ResponseUri = new Uri("http://localhost"),
                StatusCode = HttpStatusCode.NotFound,
                ContentType = "application/json",
                Body = "X-GitHub-Request-Id: 111"
            };
            var apiException = new ApiException(response);
            _log.Write(LogLevel.Info, "Test message", apiException);
            Assert.That(_log.LastMessageWritten, Is.StringContaining("ApiException"));
            Assert.That(_log.LastMessageWritten, Is.StringContaining("Status: NotFound"));
        }

        [Test]
        public void ShownMessageWithParamsContainsMessage()
        {
            _log.Write(LogLevel.Error, "Test {0}", "message");
            Assert.That(_log.LastMessageWritten, Is.StringEnding("Test message"));
        }

        [Test]
        public void ShownMessageWithExceptionContainsExceptionNameAndMessage()
        {
            _log.Write(LogLevel.Error, "Test message", new ApplicationException("Exception message"));
            Assert.That(_log.LastMessageShown, Is.StringContaining("ApplicationException"));
            Assert.That(_log.LastMessageShown, Is.StringContaining("Exception message"));
        }


    }
}