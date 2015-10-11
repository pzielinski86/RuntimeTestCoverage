using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin.UI.ViewModels
{
    public class CoverageOverviewViewModel
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ITestsExtractor _testsExtractor;
        private readonly ICoverageSettingsStore _settingsStore;

        public CoverageOverviewViewModel(ISolutionExplorer solutionExplorer, ITestsExtractor testsExtractor, ICoverageSettingsStore settingsStore)
        {
            _solutionExplorer = solutionExplorer;
            _testsExtractor = testsExtractor;
            _settingsStore = settingsStore;
            TestProjects = new ObservableCollection<TestProject>();
        }

        public async void PopulateWithTestProjects()
        {
            _solutionExplorer.Open();

            CoverageSettings storedSettings = _settingsStore.Read();

            foreach (var project in _solutionExplorer.Solution.Projects)
            {
                var fixtures = await GetProjectTestFixtures(project);

                if (fixtures.Count > 0)
                    CreateTestProject(project, fixtures, storedSettings);
            }
        }

        private async Task<List<TestFixture>> GetProjectTestFixtures(Project project)
        {
            var testFixturesInProject = new List<TestFixture>();

            foreach (var document in project.Documents)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync();
                ClassDeclarationSyntax[] testClasses = _testsExtractor.GetTestClasses(root);
                var testFixturesInDocument = testClasses.Select(x => new TestFixture(x.Identifier.ValueText)).ToArray();

                testFixturesInProject.AddRange(testFixturesInDocument);
            }

            return testFixturesInProject;
        }

        private void CreateTestProject(Project project, List<TestFixture> testFixtures, CoverageSettings storedSettings)
        {
            var testProject = new TestProject(_settingsStore);

            var storedTestProjectSettings = storedSettings.Projects.FirstOrDefault(x => x.Name == project.Name);
            if (storedTestProjectSettings == null)
                storedTestProjectSettings = new TestProjectSettings() {Name = project.Name};

            testProject.TestProjectSettings = storedTestProjectSettings;
            testProject.TestFixtures = testFixtures.ToArray();

            TestProjects.Add(testProject);
        }

        public ObservableCollection<TestProject> TestProjects { get; }
    }
}