using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.PlatformUI;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin.UI.ViewModels
{
    public sealed class CoverageOverviewViewModel
    {
        private readonly ITestExplorer _testExplorer;
        private readonly ICoverageSettingsStore _settingsStore;

        public CoverageOverviewViewModel(ITestExplorer testExplorer, ICoverageSettingsStore settingsStore)
        {
            _testExplorer = testExplorer;
            _settingsStore = settingsStore;
            TestProjects = new ObservableCollection<TestProjectViewModel>();
            RefreshCmd = new DelegateCommand(RefreshAsync);
        }

        public ICommand RefreshCmd { get; }

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

        public ObservableCollection<TestProjectViewModel> TestProjects { get; }
    }
}