//------------------------------------------------------------------------------
// <copyright file="CoverageOverviewControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
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
        private SolutionEvents _solutionEvents;
        private DTE _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageOverviewControl"/> class.
        /// </summary>
        public CoverageOverviewControl()
        {
            _dte = (DTE)CoverageOverviewCommand.Instance.ServiceProvider.GetService(typeof(DTE));
            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionEventsOpened;

            this.InitializeComponent();
            this.Loaded += CoverageOverviewControl_Loaded;
        }

        private async void CoverageOverviewControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadDataContext();
        }

        private async Task ReloadDataContext()
        {
            if (!string.IsNullOrEmpty(_dte.Solution.FileName))
            {
                Config.SetSolution(_dte.Solution.FileName);

                ISolutionExplorer solutionExplorer = new SolutionExplorer(_dte.Solution.FileName);
                ICoverageSettingsStore settingsStore = new XmlCoverageSettingsStore();
                ICoverageStore coverageStore = new SqlCompactCoverageStore();

                ITestExplorer testExplorer = new TestExplorer(solutionExplorer,
                    new NUnitTestExtractor(), coverageStore, settingsStore);
                var xmlCoverageStore = new SqlCompactCoverageStore();

                var vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(_dte.Solution.FileName,
                    new SolutionCoverageEngine(),
                    xmlCoverageStore,
                    new Logger(CoverageOverviewCommand.Instance.ServiceProvider));

                var coverageOverviewViewModel = new CoverageOverviewViewModel(testExplorer, settingsStore,
                    vsSolutionTestCoverage);

                await coverageOverviewViewModel.PopulateWithTestProjectsAsync();

                DataContext = coverageOverviewViewModel;
            }
        }

        private async void SolutionEventsOpened()
        {
            await ReloadDataContext();
        }
    }
}