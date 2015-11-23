//------------------------------------------------------------------------------
// <copyright file="CoverageOverviewControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;
using TestCoverageVsPlugin.UI.ViewModels;

namespace TestCoverageVsPlugin.UI
{
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
                ICoverageSettingsStore settingsStore = new XmlCoverageSettingsStore();
                ICoverageStore coverageStore = new SqlCompactCoverageStore();

                ITestExplorer testExplorer = new TestExplorer(solutionExplorer,
                    new NUnitTestExtractor(), coverageStore, settingsStore);
                var xmlCoverageStore = new SqlCompactCoverageStore();

                var vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(dte.Solution.FileName,
                    new SolutionCoverageEngine(), 
                    xmlCoverageStore,
                    new Logger(CoverageOverviewCommand.Instance.ServiceProvider));

                var coverageOverviewViewModel = new CoverageOverviewViewModel(testExplorer, settingsStore, vsSolutionTestCoverage);

                await coverageOverviewViewModel.PopulateWithTestProjectsAsync();

                DataContext = coverageOverviewViewModel;
            }
        }
    }
}