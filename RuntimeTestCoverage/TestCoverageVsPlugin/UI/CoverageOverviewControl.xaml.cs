//------------------------------------------------------------------------------
// <copyright file="CoverageOverviewControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;
using TestCoverageVsPlugin.UI.ViewModels;

namespace TestCoverageVsPlugin.UI
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CoverageOverviewControl.
    /// </summary>
    public partial class CoverageOverviewControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageOverviewControl"/> class.
        /// </summary>
        public CoverageOverviewControl()
        {
            InitDataContext();

            this.InitializeComponent();
            this.Loaded += CoverageOverviewControl_Loaded;

        }

        private void CoverageOverviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitDataContext();
        }

        private async void InitDataContext()
        {
            var dte = (DTE)CoverageOverviewCommand.Instance.ServiceProvider.GetService(typeof(DTE));

            if (!string.IsNullOrEmpty(dte.Solution.FileName))
            {
                ISolutionExplorer solutionExplorer = new SolutionExplorer(dte.Solution.FileName);
                ICoverageSettingsStore settingsStore = new XmlCoverageSettingsStore(dte.Solution.FileName);
                ICoverageStore coverageStore = new SqlCompactCoverageStore(dte.Solution.FileName);

                ITestExplorer testExplorer = new TestExplorer(solutionExplorer,
                    new NUnitTestExtractor(), coverageStore, settingsStore);
                var xmlCoverageStore = new SqlCompactCoverageStore(dte.Solution.FileName);

                var vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(dte.Solution.FileName,
                    () => new AppDomainSolutionCoverageEngine(), 
                    xmlCoverageStore,
                    new Logger(CoverageOverviewCommand.Instance.ServiceProvider));

                var coverageOverviewViewModel = new CoverageOverviewViewModel(testExplorer, settingsStore, vsSolutionTestCoverage);

                await coverageOverviewViewModel.PopulateWithTestProjectsAsync();

                DataContext = coverageOverviewViewModel;
            }
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "CoverageOverview");
        }
    }
}