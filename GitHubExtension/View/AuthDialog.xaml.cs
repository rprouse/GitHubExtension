// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2015 Rob Prouse
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

using Alteridem.GitHub.Extension.Interfaces;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Navigation;
using System;
using Microsoft.VisualStudio.PlatformUI;
using Alteridem.GitHub.Model;
using System.Diagnostics;
using Alteridem.GitHub.Extension.ViewModel;

namespace Alteridem.GitHub.Extension.View
{
    /// <summary>
    /// Interaction logic for AuthDialog.xaml
    /// </summary>
    public partial class AuthDialog : DialogWindow, ILoginView
    {
        public AuthViewModel _viewModel { get; set; }

        public AuthDialog()
        {
            InitializeComponent();
            _viewModel = new AuthViewModel(this);
            DataContext = _viewModel;
            browser.Navigate(_viewModel.AuthorizeUrl);
        }

        async void OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            await _viewModel.Logon(e.Uri);
        }
    }
}
