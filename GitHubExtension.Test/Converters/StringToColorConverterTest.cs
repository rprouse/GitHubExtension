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
using System.Windows.Media;
using Alteridem.GitHub.Converters;
using NUnit.Framework;
using NUnit.Framework.Constraints;

#endregion

namespace Alteridem.GitHub.Extension.Test.Converters
{
    [TestFixture]
    public class StringToColorConverterTest
    {
        [TestCase("FF0000", 0xFF, 0x00, 0x00)]
        [TestCase("FFFFFF", 0xFF, 0xFF, 0xFF)]
        [TestCase("336699", 0x33, 0x66, 0x99)]
        [TestCase("FFF", 0xFF, 0xFF, 0xFF)]
        public void TestColorConversion(string str, byte r, byte g, byte b)
        {
            var converter = new StringToColorConverter();
            var result = converter.Convert(str, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture) as SolidColorBrush;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Color.A, Is.EqualTo(0xFF));
            Assert.That(result.Color.R, Is.EqualTo(r));
            Assert.That(result.Color.G, Is.EqualTo(g));
            Assert.That(result.Color.B, Is.EqualTo(b));
        }

        [TestCase("")]
        [TestCase("#FFFFFF")]
        [TestCase("Red")]
        public void TestInvalidColorReturnsTransparent(string str)
        {
            var converter = new StringToColorConverter();
            var result = converter.Convert(str, typeof(SolidColorBrush), null, CultureInfo.CurrentCulture) as SolidColorBrush;
            Assert.That( result, Is.Not.Null );
            Assert.That( result.Color.A, Is.EqualTo( 0x00 ) );
        }
    }
}