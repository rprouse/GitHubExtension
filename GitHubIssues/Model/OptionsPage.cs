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

#region Using Directives

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

using Alteridem.GitHub.Styles;

#endregion

namespace Alteridem.GitHub.Model
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class OptionsPage : DialogPage
    {
        [Category("Interface")]
        [DisplayName("Issue Theme")]
        [Description("A light or dark background for the Issue window")]
        public IssueTheme IssueTheme { get; set; }

        [Category("Interface")]
        [DisplayName("Hide Repositories Without Issues")]
        [Description("Hides all repositories from the selection list that do not have open or closed issues")]
        public bool HideRepositoriesWithNoIssues { get; set; }

        [Category("Repositories")]
        [DisplayName("Disable Auto-Select Repository")]
        [Description("Disables automatically selecting the repository based on the code you are working on")]
        public bool DisableAutoSelectRepository { get; set; }
    }
}