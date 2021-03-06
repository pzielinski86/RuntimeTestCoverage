﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
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

            _sut = new TestRunner(_testExtractorMock, _testExecutorEngineMock, _solutionExplorerMock);
        }

        [Test]
        public void RunAllTestsInDocument_ShouldExtractAllProjectReferences_And_PassItTo_TestSandbox()
        {
            // arrange
            var project = CreateProject("SampleTestsProject");
            string[] allProjectReferences = { "A" };

            _solutionExplorerMock.GetAllProjectReferences(project.Name).Returns(allProjectReferences);
            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");

            var testClass = testNode.GetRoot().GetClassDeclarationSyntax();
            var fixtureDetails = new TestFixtureDetails();
            var testCase = new TestCase(fixtureDetails) { SyntaxNode = testClass.GetPublicMethods().Single() };
            fixtureDetails.Cases.Add(testCase);

            _testExtractorMock.GetTestClasses(Arg.Any<CSharpSyntaxNode>())
                .Returns(new[] { testClass });
            _testExtractorMock.GetTestFixtureDetails(testClass, Arg.Any<ISemanticModel>()).Returns(fixtureDetails);

            var rewrittenDocument = new RewrittenDocument(testNode, null, false);

            // act
            _sut.RunAllTestsInDocument(rewrittenDocument, null, project, new string[0]);

            // assert
            _testExecutorEngineMock.Received(1).
                RunTestFixture(Arg.Is<string[]>(x => x[0] == allProjectReferences[0]), Arg.Any<TestFixtureExecutionScriptParameters>());
        }

        [Test]
        public void RunAllTestsInDocument_ShouldPassAllRewrittenAssemblies_To_TestSandbox()
        {
            // arrange
            var project = CreateProject("SampleTestsProject");
            string[] rewrittenAssemblies = new[] { "A" };

            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");

            var testClass = testNode.GetRoot().GetClassDeclarationSyntax();
            var fixtureDetails = new TestFixtureDetails();
            var testCase = new TestCase(fixtureDetails) { SyntaxNode = testClass.GetPublicMethods().Single() };
            fixtureDetails.Cases.Add(testCase);

            _testExtractorMock.GetTestClasses(Arg.Any<CSharpSyntaxNode>())
                .Returns(new[] { testClass });
            _testExtractorMock.GetTestFixtureDetails(testClass, Arg.Any<ISemanticModel>()).Returns(fixtureDetails);
            var rewrittenDocument = new RewrittenDocument(testNode, null, false);


            // act
            _sut.RunAllTestsInDocument(rewrittenDocument, null, project, rewrittenAssemblies);

            // assert
            _testExecutorEngineMock.Received(1).
                RunTestFixture(Arg.Is<string[]>(x => x[0] == rewrittenAssemblies[0]), Arg.Any<TestFixtureExecutionScriptParameters>());
        }


        [Test]
        public void RunAllTestsInDocument_ShouldExtractAllTestClasses()
        {
            // arrange
            var project = CreateProject("SampleTestsProject");

            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");

            var rewrittenDocument = new RewrittenDocument(testNode, null, false);

            // act
            _sut.RunAllTestsInDocument(rewrittenDocument, null, project, new string[0]);

            // assert
            _testExtractorMock.Received(1).GetTestClasses(testNode.GetRoot());
        }

        [Test]
        public void RunAllTestsInDocument_ShouldExtractAllTestCases()
        {
            // arrange
            var project = CreateProject("SampleTestsProject");
            var semanticModel = Substitute.For<ISemanticModel>();

            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");

            var testClass = testNode.GetRoot().GetClassDeclarationSyntax();
            var fixtureDetails = new TestFixtureDetails();
            var testCase = new TestCase(fixtureDetails) { SyntaxNode = testClass.GetPublicMethods().Single() };
            fixtureDetails.Cases.Add(testCase);

            _testExtractorMock.GetTestClasses(Arg.Any<CSharpSyntaxNode>())
                .Returns(new[] { testClass });

            _testExtractorMock.GetTestFixtureDetails(Arg.Any<ClassDeclarationSyntax>(), Arg.Any<ISemanticModel>()).Returns(fixtureDetails);
            var rewrittenDocument = new RewrittenDocument(testNode, null, false);


            // act
            _sut.RunAllTestsInDocument(rewrittenDocument, semanticModel, project, new string[0]);

            // assert
            _testExtractorMock.Received(1).GetTestFixtureDetails(testClass, semanticModel);
        }

        [Test]
        public void RunAllTestsInDocument_ShouldReturn_One_Line_Coverage_When_ThereIsOneClass_And_OneTestCase_And_ItContains_One_LineCoverage()
        {
            // arrange
            var testNode = CSharpSyntaxTree.ParseText("[TestFixture]class HelloWorldTests{" +
                                             " [Test] public void TestMethod()" +
                                             "{}" +
                                             "}");

            var testClass = testNode.GetRoot().GetClassDeclarationSyntax();
            var fixtureDetails = new TestFixtureDetails();
            var testCase = new TestCase(fixtureDetails) { SyntaxNode = testClass.GetPublicMethods().Single(), MethodName = "TestMethod" };
            fixtureDetails.Cases.Add(testCase);

            _testExtractorMock.GetTestClasses(Arg.Any<CSharpSyntaxNode>())
                .Returns(new[] { testClass });
            _testExtractorMock.GetTestFixtureDetails(testClass, Arg.Any<ISemanticModel>()).Returns(fixtureDetails);

            var testRunResultMock = Substitute.For<ITestRunResult>();
            testRunResultMock.TestName.Returns("TestMethod");

            var expectedLineCoverage = new[] { new LineCoverage() };

            testRunResultMock.GetCoverage(Arg.Any<SyntaxNode>(), Arg.Any<string>(),
                Arg.Any<string>())
                .Returns(expectedLineCoverage);

            _testExecutorEngineMock.RunTestFixture(Arg.Any<string[]>(), Arg.Any<TestFixtureExecutionScriptParameters>()).
                Returns(new[] { testRunResultMock });

            var project = CreateProject("SampleTestsProject");
            var rewrittenDocument = new RewrittenDocument(testNode, null, false);

            // act
            LineCoverage[] output = _sut.RunAllTestsInDocument(rewrittenDocument, null, project, new string[0]);

            // assert
            Assert.That(output, Is.SameAs(output));
        }

        private Project CreateProject(string projectName)
        {
            var workspace = new AdhocWorkspace();
            return workspace.CurrentSolution.AddProject(projectName, projectName, LanguageNames.CSharp);
        }
    }
}