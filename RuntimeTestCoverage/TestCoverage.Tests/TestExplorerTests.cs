using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NSubstitute;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Storage;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class TestExplorerTests
    {
        private ISolutionExplorer _solutionExplorerMock;
        private ITestsExtractor _testExtractorMock;
        private ICoverageSettingsStore _settingsStoreMock;
        private TestExplorer _sut;

        [SetUp]
        public void Setup()
        {
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();
            _testExtractorMock = Substitute.For<ITestsExtractor>();
            _settingsStoreMock = Substitute.For<ICoverageSettingsStore>();

            _settingsStoreMock.Read().Returns(new CoverageSettings());

            _sut = new TestExplorer(_solutionExplorerMock, _testExtractorMock, _settingsStoreMock);
        }


        [Test]
        public async void Should_ReturnIgnoredSolutionTestProject_When_SolutionContainsTestProject_And_StoredSettingsAreUnavailable()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            TestProject[] projects = await _sut.GetAllTestProjectsAsync();

            // assert
            Assert.That(projects.Length, Is.EqualTo(1));
            Assert.That(projects[0].Project.Name, Is.EqualTo("foo"));
            Assert.IsFalse(projects[0].IsCoverageEnabled);
        }

        [Test]
        public async void Should_ReturnTestProjectWithFixtures_When_SolutionContainsTestProject_And_StoredSettingsAreUnavailable()
        {
            // arrange
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo", LanguageNames.CSharp);
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            TestProject[] projects = await _sut.GetAllTestProjectsAsync();

            // assert
            Assert.That(projects[0].TestFixtures.Length, Is.EqualTo(1));
            Assert.That(projects[0].TestFixtures[0].Identifier.ValueText, Is.EqualTo("MathHelperTests"));
        }

        [Test]
        public async void Should_ReturnTestProjectWithFixtures_When_SolutionContainsTestProject_And_StoredSettingsAreAvailable()
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
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            TestProject[] projects = await _sut.GetAllTestProjectsAsync();

            // assert
            Assert.That(projects[0].TestFixtures.Length, Is.EqualTo(1));
            Assert.That(projects[0].TestFixtures[0].Identifier.ValueText, Is.EqualTo("MathHelperTests"));
        }

        [Test]
        public async void Should_ReturnUnignoredTestProject_When_StoredSettingsAreAvailable_And_SolutionContainsThatProject()
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
            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");

            workspace.AddDocument(project.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(workspace.CurrentSolution);

            // act
            TestProject[] projects = await _sut.GetAllTestProjectsAsync();

            // assert
            Assert.That(projects.Length, Is.EqualTo(1));
            Assert.That(projects[0].Project.Name, Is.EqualTo("foo"));
            Assert.IsTrue(projects[0].IsCoverageEnabled);
        }

        [Test]
        public async void ShouldNot_TestProjectTestProjectsStoredInFile_When_StoredSettingsAreAvailable_And_SolutionDoesContainsThatProject()
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
            TestProject[] projects = await _sut.GetAllTestProjectsAsync();

            // assert
            Assert.That(projects.Length, Is.EqualTo(0));
        }

        [Test]
        public async void Should_ReturnTestProjects_With_ReferencedCoveredProjects()
        {
            // arrange            
            _settingsStoreMock.Read().Returns(new CoverageSettings());

            var workspace = new AdhocWorkspace();

            var nestedCoveredProject = workspace.AddProject("MathHelper.Utils", LanguageNames.CSharp);
            var coveredProject = workspace.AddProject("MathHelper.Logic", LanguageNames.CSharp);
            var testProject = workspace.AddProject("MathHelper.Tests", LanguageNames.CSharp);

            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");
            workspace.AddDocument(testProject.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            Solution solution = workspace.CurrentSolution.AddProjectReference(testProject.Id, new ProjectReference(coveredProject.Id));
            solution = solution.AddProjectReference(coveredProject.Id, new ProjectReference(nestedCoveredProject.Id));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(solution);

            // act
            Project[] projects = await _sut.GetAllTestProjectsWithCoveredProjectsAsync();

            // assert
            Assert.That(projects.Length, Is.EqualTo(3));
            CollectionAssert.Contains(projects.Select(x => x.Name), testProject.Name);
            CollectionAssert.Contains(projects.Select(x => x.Name), coveredProject.Name);
            CollectionAssert.Contains(projects.Select(x => x.Name), nestedCoveredProject.Name);
        }

        [Test]
        public async void ShouldNot_DuplicateProjects_Which_Were_ReferencesTwoTimes()
        {
            // arrange            
            _settingsStoreMock.Read().Returns(new CoverageSettings());

            var workspace = new AdhocWorkspace();

            var nestedCoveredProject = workspace.AddProject("MathHelper.Utils", LanguageNames.CSharp);
            var coveredProject = workspace.AddProject("MathHelper.Logic", LanguageNames.CSharp);
            var testProject = workspace.AddProject("MathHelper.Tests", LanguageNames.CSharp);

            var testClass = CSharpSyntaxTree.ParseText(@"[TestFixtureViewModel]class MathHelperTests{ [Test]void Test(){}}");
            workspace.AddDocument(testProject.Id, "MathHelperTests.cs", SourceText.From(testClass.ToString()));

            Solution solution = workspace.CurrentSolution.AddProjectReference(testProject.Id, new ProjectReference(coveredProject.Id));
            solution = solution.AddProjectReference(coveredProject.Id, new ProjectReference(nestedCoveredProject.Id));
            solution = solution.AddProjectReference(testProject.Id, new ProjectReference(nestedCoveredProject.Id));

            _testExtractorMock.GetTestClasses(Arg.Any<SyntaxNode>()).Returns(new[] { testClass.GetRoot().GetClassDeclarationSyntax() });
            _solutionExplorerMock.Solution.Returns(solution);

            // act
            Project[] projects = await _sut.GetAllTestProjectsWithCoveredProjectsAsync();

            // assert
            Assert.That(projects.Length, Is.EqualTo(3));
        }
    }
}