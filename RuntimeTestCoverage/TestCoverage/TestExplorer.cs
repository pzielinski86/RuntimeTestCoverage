using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;

namespace TestCoverage
{
    public class TestExplorer : ITestExplorer
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ITestsExtractor _testsExtractor;
        private readonly ICoverageSettingsStore _coverageSettingsStore;

        public TestExplorer(ISolutionExplorer solutionExplorer, ITestsExtractor testsExtractor, ICoverageSettingsStore coverageSettingsStore)
        {
            _solutionExplorer = solutionExplorer;
            _testsExtractor = testsExtractor;
            _coverageSettingsStore = coverageSettingsStore;
        }

        public async Task<TestProject[]> GetTestProjectsAsync()
        {
            var testProjects = new List<TestProject>();
            var settings = _coverageSettingsStore.Read();

            foreach (var project in _solutionExplorer.Solution.Projects)
            {
                var fixtures = await GetProjectTestFixtures(project);

                if (fixtures.Length > 0)
                {
                    var testProject = new TestProject();
                    testProject.Project = project;
                    testProject.TestFixtures = fixtures;

                    var storedProject = settings.Projects.SingleOrDefault(x => x.Name == project.Name);

                    if (storedProject != null)
                        testProject.IsCoverageEnabled = storedProject.IsCoverageEnabled;

                    testProjects.Add(testProject);
                }
            }

            return testProjects.ToArray();
        }

        private async Task<ClassDeclarationSyntax[]> GetProjectTestFixtures(Project project)
        {
            var testFixturesInProject = new List<ClassDeclarationSyntax>();

            foreach (var document in project.Documents)
            {
                SyntaxNode root = await document.GetSyntaxRootAsync();
                ClassDeclarationSyntax[] testClasses = _testsExtractor.GetTestClasses(root);

                testFixturesInProject.AddRange(testClasses);
            }

            return testFixturesInProject.ToArray();
        }
    }
}