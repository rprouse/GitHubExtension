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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#endregion

namespace Alteridem.GitHub.Extension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "0.1", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(IssueListToolWindow))]
    [ProvideToolWindow(typeof(IssueToolWindow))]
    [ProvideService(typeof(IIssueViewer))]
    [Guid(GuidList.guidGitHubExtensionPkgString)]
    public sealed class GitHubExtensionPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public GitHubExtensionPackage()
        {
            Factory.AddAssembly(Assembly.GetExecutingAssembly());
            Cache.ApplicationName = "GitHubExtension";
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this));
            
            var container = this as IServiceContainer;
            var callback = new ServiceCreatorCallback(CreateIssueViewer);
            container.AddService(typeof(IIssueViewer), callback, true);
        }

        private object CreateIssueViewer(IServiceContainer container, Type servicetype)
        {
            return ShowIssueToolWindow();
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowIssueListToolWindow(object sender, EventArgs e)
        {
            ShowIssueListToolWindow( );
        }

        private IssueListToolWindow ShowIssueListToolWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = FindToolWindow(typeof(IssueListToolWindow), 0, true) as IssueListToolWindow;
            if ( ( null == window ) || ( null == window.Frame ) )
            {
                throw new NotSupportedException( Resources.CanNotCreateWindow );
            }
            var windowFrame = ( IVsWindowFrame )window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( windowFrame.Show( ) );
            return window;
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowIssueToolWindow(object sender, EventArgs e)
        {
            ShowIssueToolWindow( );
        }

        private IssueToolWindow ShowIssueToolWindow()
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = FindToolWindow(typeof(IssueToolWindow), 0, true) as IssueToolWindow;
            if ( ( null == window ) || ( null == window.Frame ) )
            {
                throw new NotSupportedException( Resources.CanNotCreateWindow );
            }
            var windowFrame = ( IVsWindowFrame )window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( windowFrame.Show( ) );
            return window;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidNewIssue);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
                // Create the commands for the tool windows
                var issueListWndCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidIssues);
                var menuIssueListWin = new MenuCommand(ShowIssueListToolWindow, issueListWndCommandID);
                mcs.AddCommand(menuIssueListWin);

                var issueWndCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidIssueWindow);
                var menuIssueWin = new MenuCommand(ShowIssueToolWindow, issueWndCommandID);
                mcs.AddCommand(menuIssueWin);
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "GitHub Extension",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }

    }
}
