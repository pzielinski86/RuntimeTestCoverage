using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;
using TestCoverage.Storage;

namespace TestCoverage
{
    public class TestExplorer : ITestExplorer
    {
        private readonly ISolutionExplorer _solutionExplorer;
        private readonly ITestsExtractor _testsExtractor;
        private readonly ICoverageStore _coverageStore;
        private readonly ICoverageSettingsStore _coverageSettingsStore;

        public TestExplorer(ISolutionExplorer solutionExplorer, 
            ITestsExtractor testsExtractor, 
            ICoverageStore coverageStore,
            ICoverageSettingsStore coverageSettingsStore)
        {
            _solutionExplorer = solutionExplorer;
            _testsExtractor = testsExtractor;
            _coverageStore = coverageStore;
            _coverageSettingsStore = coverageSettingsStore;
        }

        public async Task<TestProject[]> GetAllTestProjectsAsync()
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

        public async Task<Project[]> GetAllTestProjectsWithCoveredProjectsAsync()
        {
            TestProject[] testProjects = await GetAllTestProjectsAsync();

            var allProjects = GetAllReferencedProjects(testProjects);

            return allProjects;
        }

        public async Task<Project[]> GetUnignoredTestProjectsWithCoveredProjectsAsync()
        {
            TestProject[] unignoredTestProjects = await GetUnignoredTestProjectsAsync();

            return GetAllReferencedProjects(unignoredTestProjects);
        }

        public RewrittenDocument[] GetReferencedTests(RewrittenDocument document, string projectName)
        {
            var methods = document.SyntaxTree.GetRoot().GetPublicMethods();
            string documentName = Path.GetFileNameWithoutExtension(document.DocumentPath);
            var currentCoverage = _coverageStore.ReadAll();
            var rewrittenDocuments = new List<RewrittenDocument>();

            foreach (var method in methods)
            {
                string path = NodePathBuilder.BuildPath(method, documentName, projectName);

                foreach (var docCoverage in currentCoverage.Where(x => x.Path == path))
                {
                    if (rewrittenDocuments.All(x => x.DocumentPath != docCoverage.TestDocumentPath))
                    {
                        SyntaxTree testRoot = _solutionExplorer.OpenFile(docCoverage.TestDocumentPath);

                        var rewrittenDocument = new RewrittenDocument(testRoot, docCoverage.TestDocumentPath);
                        rewrittenDocuments.Add(rewrittenDocument);
                    }
                }
            }

            return rewrittenDocuments.ToArray();
        }

        public ISolutionExplorer SolutionExplorer => _solutionExplorer;

        private Project[] GetAllReferencedProjects(TestProject[] testProjects)
        {
            var allProjects = new List<Project>();

            foreach (var testProject in testProjects)
            {
                allProjects.Add(testProject.Project);

                AddReferencedProjects(testProject.Project, allProjects);
            }
            return allProjects.Distinct().ToArray();
        }

        private async Task<TestProject[]> GetUnignoredTestProjectsAsync()
        {
            var allTestProjects = await GetAllTestProjectsAsync();

            return allTestProjects.Where(x => x.IsCoverageEnabled).ToArray();
        }

        private void AddReferencedProjects(Project project, List<Project> allProjects)
        {
            foreach (ProjectReference projectReference in project.ProjectReferences)
            {
                var foundReferencedProject =
                    _solutionExplorer.Solution.
                    Projects.
                    Single(p => p.Id == projectReference.ProjectId);

                allProjects.Add(foundReferencedProject);

                AddReferencedProjects(foundReferencedProject, allProjects);
            }
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