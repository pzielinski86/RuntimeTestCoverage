using System.Threading.Tasks;
using EnvDTE;
using LiveCoverageVsPlugin.Logging;
using LiveCoverageVsPlugin.UI.ViewModels;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;

namespace LiveCoverageVsPlugin.UI
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for CoverageOverviewToolControl.
    /// </summary>
    public partial class CoverageOverviewToolControl : UserControl
    {
        private SolutionEvents _solutionEvents;
        private DTE _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageOverviewControl"/> class.
        /// </summary>
        public CoverageOverviewToolControl()
        {
            _dte = (DTE)CoverageOverviewToolCommand.Instance.ServiceProvider.GetService(typeof(DTE));
            _solutionEvents = _dte.Events.SolutionEvents;
            _solutionEvents.Opened += SolutionEventsOpened;
            LogFactory.CurrentLogger = new VisualStudioLogger(CoverageOverviewToolCommand.Instance.ServiceProvider);

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

                var myWorkspace = CoverageOverviewToolCommand.Instance.MyWorkspace;
                var rewrittenDocumentsStorage = new RewrittenDocumentsStorage();

                ISolutionExplorer solutionExplorer = new SolutionExplorer(rewrittenDocumentsStorage, myWorkspace);
                ICoverageSettingsStore settingsStore = new XmlCoverageSettingsStore();
                ICoverageStore coverageStore = new SqlCompactCoverageStore();

                ITestExplorer testExplorer = new TestExplorer(solutionExplorer,
                    new NUnitTestExtractor(), coverageStore, settingsStore);
                var xmlCoverageStore = new SqlCompactCoverageStore();

                var vsSolutionTestCoverage = VsSolutionTestCoverage.CreateInstanceIfDoesNotExist(myWorkspace,
                    new SolutionCoverageEngine(),
                    xmlCoverageStore);

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