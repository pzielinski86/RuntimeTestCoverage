using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TestCoverage;
using TestCoverage.Storage;
using TestCoverage.Tasks;

namespace LiveCoverageVsPlugin.UI.ViewModels
{
    public sealed class CoverageOverviewViewModel: INotifyPropertyChanged
    {
        private readonly ITestExplorer _testExplorer;
        private readonly ICoverageSettingsStore _settingsStore;
        private readonly IVsSolutionTestCoverage _vsSolutionTestCoverage;

        public CoverageOverviewViewModel(ITestExplorer testExplorer, ICoverageSettingsStore settingsStore, IVsSolutionTestCoverage vsSolutionTestCoverage)
        {
            _testExplorer = testExplorer;
            _settingsStore = settingsStore;
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
            TestProjects = new ObservableCollection<TestProjectViewModel>();
            RefreshCmd = new DelegateCommand(RefreshAsync);
            ResyncCmd=new DelegateCommand(Resync);
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }


        public ICommand RefreshCmd { get; }

        public ICommand ResyncCmd { get; }

        private async void Resync(object obj)
        {
            Title = "Processing...";
            await _vsSolutionTestCoverage.CalculateForAllDocumentsAsync();
            UpdateTitleWithResults();
        }

        private async void RefreshAsync(object obj)
        {            
            await PopulateWithTestProjectsAsync();
        }

        public async Task PopulateWithTestProjectsAsync()
        {
            TestProjects.Clear();

            var testProjects = await _testExplorer.GetAllTestProjectsAsync();

            foreach (var testProject in testProjects)
            {
                CreateTestProject(testProject);
            }

            UpdateTitleWithResults();
        }

        private void CreateTestProject(TestProject testProject)
        {
            var testFixturesInDocument = testProject.
               TestFixtures.Select(x => new TestFixtureViewModel(x.Identifier.ValueText)).
               ToArray();

            var testProjectViewModel = new TestProjectViewModel(_settingsStore)
            {
                TestProjectSettings = new TestProjectSettings
                {
                    Name = testProject.Project.Name,
                    IsCoverageEnabled = testProject.IsCoverageEnabled
                }
            };

            testProjectViewModel.TestFixtures = testFixturesInDocument;

            TestProjects.Add(testProjectViewModel);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateTitleWithResults()
        {
            if (_vsSolutionTestCoverage == null)
                return;

            int documentsCount = _vsSolutionTestCoverage.SolutionCoverageByDocument.Count;
            int coverage = _vsSolutionTestCoverage.SolutionCoverageByDocument.Sum(x => x.Value.Count);
            int successes =
                _vsSolutionTestCoverage.SolutionCoverageByDocument.Sum(x => x.Value.Count(y => y.IsSuccess));

            int failures =
                _vsSolutionTestCoverage.SolutionCoverageByDocument.Sum(x => x.Value.Count(y => !y.IsSuccess));

            Title = $"Documents: {documentsCount}, Coverage: {coverage}, Success: {successes}, Failures: {failures}";
        }
        public ObservableCollection<TestProjectViewModel> TestProjects { get; }
    }
}