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
using NLog;
using Octokit;

#endregion

namespace Alteridem.GitHub.Model
{
    public static class Cache
    {
        public static CredentialCache Credentials
        {
            get
            {
                string cached = Properties.Settings.Default.Credentials;
                return CredentialCache.FromString(cached);
            }
            set { Properties.Settings.Default.Credentials = value != null ? value.ToString() : null; }
        }

        public static int Repository
        {
            get { return Properties.Settings.Default.Repository; }
            set { Properties.Settings.Default.Repository = value; }
        }

        public static void SaveCredentials(string logon, string password, string accessToken)
        {
            if(!string.IsNullOrEmpty(logon) && !string.IsNullOrEmpty(password))
                Credentials = new CredentialCache { Logon = logon, Password = password, AccessToken = string.Empty };
            else
                Credentials = new CredentialCache { Logon = string.Empty, Password = string.Empty, AccessToken = accessToken };
        }

        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
    }
}