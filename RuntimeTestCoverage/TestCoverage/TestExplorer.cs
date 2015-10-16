﻿using System.Collections.Generic;
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
            _solutionExplorer.Open();
            _testsExtractor = testsExtractor;
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

        private Project[] GetAllReferencedProjects(TestProject[] testProjects)
        {
            var allProjects = new List<Project>();

            foreach (var testProject in testProjects)
            {
                allProjects.Add(testProject.Project);

                AddReferencedProjects(testProject.Project, allProjects);
            }
            return allProjects.ToArray();
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