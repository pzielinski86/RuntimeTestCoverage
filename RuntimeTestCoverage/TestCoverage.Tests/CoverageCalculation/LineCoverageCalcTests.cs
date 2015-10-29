using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Rewrite;
using TestCoverage.Storage;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class LineCoverageCalcTests
    {
        private LineCoverageCalc _sut;

        private ISolutionExplorer _solutionExplorerMock;
        private ICompiler _compilerMock;
        private ICoverageStore _coverageStoreMock;
        private ITestRunner _testRunnerMock;
        private List<ICompiledItem> _compiledAllItems;
        private List<ICompiledItem> _compiledSingleProjectItems;
        private ITestExplorer _testExplorerMock;

        [SetUp]
        public void Setup()
        {
            _compilerMock = Substitute.For<ICompiler>();
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();
            _testRunnerMock = Substitute.For<ITestRunner>();
            _coverageStoreMock = Substitute.For<ICoverageStore>();
            _compiledAllItems = new List<ICompiledItem>();
            _compiledSingleProjectItems=new List<ICompiledItem>();
            _testExplorerMock = Substitute.For<ITestExplorer>();

            _testExplorerMock.SolutionExplorer.Returns(_solutionExplorerMock);

            _sut = new LineCoverageCalc(_testExplorerMock,
                _compilerMock,
                _coverageStoreMock,
                _testRunnerMock);

            _compilerMock.Compile(Arg.Any<CompilationItem>(), Arg.Any<IEnumerable<_Assembly>>(),
                Arg.Any<AuditVariablesMap>())
                .Returns((x) => _compiledSingleProjectItems.ToArray());

            _compilerMock.Compile(Arg.Any<IEnumerable<CompilationItem>>(), Arg.Any<AuditVariablesMap>())
                .Returns((x) => _compiledAllItems.ToArray());
        }

        [Test]
        public void CalculateForAllTests_Should_CompileProvidedDocuments()
        {
            // arrange
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenDocument>>();

            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);

            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, new AuditVariablesMap());
            var rewrittenTree = CSharpSyntaxTree.ParseText("");

            var rewrittenDocument1 = new RewrittenDocument(null, rewrittenTree, null);
            rewriteResult.Items[project1] = new List<RewrittenDocument>() { rewrittenDocument1 };

            var compiledItem = Substitute.For<ICompiledItem>();
            compiledItem.Project.Returns(project1);
            _compiledAllItems.Add(compiledItem);

            // act
            _sut.CalculateForAllTests(rewriteResult);

            // assert
            _compilerMock.Received(1).Compile
                (Arg.Is<IEnumerable<CompilationItem>>(x => x.First().SyntaxTrees[0] ==
                rewriteResult.ToCompilationItems().First().SyntaxTrees[0]),
                rewriteResult.AuditVariablesMap);
        }

        [Test]
        public void CalculateForAllTests_Should_Return_OneCoverage_From_AllTests_When_There_IsOneProject_And_OneLineCoverage()
        {
            // arrange
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenDocument>>();
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);

            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, new AuditVariablesMap());
            var rewrittenTree = CSharpSyntaxTree.ParseText("");

            var rewrittenDocument1 = new RewrittenDocument(null, rewrittenTree, null);
            rewriteResult.Items[project1] = new List<RewrittenDocument>() { rewrittenDocument1 };

            var semanticModel = Substitute.For<ISemanticModel>();
            var compiledItem = Substitute.For<ICompiledItem>();
            var assembly = Substitute.For<_Assembly>();

            compiledItem.Project.Returns(project1);
            compiledItem.Assembly.Returns(assembly);
            compiledItem.GetSemanticModel(rewrittenDocument1.SyntaxTree).Returns(semanticModel);
            _compiledAllItems.Add(compiledItem);

            var expectedLineCoverage = new[] {new LineCoverage()};
            _testRunnerMock.RunAllTestsInDocument(rewrittenDocument1, 
                semanticModel, 
                project1, 
                Arg.Is<_Assembly[]>(x=>assembly==x[0]))
                .Returns(expectedLineCoverage);

            // act
            LineCoverage[] output = _sut.CalculateForAllTests(rewriteResult);

            // assert
            Assert.That(output, Is.EquivalentTo(expectedLineCoverage));
        }

        [Test]
        public void CalculateForAllTests_Should_Return_TwoLinesCoverage_From_AllTests_When_There_AreTwoProjects_And_EachProjectHasOneLineCoverage()
        {
            // arrange
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenDocument>>();
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var project2 = workspace.AddProject("foo2.dll", LanguageNames.CSharp);

            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, new AuditVariablesMap());
            var rewrittenTree = CSharpSyntaxTree.ParseText("");

            var rewrittenDocument1 = new RewrittenDocument(null, rewrittenTree, null);
            rewriteResult.Items[project1] = new List<RewrittenDocument>() { rewrittenDocument1 };
            rewriteResult.Items[project2] = new List<RewrittenDocument>() { rewrittenDocument1 };

            var compiledItem1 = Substitute.For<ICompiledItem>();
            var compiledItem2 = Substitute.For<ICompiledItem>();

            compiledItem1.Project.Returns(project1);
            compiledItem2.Project.Returns(project2);

            _compiledAllItems.Add(compiledItem1);
            _compiledAllItems.Add(compiledItem2);

            var expectedLineCoverage = new[] { new LineCoverage() };
            _testRunnerMock.RunAllTestsInDocument(rewrittenDocument1,
                Arg.Any<ISemanticModel>(),
                project1,
                Arg.Any<_Assembly[]>())
                .Returns(expectedLineCoverage);

            _testRunnerMock.RunAllTestsInDocument(rewrittenDocument1,
                Arg.Any<ISemanticModel>(),
                project2,
                Arg.Any<_Assembly[]>())
                .Returns(expectedLineCoverage);

            // act
            LineCoverage[] output = _sut.CalculateForAllTests(rewriteResult);

            // assert
            Assert.That(output.Length, Is.EqualTo(2));
        }

        [Test]
        public void CalculateForDocument_Should_Execute_TestsInProvidedDocuments_When_ProvidedDocumentIsTestDocument()
        {
            // arrange
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenDocument>>();
            var workspace = new AdhocWorkspace();
            var testProject = workspace.AddProject("TestProject.dll", LanguageNames.CSharp);

            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, new AuditVariablesMap());
            var rewrittenTree = CSharpSyntaxTree.ParseText("");

            var rewrittenDocument1 = new RewrittenDocument(null, rewrittenTree, null);
            rewriteResult.Items[testProject] = new List<RewrittenDocument>() { rewrittenDocument1 };

            var compiledItem1 = Substitute.For<ICompiledItem>();

            compiledItem1.Project.Returns(testProject);

            _compiledSingleProjectItems.Add(compiledItem1);

            var expectedLineCoverage = new[] { new LineCoverage() };
            _testRunnerMock.RunAllTestsInDocument(rewrittenDocument1,
                Arg.Any<ISemanticModel>(),
                testProject,
                Arg.Any<_Assembly[]>())
                .Returns(expectedLineCoverage);

            // act
            LineCoverage[] output = _sut.CalculateForDocument(testProject, rewrittenDocument1);

            // assert
            Assert.That(output.Length, Is.EqualTo(2));
        }
    }
}