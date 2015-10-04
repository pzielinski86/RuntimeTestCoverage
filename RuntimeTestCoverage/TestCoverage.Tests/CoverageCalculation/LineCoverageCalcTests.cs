using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class LineCoverageCalcTests
    {
        private ISolutionExplorer _solutionExplorerMock;
        private ICompiler _compilerMock;
        private LineCoverageCalc _lineCoverageCalc;
        private ITestsExtractor _testsExtractor;
        private ITestExecutorScriptEngine _testExecutorScriptEngine;

        [SetUp]
        public void Setup()
        {
            _compilerMock = NSubstitute.Substitute.For<ICompiler>();
            _solutionExplorerMock = NSubstitute.Substitute.For<ISolutionExplorer>();
            _testsExtractor = Substitute.For<ITestsExtractor>();
            _testExecutorScriptEngine = Substitute.For<ITestExecutorScriptEngine>();

            _testExecutorScriptEngine.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
                Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>()).Returns(new TestRunResult(new string[0], true,null));

            _lineCoverageCalc = new LineCoverageCalc(_solutionExplorerMock, _compilerMock, _testsExtractor, _testExecutorScriptEngine);
        }

        [Test]
        public void Should_CompileProvidedDocuments()
        {
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject("foo.dll", LanguageNames.CSharp);
            var syntaxTree = CSharpSyntaxTree.ParseText("class Class{}");

            rewrittenItemsByProject[project] = new List<RewrittenItemInfo>();
            rewrittenItemsByProject[project].Add(new RewrittenItemInfo("path", syntaxTree));

            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            _compilerMock.Received(1).Compile(
                Arg.Is<IEnumerable<CompilationItem>>(x => DoesContainExpectedCompilationItems(x, rewrittenItemsByProject)),
                auditVariablesMap);
        }

        [Test]
        public void Should_ReturnLineCoverage_With_Failure_When_AssertionFails()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            auditVariablesMap.Map["1"]=new AuditVariablePlaceholder("path1",string.Empty,0);

            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1)
            };

            SyntaxNode[] testClasses = { CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot() };
            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);
            _testExecutorScriptEngine.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
            Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>()).
            Returns(new TestRunResult(new[] { "1"}, false,null));

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            var output = _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            Assert.IsFalse(output.First().IsSuccess);
        }

        [Test]
        public void Should_ReturnCoverageForAllDocuments()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            auditVariablesMap.Map["1"] = new AuditVariablePlaceholder("path1", string.Empty, 0);
            auditVariablesMap.Map["2"] = new AuditVariablePlaceholder("path2", string.Empty, 0);

            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1),
                new RewrittenItemInfo("path2", syntaxTree1),
            };

            SyntaxNode[] testClasses = { CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot() };
            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);
            _testExecutorScriptEngine.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
                Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>()).
                Returns(new TestRunResult(new []{"1","2"}, false,null));

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            LineCoverage[] output = _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            Assert.That(output[0].DocumentPath, Is.EqualTo("path1"));
            Assert.That(output[1].DocumentPath, Is.EqualTo("path2"));
        }

        [Test]
        public void Should_PassTestProjectReferencesToTestExecutor()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            MetadataReference[] expectedTestProjectReferences = new MetadataReference[2];

            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1),
            };

            SyntaxNode[] testClasses = { CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot() };
            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);
            _solutionExplorerMock.GetProjectReferences(project1).Returns(expectedTestProjectReferences);

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            _testExecutorScriptEngine.Received(1).RunTest(expectedTestProjectReferences, Arg.Any<Assembly[]>(),
                Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>());
        }

        [Test]
        public void Should_PassCompiledAssemblies_To_TestExecutor()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            var expectedAssemblies = new Assembly[2];

            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1),
            };

            SyntaxNode[] testClasses = { CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot() };
            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);
            _compilerMock.Compile(Arg.Any<IEnumerable<CompilationItem>>(), Arg.Any<AuditVariablesMap>())
                .Returns(expectedAssemblies);

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            _testExecutorScriptEngine.Received(1).RunTest(Arg.Any<MetadataReference[]>(), expectedAssemblies,
                Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>());
        }

        [Test]
        public void Should_PassTestMethod_To_TestExecutor()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1),
            };

            SyntaxNode[] testClasses = { CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot() };
            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            _testExecutorScriptEngine.Received(1).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
                testMethods[0], Arg.Any<AuditVariablesMap>());
        }

        [Test]
        public void Should_RunAllTests8Times_When_ThereAre_TwoDocuments_Containing_TwoTestClasses_With_TwoTestMethodsEach()
        {
            // given
            AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
            var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
            var workspace = new AdhocWorkspace();

            var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);

            var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

            rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
            {
                new RewrittenItemInfo("path1", syntaxTree1),
                new RewrittenItemInfo("path2", syntaxTree1)
            };

            SyntaxNode[] testClasses = new SyntaxNode[2];
            testClasses[0] = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot();
            testClasses[1] = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot();

            SyntaxNode[] testMethods = { syntaxTree1.GetRoot().ChildNodes().First(), syntaxTree1.GetRoot().ChildNodes().First() };

            _testsExtractor.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
            _testsExtractor.GetTestMethods(testClasses[0]).Returns(testMethods);
            _testsExtractor.GetTestMethods(testClasses[1]).Returns(testMethods);

            // when
            RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
            _lineCoverageCalc.CalculateForAllTests(rewriteResult);

            // then
            _testExecutorScriptEngine.Received(8).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
                Arg.Any<SyntaxNode>(), Arg.Any<AuditVariablesMap>());
        }

        private bool DoesContainExpectedCompilationItems(IEnumerable<CompilationItem> expectedCompilationItems, Dictionary<Project, List<RewrittenItemInfo>> rewrittenItemsByProject)
        {
            var compilationItems = expectedCompilationItems as CompilationItem[] ?? expectedCompilationItems.ToArray();

            if (compilationItems.Count() != rewrittenItemsByProject.Count)
                return false;

            foreach (Project project in rewrittenItemsByProject.Keys)
            {
                var item = compilationItems.SingleOrDefault(x => x.Project == project);

                if (item?.SyntaxTrees.Length != rewrittenItemsByProject[project].Count)
                    return false;

                for (int i = 0; i < item.SyntaxTrees.Length; i++)
                {
                    if (item.SyntaxTrees[i] != rewrittenItemsByProject[project][i].SyntaxTree)
                        return false;
                }
            }

            return true;
        }
    }

}