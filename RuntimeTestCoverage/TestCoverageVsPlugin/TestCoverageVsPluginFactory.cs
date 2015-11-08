using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
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
            _vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(solutionPath,
                () => new AppDomainSolutionCoverageEngine(), new SqlCompactCoverageStore(solutionPath));

            _vsSolutionTestCoverage.LoadCurrentCoverage();
        }

        private void ProjectItemAdded(ProjectItem projectItem)
        {
            var project = projectItem.ContainingProject;
            if (project.Saved == false)
            {
                project.Save();
            }

            _vsSolutionTestCoverage.InitAsync(true);
        }

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new TestDotsCoverageVsPlugin(_vsSolutionTestCoverage, textViewHost.TextView, _statusBar, _dte.Solution);
        }

    }
    #endregion
}
