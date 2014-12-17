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
using System.Windows.Controls;
using System.Windows.Input;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;

#endregion

namespace Alteridem.GitHub.Extension.ViewModel
{
    public class LoginViewModel : BaseGitHubViewModel
    {
        private ILoginView _view;
        private string _username;
        private string _accessToken;
        private string _message;
        private ICommand _logonCommand;

        public LoginViewModel(ILoginView view)
        {
            _view = view;
            LogonCommand = new RelayCommand(Logon, p => CanLogon());
        }

        public bool HasClientId
        {
            get
            {
                return GitHubApi.HasClientId;
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged();
            }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                if (value == _accessToken) return;
                _accessToken = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogonCommand
        {
            get { return _logonCommand; }
            private set
            {
                if (Equals(value, _logonCommand)) return;
                _logonCommand = value;
                OnPropertyChanged();
            }
        }

        public bool CanLogon()
        {
            return !string.IsNullOrWhiteSpace(Username)
                || !string.IsNullOrEmpty(AccessToken);
        }

        public async void Logon(object parameter)
        {
            Message = "Logging in...";
            // This breaks MVVM, but you can't bind to the password in a password
            // box for security reasons
            var pass = parameter as PasswordBox;
            if( pass == null)
                return;

            _view.IsEnabled = false;

            try
            {
                // This will throw if authentication fails
                await GitHubApi.Login(Username, pass.Password, AccessToken);
                _view.Close();
            }
            catch (Exception e)
            {
                _view.IsEnabled = true;
                Message = "Logon failed. " + e.Message;
            }
        }
    }
}