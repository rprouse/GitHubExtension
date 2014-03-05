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
using System.Windows;
using System.Windows.Input;
using Alteridem.GitHub.Annotations;
using Alteridem.GitHub.Model;

#endregion

namespace Alteridem.GitHub.Extension.View
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window, ILogonView
    {
        public LoginDialog()
        {
            InitializeComponent();
            LogonCommand = new RelayCommand( p => Logon(), p => CanLogon() );
            DataContext = this;
        }

        [NotNull]
        public string Username { get { return UserText.Text; } }

        [NotNull]
        public string Password { get { return PassText.Password; } }

        public ICommand LogonCommand { get; private set; }

        public void OnLoggingIn()
        {
            IsEnabled = false;
            Message.Text = "Logging in...";
            // TODO: Display progress
        }

        public void OnSuccess()
        {
            Close();
        }

        public void OnError(Exception ex)
        {
            IsEnabled = true;
            Message.Text = ex.Message;
        }

        private bool CanLogon()
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void Logon()
        {
            Message.Text = string.Empty;
            var api = Factory.Get<GitHubApi>( );
            api.Login( this );
        }
    }
}
