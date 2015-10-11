using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NSubstitute;
using NUnit.Framework;
using TestCoverage;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Storage;
using TestCoverageVsPlugin.UI.ViewModels;

namespace TestCoverageVsPlugin.Tests.UI
{
    [TestFixture]
    public class CoverageOverviewViewModelTests
    {
        private CoverageOverviewViewModel _sut;
        private ISolutionExplorer _solutionExplorerMock;
        private ITestsExtractor _testExtractorMock;
        private ICoverageSettingsStore _settingsStoreMock;

        [SetUp]
        public void Setup()
        {
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();
            _testExtractorMock = Substitute.For<ITestsExtractor>();
            _settingsStoreMock = Substitute.For<ICoverageSettingsStore>();

            _settingsStoreMock.Read().Returns(new CoverageSettings());

            _sut =new CoverageOverviewViewModel(_solutionExplorerMock,_testExtractorMock,_settingsStoreMock);
        }

        [Test]
        public void Should_PopulateWithSolutionTestProjects_When_SolutionContainsTestProject_And_StoredSettingsAreUnavailable()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixture]class MathHelperTests{ [Test]void Test(){}}");
            
            workspace.AddDocument(project.Id,"MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax()});
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            _sut.PopulateWithTestProjects();

            // assert
            Assert.That(_sut.TestProjects.Count,Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestProjectSettings.Name, Is.EqualTo("foo"));
            Assert.IsFalse(_sut.TestProjects[0].TestProjectSettings.IsCoverageEnabled);
        }

        [Test]
        public void Should_PopulateTestProjectWithFixtures_When_SolutionContainsTestProject_And_StoredSettingsAreUnavailable()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixture]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            _sut.PopulateWithTestProjects();

            // assert
            Assert.That(_sut.TestProjects[0].TestFixtures.Length, Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestFixtures[0].Name, Is.EqualTo("MathHelperTests"));
        }

        [Test]
        public void Should_PopulateTestProjectWithFixtures_When_SolutionContainsTestProject_And_StoredSettingsAreAvailable()
        {
            // arrange
            var coverageSettings = new CoverageSettings();
            var testProjectSettings = new TestProjectSettings();
            coverageSettings.Projects.Add(testProjectSettings);
            _settingsStoreMock.Read().Returns(coverageSettings);

            testProjectSettings.IsCoverageEnabled = true;
            testProjectSettings.Name = "foo";

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixture]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            _sut.PopulateWithTestProjects();

            // assert
            Assert.That(_sut.TestProjects[0].TestFixtures.Length, Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestFixtures[0].Name, Is.EqualTo("MathHelperTests"));
        }

        [Test]
        public void Should_PopulateWithTestProjectsStoredInFile_When_StoredSettingsAreAvailable_And_SolutionContainsThatProject()
        {
            // arrange
            var coverageSettings = new CoverageSettings();
            var testProjectSettings = new TestProjectSettings();
            coverageSettings.Projects.Add(testProjectSettings);
            _settingsStoreMock.Read().Returns(coverageSettings);

            testProjectSettings.IsCoverageEnabled = true;
            testProjectSettings.Name = "foo";

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixture]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            _sut.PopulateWithTestProjects();

            // assert
            Assert.That(_sut.TestProjects.Count, Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestProjectSettings.Name, Is.EqualTo("foo"));
            Assert.IsTrue(_sut.TestProjects[0].TestProjectSettings.IsCoverageEnabled);
        }

        [Test]
        public void ShouldNot_PopulateWithTestProjectsStoredInFile_When_StoredSettingsAreAvailable_And_SolutionDoesContainsThatProject()
        {
            // arrange
            var coverageSettings = new CoverageSettings();
            var testProjectSettings = new TestProjectSettings();
            coverageSettings.Projects.Add(testProjectSettings);
            _settingsStoreMock.Read().Returns(coverageSettings);

            testProjectSettings.IsCoverageEnabled = true;
            testProjectSettings.Name = "foo";

            var workspace = new AdhocWorkspace();
            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new ClassDeclarationSyntax[0]);
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            _sut.PopulateWithTestProjects();

            // assert
            Assert.That(_sut.TestProjects.Count, Is.EqualTo(0));
        }
    }
}
