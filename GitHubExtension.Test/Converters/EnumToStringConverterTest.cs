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
using System.Globalization;
using System.Runtime.InteropServices;
using Alteridem.GitHub.Converters;
using NUnit.Framework;

#endregion

namespace Alteridem.GitHub.Extension.Test.Converters
{
    [TestFixture]
    public class EnumToStringConverterTest
    {
        private readonly EnumToStringConverter _converter = new EnumToStringConverter();

        public enum TestEnum
        {
            None,
            ThreeBlindMice
        }

        [TestCase(TestEnum.None, "None")]
        [TestCase(TestEnum.ThreeBlindMice, "Three Blind Mice")]
        public void CanConvertEnumToString(TestEnum value, string expected)
        {
            Assert.That(_converter.Convert(value, typeof(string), null, CultureInfo.InvariantCulture), Is.EqualTo(expected));
        }

        [TestCase(TestEnum.None, "None")]
        [TestCase(TestEnum.ThreeBlindMice, "Three Blind Mice")]
        public void CanConvertStringToEnum(TestEnum expected, string value)
        {
            Assert.That(_converter.ConvertBack(value, typeof(TestEnum), null, CultureInfo.InvariantCulture), Is.EqualTo(expected));
        }

        [Test]
        public void NullConvertsToEmptyString()
        {
            Assert.That(_converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture), Is.EqualTo(string.Empty));
        }

        [Test]
        public void NullValueThrowsArgumentNullExceptions()
        {
            Assert.That(() => _converter.ConvertBack(null, typeof(string), null, CultureInfo.InvariantCulture), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public void NotEnumTypeThrowsArgumentExceptions()
        {
            Assert.That(() => _converter.ConvertBack("Three Blind Mice", typeof(int), null, CultureInfo.InvariantCulture), Throws.ArgumentException);
        }

        [Test]
        public void InvalidEnumValueThrowsFormatException()
        {
            Assert.That(() => _converter.ConvertBack("Bad Value", typeof(TestEnum), null, CultureInfo.InvariantCulture), Throws.ArgumentException);
        }
    }
}
