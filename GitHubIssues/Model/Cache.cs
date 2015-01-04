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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

#endregion

namespace Alteridem.GitHub.Model
{
    [Export]
    public class Cache : ICache
    {
        private readonly SVsServiceProvider _serviceProvider;

        [ImportingConstructor]
        private Cache([Import] SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private WritableSettingsStore WritableSettingsStore
        {
            get
            {
                ShellSettingsManager shellSettingsManager = new ShellSettingsManager(_serviceProvider);
                return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            }
        }

        public CredentialCache Credentials
        {
            get
            {
                string cached = WritableSettingsStore.GetString("Alteridem.GitHubExtension", "Credentials", "");
                return CredentialCache.FromString(cached);
            }

            set
            {
                string credentials = value != null ? value.ToString() : null;
                WritableSettingsStore.CreateCollection("Alteridem.GitHubExtension");
                WritableSettingsStore.SetString("Alteridem.GitHubExtension", "Credentials", credentials ?? string.Empty);
            }
        }

        public int Repository
        {
            get
            {
                return WritableSettingsStore.GetInt32("Alteridem.GitHubExtension", "Repository", 0);
            }

            set
            {
                WritableSettingsStore.CreateCollection("Alteridem.GitHubExtension");
                WritableSettingsStore.SetInt32("Alteridem.GitHubExtension", "Repository", value);
            }
        }

        public void SaveCredentials(string logon, string password)
        {
            Credentials = new CredentialCache { Logon = logon, Password = password, AccessToken = string.Empty };
        }

        public void SaveToken( string accessToken )
        {
            Credentials = new CredentialCache { Logon = string.Empty, Password = string.Empty, AccessToken = accessToken };
        }
    }
}