using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestCoverageVsPlugin
{
    #region TestCoverageVsPlugin Factory
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor
    /// to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(TestCoverageVsPlugin.MarginName)]
    [Order(After = PredefinedMarginNames.LeftSelection)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)] 
    [ContentType("text")] //Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        private readonly SolutionTestCoverage _solutionTestCoverage;
        private IVsStatusbar _statusBar;

        static MarginFactory()
        {

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;   
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("TestCoverage"))
            {
                string path = Assembly.GetExecutingAssembly().Location;
                path = System.IO.Path.GetDirectoryName(path);

                return Assembly.LoadFrom(System.IO.Path.Combine(path, SolutionTestCoverage.TestcoverageDll));
            }
            return null;
        }

        [ImportingConstructor]
        public MarginFactory([Import]SVsServiceProvider serviceProvider)
        {
            DTE dte = (DTE)serviceProvider.GetService(typeof(DTE));
            _statusBar = serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;

            string solutionPath = dte.Solution.FullName;
            _solutionTestCoverage = new SolutionTestCoverage(solutionPath, dte);
            _solutionTestCoverage.CalculateForAllDocuments();
        }                

        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new TestCoverageVsPlugin(_solutionTestCoverage, textViewHost.TextView,_statusBar);
        }
   
    }
    #endregion
}
