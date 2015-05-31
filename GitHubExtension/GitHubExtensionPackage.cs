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
using System.Reflection;
using System.Runtime.InteropServices;
using Alteridem.GitHub.Extension.Interfaces;
using Alteridem.GitHub.Extension.View;
using Alteridem.GitHub.Interfaces;
using Alteridem.GitHub.Model;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Tvl.VisualStudio.OutputWindow.Interfaces;

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
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "0.5", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(IssueListToolWindow))]
    [ProvideToolWindow(typeof(IssueToolWindow))]
    [ProvideOptionPage(typeof(OptionsPage), "GitHub", "General", 0, 0, true)]
    [ProvideService(typeof(IIssueToolWindow))]
    [Guid(GuidList.guidGitHubExtensionPkgString)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), PackageRegistration(UseManagedResourcesOnly = true)]
    public sealed class GitHubExtensionPackage : Package, IOptionsProvider
    {
        /// <summary>
        /// This error list provider is used to warn developers if the VSBase Services Debugging Support extension is
        /// not installed.
        /// </summary>
        private ErrorListProvider vsbaseWarningProvider;

        /// <summary>
        /// The solution events. We must keep a reference to this, or the events will not fire
        /// </summary>
        private SolutionEvents _solutionEvents;

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
            Debug.WriteLine(ToString(), "Entering constructor for");
            
            var container = this as IServiceContainer;
            var callback = new ServiceCreatorCallback(CreateIssueViewer);
            container.AddService(typeof(IIssueToolWindow), callback, true);
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

        public OptionsPage Options
        {
            get { return GetDialogPage(typeof (OptionsPage)) as OptionsPage; }
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
            Debug.WriteLine (ToString(), "Entering Initialize() of");
            base.Initialize();

            CreateMenuHandlers();
            HookupEvents();
            SetupServices();
        }

        /// <summary>
        /// Releases the resources used by the <see cref="T:Microsoft.VisualStudio.Shell.Package"/> object.
        /// </summary>
        /// <param name="disposing">true if the object is being disposed, false if it is being finalized.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vsbaseWarningProvider != null)
                    vsbaseWarningProvider.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var add = Factory.Get<IIssueEditor>();
            add.SetIssue(null);
            add.ShowModal();
        }

        private void HandleNavigateToVsBaseServicesExtension(object sender, EventArgs e)
        {
            const string vsbaseDebugExtensionLocation = "https://visualstudiogallery.msdn.microsoft.com/fca95a59-3fc6-444e-b20c-cc67828774cd";
            IVsWebBrowsingService webBrowsingService = GetService<SVsWebBrowsingService, IVsWebBrowsingService>();
            if (webBrowsingService != null)
            {
                IVsWindowFrame windowFrame;
                webBrowsingService.Navigate(vsbaseDebugExtensionLocation, 0, out windowFrame);
                return;
            }

            IVsUIShellOpenDocument openDocument = GetService<SVsUIShellOpenDocument, IVsUIShellOpenDocument>();
            if (openDocument != null)
            {
                openDocument.OpenStandardPreviewer(0, vsbaseDebugExtensionLocation, VSPREVIEWRESOLUTION.PR_Default, 0);
            }
        }

        private void CreateMenuHandlers()
        {
            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService<IMenuCommandService, OleMenuCommandService>();
            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidNewIssue);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
                // Create the commands for the tool windows
                var issueListWndCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidIssues);
                var menuIssueListWin = new MenuCommand(ShowIssueListToolWindow, issueListWndCommandID);
                mcs.AddCommand(menuIssueListWin);

                var issueWndCommandID = new CommandID(GuidList.guidGitHubExtensionCmdSet, (int)PkgCmdIDList.cmdidIssueWindow);
                var menuIssueWin = new MenuCommand(ShowIssueToolWindow, issueWndCommandID);
                mcs.AddCommand(menuIssueWin);
            }
        }

        private void HookupEvents()
        {
            var dte = GetService<DTE>();
            if (dte != null)
            {
                _solutionEvents = dte.Events.SolutionEvents;
                _solutionEvents.Opened += OnSolutionOpened;
            }
        }

        void OnSolutionOpened()
        {
            var dte = GetService<DTE>();
            var gitHubApi = Factory.Get<GitHubApiBase>();
            if (dte != null && gitHubApi != null)
            {
                // Switch the repository to that of the solution
                Debug.WriteLine(dte.Solution.FullName, "Opened solution");
                gitHubApi.SetRepositoryForSolution(dte.Solution.FullName);
            }
        }

        private void SetupServices()
        {
            var componentModel = GetService<SComponentModel, IComponentModel>();
            var outputWindowService = componentModel.DefaultExportProvider.GetExportedValueOrDefault<IOutputWindowService>();
            IOutputWindowPane gitHubPane = null;

            // Warn users if dependencies aren't installed.
            vsbaseWarningProvider = new ErrorListProvider(this);
            if (outputWindowService != null)
            {
                gitHubPane = outputWindowService.TryGetPane(OutputWriter.GitHubOutputWindowPaneName);
            }
            else
            {
                var task = new ErrorTask
                {
                    Category = TaskCategory.Misc,
                    ErrorCategory = TaskErrorCategory.Error,
                    Text =
                        "The required VSBase Services debugging support extension is not installed; output window messages will not be shown. Click here for more information."
                };
                task.Navigate += HandleNavigateToVsBaseServicesExtension;
                vsbaseWarningProvider.Tasks.Add(task);
                vsbaseWarningProvider.Show();
            }

            // This code is a bit of a hack to bridge MEF created components and Ninject managed components
            Factory.Rebind<IOutputWindowPane>().ToConstant(gitHubPane);
            Factory.Rebind<ICache>().ToConstant(componentModel.DefaultExportProvider.GetExportedValue<Cache>());
            Factory.Rebind<IOptionsProvider>().ToConstant(this);
        }

        /// <summary>
        /// Simplify getting services
        /// </summary>
        private T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Simplify getting services
        /// </summary>
        private TReturn GetService<TGet, TReturn>()
            where TGet : class
            where TReturn : class
        {
            return GetService(typeof(TGet)) as TReturn;
        }
    }
}
