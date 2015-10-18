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
        private ITestExplorer _testExplorerMock;
        private ICoverageSettingsStore _coverageSettingsStoreMock;

        [SetUp]
        public void Setup()
        {
            _testExplorerMock = Substitute.For<ITestExplorer>();
            _coverageSettingsStoreMock = Substitute.For<ICoverageSettingsStore>();

            _sut = new CoverageOverviewViewModel(_testExplorerMock,_coverageSettingsStoreMock,null);
        }

        [Test]
        public async void Should_PopulateProjects_With_ProjectsFromTestExplorer()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");
            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            var testProject = new TestProject();
            testProject.Project = project;
            testProject.IsCoverageEnabled = true;
            _testExplorerMock.GetAllTestProjectsAsync().Returns(Task.FromResult(new[] { testProject }));

            // act
            await _sut.PopulateWithTestProjectsAsync();

            // assert
            Assert.That(_sut.TestProjects.Count, Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestProjectSettings.Name,Is.EqualTo("foo"));
            Assert.That(_sut.TestProjects[0].TestProjectSettings.IsCoverageEnabled, Is.EqualTo(testProject.IsCoverageEnabled));
        }

        [Test]
        public void Should_PopulateTestFixtures_With_FixturesFromTestExplorer()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");
            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            var testProject = new TestProject();
            testProject.Project = project;
            testProject.IsCoverageEnabled = true;
            testProject.TestFixtures = new[] { testClass.GetRoot().GetClassDeclarationSyntax() };
            _testExplorerMock.GetAllTestProjectsAsync().Returns(new[] { testProject });

            // act
            _sut.PopulateWithTestProjectsAsync();

            // assert
            Assert.That(_sut.TestProjects[0].TestFixtures.Length, Is.EqualTo(1));
            Assert.That(_sut.TestProjects[0].TestFixtures[0].Name, Is.EqualTo("MathHelperTests"));
        }

        [Test]
        public void Should_ClearData_When_RefreshCommandIsCalled_TwoTimesInRow()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");
            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            var testProject=new TestProject();
            testProject.Project = project;
            testProject.TestFixtures = new[] {testClass.GetRoot().GetClassDeclarationSyntax()};
            _testExplorerMock.GetAllTestProjectsAsync().Returns(new[] {testProject});

            // act
            _sut.RefreshCmd.Execute(null);
            _sut.RefreshCmd.Execute(null);

            // assert
            Assert.That(_sut.TestProjects.Count, Is.EqualTo(1));
        }
    }
}
