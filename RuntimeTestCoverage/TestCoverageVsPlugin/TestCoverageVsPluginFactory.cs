using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
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
        private readonly VsSolutionTestCoverage _vsSolutionTestCoverage;
        private IVsStatusbar _statusBar;
        private DTE _dte;
        private readonly ProjectItemsEvents _projectItemsEvents;
        private Logger _logger;

        static MarginFactory()
        {

        }

        [ImportingConstructor]
        public MarginFactory([Import]SVsServiceProvider serviceProvider)
        {
            _dte = (DTE)serviceProvider.GetService(typeof(DTE));

            _projectItemsEvents = ((Events2)_dte.Events).ProjectItemsEvents;
            _projectItemsEvents.ItemAdded += ProjectItemAdded;
            _statusBar = serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            
            string solutionPath = _dte.Solution.FullName;

            Config.SetSolution(solutionPath);

            _logger = new Logger(serviceProvider);
            _vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(solutionPath,
               new SolutionCoverageEngine(),
                new SqlCompactCoverageStore(),
                _logger);

            _vsSolutionTestCoverage.Reinit();
            _vsSolutionTestCoverage.LoadCurrentCoverage();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Write(e.ExceptionObject.ToString());
        }

        private void ProjectItemAdded(ProjectItem projectItem)
        {
            var project = projectItem.ContainingProject;
            if (project.Saved == false)
            {
                project.Save();
            }

            _vsSolutionTestCoverage.Reinit();
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new TestDotsCoverageVsPlugin(_vsSolutionTestCoverage, textViewHost.TextView, _statusBar, _dte.Solution);
        }

    }
    #endregion
}
