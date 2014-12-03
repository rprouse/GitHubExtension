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
using Alteridem.GitHub.Interfaces;
using NLog;

namespace Alteridem.GitHub.Extension.View
{
    public class ErrorReporter : IErrorReporter
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger( );

        #region Implementation of IErrorReporter

        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Show( string message )
        {
            log.Error( message );
            VisualStudioMessageBox.Show( message );
        }

        /// <summary>
        /// Shows the specified message to the user along with the exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Show( string message, Exception ex )
        {
            log.Error( message, ex );
            string formatted = string.Format( "{0}\r\n\r\n{1}:\r\n{2}", message, ex.GetType( ).Name, ex.Message );
            VisualStudioMessageBox.Show( formatted );
        }

        #endregion
    }
}