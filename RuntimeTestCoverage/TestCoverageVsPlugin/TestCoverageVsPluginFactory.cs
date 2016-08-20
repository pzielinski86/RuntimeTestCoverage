using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using TestCoverage;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin
{
    #region TestDotsCoverageVsPlugin Factory
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor
    /// to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(TestDotsCoverageVsPlugin.MarginName)]
    [Order(After = PredefinedMarginNames.LeftSelection)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")] //Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        private readonly SVsServiceProvider _serviceProvider;
        private VsSolutionTestCoverage _vsSolutionTestCoverage;
        private IVsStatusbar _statusBar;
        private DTE _dte;
        private readonly ProjectItemsEvents _projectItemsEvents;
        private Logger _logger;
        private Workspace _myWorkspace;
        private readonly SolutionEvents _solutionEvents;


        [ImportingConstructor]
        public MarginFactory([Import]SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            _dte = (DTE)serviceProvider.GetService(typeof(DTE));
          

            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionEvents_Opened;
            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _projectItemsEvents = ((Events2)_dte.Events).ProjectItemsEvents;
            _projectItemsEvents.ItemAdded += ProjectItemAdded;
            _projectItemsEvents.ItemRemoved += ProjectItemRemoved;
            _statusBar = serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            _logger = new Logger(serviceProvider);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void ProjectItemRemoved(ProjectItem projectitem)
        {
            for(short i=0;i<projectitem.FileCount;i++)
                _vsSolutionTestCoverage.RemoveByPath(projectitem.FileNames[i]);
        }

        private void InitMyWorkspace(SVsServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel) serviceProvider.GetService(typeof (SComponentModel));
            _myWorkspace = componentModel.GetService<VisualStudioWorkspace>();
        }

        private void SolutionEvents_AfterClosing()
        {
            _vsSolutionTestCoverage.Dispose();
            _vsSolutionTestCoverage = null;
        }

        private void SolutionEvents_Opened()
        {      
            InitSolutionCoverageEngine();
        }

        private void InitSolutionCoverageEngine()
        {
            InitMyWorkspace(_serviceProvider);
            string solutionPath = _dte.Solution.FullName;

            if (_vsSolutionTestCoverage != null && _vsSolutionTestCoverage.SolutionPath == solutionPath)
                return;

            Config.SetSolution(solutionPath);

            _vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(_myWorkspace,
               new SolutionCoverageEngine(),
                new SqlCompactCoverageStore(),
                _logger);

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject.ToString());
        }

        private void ProjectItemAdded(ProjectItem projectItem)
        {
            var project = projectItem.ContainingProject;
            if (project.Saved == false)
            {
                project.Save();
            }

            InitSolutionCoverageEngine();
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            InitSolutionCoverageEngine();

            return new TestDotsCoverageVsPlugin(_vsSolutionTestCoverage, textViewHost.TextView, _statusBar, _dte.Solution);
        }
    }
    #endregion
}
