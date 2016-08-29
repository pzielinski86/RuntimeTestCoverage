using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using TestCoverage;
using TestCoverage.Monitors;
using TestCoverage.Storage;
using TestCoverage.Tasks;
using TestCoverageVsPlugin.Logging;
using TestCoverageVsPlugin.Tasks;

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
        private ITaskCoverageManager _taskCoverageManager;
        private IVsStatusbar _statusBar;
        private DTE _dte;
        private Workspace _myWorkspace;
        private readonly SolutionEvents _solutionEvents;
        private RoslynSolutionWatcher _roslynSolutionWatcher;


        [ImportingConstructor]
        public MarginFactory([Import]SVsServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            _dte = (DTE)serviceProvider.GetService(typeof(DTE));


            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionEvents_Opened;
            _solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            _statusBar = serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            LogFactory.CurrentLogger = new VisualStudioLogger(serviceProvider);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void InitMyWorkspace(SVsServiceProvider serviceProvider)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _myWorkspace = componentModel.GetService<VisualStudioWorkspace>();
        }

        private void SolutionEvents_AfterClosing()
        {
            _vsSolutionTestCoverage.Dispose();
            _vsSolutionTestCoverage = null;
        }

        private void InitSolutionCoverageEngine()
        {
            InitMyWorkspace(_serviceProvider);
            string solutionPath = _dte.Solution.FullName;

            if (_vsSolutionTestCoverage != null && _vsSolutionTestCoverage.MyWorkspace == _myWorkspace)
                return;

            Config.SetSolution(solutionPath);

            var sqlCompactCoverageStore = new SqlCompactCoverageStore();
            var rewrittenDocumentsStorage = new RewrittenDocumentsStorage();

            _vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(_myWorkspace,
                new SolutionCoverageEngine(),
                sqlCompactCoverageStore);

            _taskCoverageManager = new TaskCoverageManager(new VsDispatchTimer(), new RoslynDocumentProvider(), _vsSolutionTestCoverage);
            _roslynSolutionWatcher = new RoslynSolutionWatcher(_dte, _myWorkspace,
                sqlCompactCoverageStore, rewrittenDocumentsStorage, _taskCoverageManager);
            _roslynSolutionWatcher.DocumentRemoved += _roslynSolutionWatcher_DocumentRemoved;
            _roslynSolutionWatcher.Start();
        }

        private void _roslynSolutionWatcher_DocumentRemoved(object sender, DocumentRemovedEventArgs e)
        {
            _vsSolutionTestCoverage.RemoveByPath(e.DocumentPath);
        }

        private void SolutionEvents_Opened()
        {
            InitSolutionCoverageEngine();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogFactory.CurrentLogger.Error(e.ExceptionObject.ToString());
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            InitSolutionCoverageEngine();

            return new TestDotsCoverageVsPlugin(_vsSolutionTestCoverage,
                _taskCoverageManager,
                textViewHost.TextView,
                _statusBar,
                _dte.Solution);
        }
    }
    #endregion
}
