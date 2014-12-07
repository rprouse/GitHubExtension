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
using Octokit;

namespace Alteridem.GitHub.Logging
{
    /// <summary>
    /// Abstract base class so the implementers only need to write the output methods
    /// </summary>
    public abstract class BaseOutputWriter : IOutputWriter
    {
        // TODO: These minimum levels should be settings
        private const LogLevel MIN_LEVEL_SHOWN = LogLevel.Error;
        private const LogLevel MIN_LEVEL_LOGGED = LogLevel.Info;

        #region Abstract Methods

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected abstract void WriteMessage(string message);

        /// <summary>
        /// Shows the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected abstract void ShowMessage(string message);

        #endregion

        #region Helper Methods

        private void WriteMessage(LogLevel level, string message)
        {
            WriteMessage(FormatLogMessage(level, message));
        }

        private void WriteMessage(LogLevel level, string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            WriteMessage(level, formatted);
        }

        private void WriteMessage(LogLevel level, string message, Exception ex)
        {
            string formatted;
            var apiEx = ex as ApiException;
            if (apiEx != null)
                formatted = FormatApiExceptionForWriting(message, apiEx);
            else
                formatted = FormatExceptionForWriting(message, ex);
            WriteMessage(level, formatted);
        }

        private void ShowMessage(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            ShowMessage(formatted);
        }

        private void ShowMessage(string message, Exception ex)
        {
            var formatted = FormatExceptionForDisplay(message, ex);
            ShowMessage(formatted);
        }

        private string FormatLogMessage(LogLevel level, string message)
        {
            return string.Format("{0:s} [{1}] {2}", DateTime.Now, level, message);
        }

        private string FormatExceptionForWriting(string message, Exception ex)
        {
            return string.Format("{0} {1}: {2}", message, ex.GetType().Name, ex.Message);
        }

        private string FormatApiExceptionForWriting(string message, ApiException ex)
        {
            string info = string.Format("{0}, Status: {1}, API Error Msg: {2}", ex.Message, ex.StatusCode,
                ex.ApiError.Message);
            return string.Format("{0} {1}: {2}", message, ex.GetType().Name, info);
        }

        private string FormatExceptionForDisplay(string message, Exception ex)
        {
            return string.Format("{0}\r\n\r\n{1}: {2}", message, ex.GetType().Name, ex.Message);
        }

        #endregion

        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        public void Write(LogLevel level, string message)
        {
            if (level >= MIN_LEVEL_SHOWN)
                ShowMessage(message);
            if (level >= MIN_LEVEL_LOGGED)
                WriteMessage(level, message);
        }

        /// <summary>
        /// Shows the specified message to the user.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        /// <param name="args">Arguments to the message</param>
        public void Write(LogLevel level, string message, params object[] args)
        {
            if (level >= MIN_LEVEL_SHOWN)
                ShowMessage(message, args);
            if (level >= MIN_LEVEL_LOGGED)
                WriteMessage(level, message, args);
        }

        /// <summary>
        /// Shows the specified message to the user along with the exception information.
        /// </summary>
        /// <param name="level">The level at which to log the message</param>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Write(LogLevel level, string message, Exception ex)
        {
            if (level >= MIN_LEVEL_SHOWN)
                ShowMessage(message, ex);
            if (level >= MIN_LEVEL_LOGGED)
                WriteMessage(level, message, ex);
        }
    }
}