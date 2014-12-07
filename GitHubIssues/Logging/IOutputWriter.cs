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

namespace Alteridem.GitHub.Logging
{
    /// <summary>
    /// Interface for logging and showing messages to the user
    /// </summary>
    public interface IOutputWriter
    {
        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        void Write(LogLevel level, string message);

        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Arguments to the message</param>
        void Write(LogLevel level, string message, params object[] args);

        /// <summary>
        /// Shows the specified message to the user along with the exception information.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        void Write(LogLevel level, string message, Exception ex);
    }
}