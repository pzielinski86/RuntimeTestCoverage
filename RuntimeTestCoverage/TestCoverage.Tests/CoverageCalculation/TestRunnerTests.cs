using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class TestRunnerTests
    {
        private ITestRunner _sut;
        private ISolutionExplorer _solutionExplorerMock;
        private ITestExecutorScriptEngine _testExecutorEngineMock;
        private ITestsExtractor _testExtractorMock;

        [SetUp]
        public void Setup()
        {
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();
            _testExecutorEngineMock = Substitute.For<ITestExecutorScriptEngine>();
            _testExtractorMock = Substitute.For<ITestsExtractor>();
                
            _sut =new TestRunner(_testExtractorMock,_testExecutorEngineMock,_solutionExplorerMock);
        }

        [Test]
        public void RunAllTestsInDocument_ShouldReturn_One_Line_Coverage_When_ThereIsOneClass_And_OneTestCase()
        {
            // arrange
            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");
            var testClass = testNode.GetRoot().GetClassDeclarationSyntax();
            var fixtureDetails=new TestFixtureDetails();
            var testCase = new TestCase(fixtureDetails) {SyntaxNode = testClass.GetPublicMethods().Single()};
            fixtureDetails.Cases.Add(testCase);

            var rewrittenDocument = new RewrittenDocument(new AuditVariablesMap(), testNode, null);
            var project = CreateProject("SampleTestsProject");

            _testExtractorMock.GetTestClasses(Arg.Any<CSharpSyntaxNode>())
                .Returns(new[] { testClass });
            _testExtractorMock.GetTestFixtureDetails(testClass, Arg.Any<ISemanticModel>()).Returns(fixtureDetails);
            _testExecutorEngineMock.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(), testCase,
                Arg.Any<AuditVariablesMap>());

            // act
            _sut.RunAllTestsInDocument(rewrittenDocument, null, project, null);
        }

        private Project CreateProject(string projectName)
        {
            var workspace=new AdhocWorkspace();
            return workspace.CurrentSolution.AddProject(projectName, projectName, LanguageNames.CSharp);
        }
    }
}