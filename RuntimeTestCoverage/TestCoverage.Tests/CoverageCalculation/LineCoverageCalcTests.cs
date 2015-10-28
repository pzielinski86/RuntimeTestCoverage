using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [SetUp]
        public void Setup()
        {
            _compilerMock = Substitute.For<ICompiler>();
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();
            _testRunnerMock = Substitute.For<ITestRunner>();
            _coverageStoreMock = Substitute.For<ICoverageStore>();

            _sut = new LineCoverageCalc(_solutionExplorerMock,
                _compilerMock,
                _coverageStoreMock,
                _testRunnerMock);
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
            var rewrittenDocument1=new RewrittenDocument(null, rewrittenTree,null);
            rewriteResult.Items[project1] = new List<RewrittenDocument>() {rewrittenDocument1};

            // act
            _sut.CalculateForAllTests(rewriteResult);

            // assert
            _compilerMock.Received(1).Compile
                (Arg.Is<IEnumerable<CompilationItem>>(x => x.First().SyntaxTrees[0] == rewriteResult.ToCompilationItems().First().SyntaxTrees[0])
                , rewriteResult.AuditVariablesMap);
        }

        //[Test]
        //public void Should_ReturnLineCoverage_With_Failure_When_AssertionFails()
        //{
        //    // given
        //    var auditVariablesMap = new AuditVariablesMap();
        //    auditVariablesMap.Map["1"] = new AuditVariablePlaceholder("path1", string.Empty, 0);

        //    var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
        //    var workspace = new AdhocWorkspace();

        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
        //    {
        //        new RewrittenItemInfo("path1", syntaxTree1)
        //    };

        //    var testClasses = new[] { syntaxTree1.GetRoot().GetClassDeclarationSyntax() };
        //    MethodDeclarationSyntax testMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails(testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);
        //    _testExecutorScriptEngineMock.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //    Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>()).
        //    Returns(new TestRunResult(new[] { "1" }, true, null));

        //    // when
        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    var output = _lineCoverageCalc.CalculateForAllTests(rewriteResult);

        //    // then
        //    Assert.IsFalse(output.First().IsSuccess);
        //}

        //[Test]
        //public void Should_ReturnCoverageForAllDocuments()
        //{
        //    // given
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
        //    auditVariablesMap.Map["1"] = new AuditVariablePlaceholder("path1", string.Empty, 0);
        //    auditVariablesMap.Map["2"] = new AuditVariablePlaceholder("path2", string.Empty, 0);

        //    var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
        //    var workspace = new AdhocWorkspace();

        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
        //    {
        //        new RewrittenItemInfo("path1", syntaxTree1),
        //        new RewrittenItemInfo("path2", syntaxTree1),
        //    };

        //    var testClasses = new[] { syntaxTree1.GetRoot().GetClassDeclarationSyntax() };
        //    var testMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails((ClassDeclarationSyntax)testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);

        //    _testExecutorScriptEngineMock.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //        Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>()).
        //        Returns(new TestRunResult(new[] { "1", "2" }, false, null));

        //    // when
        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    LineCoverage[] output = _lineCoverageCalc.CalculateForAllTests(rewriteResult);

        //    // then
        //    Assert.That(output[0].DocumentPath, Is.EqualTo("path1"));
        //    Assert.That(output[1].DocumentPath, Is.EqualTo("path2"));
        //}

        //[Test]
        //public void Should_PassTestProjectReferencesToTestExecutor()
        //{
        //    // given
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
        //    MetadataReference[] expectedTestProjectReferences = new MetadataReference[2];

        //    var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
        //    var workspace = new AdhocWorkspace();

        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
        //    {
        //        new RewrittenItemInfo("path1", syntaxTree1),
        //    };

        //    var testClasses = new[] { syntaxTree1.GetRoot().GetClassDeclarationSyntax() };
        //    var testMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails(testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);
        //    _solutionExplorerMock.GetProjectReferences(project1).Returns(expectedTestProjectReferences);

        //    // when
        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    _lineCoverageCalc.CalculateForAllTests(rewriteResult);

        //    // then
        //    _testExecutorScriptEngineMock.Received(1).RunTest(expectedTestProjectReferences, Arg.Any<Assembly[]>(),
        //        Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>());
        //}


        //[Test]
        //public void Should_PassTestMethod_To_TestExecutor()
        //{
        //    // given
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();

        //    var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
        //    var workspace = new AdhocWorkspace();

        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
        //    {
        //        new RewrittenItemInfo("path1", syntaxTree1),
        //    };

        //    var testClasses = new[] { syntaxTree1.GetRoot().GetClassDeclarationSyntax() };
        //    var testMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails(testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);

        //    // when
        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    _lineCoverageCalc.CalculateForAllTests(rewriteResult);

        //    // then
        //    _testExecutorScriptEngineMock.Received(1).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //        testFixtureDetails.Cases[0], Arg.Any<AuditVariablesMap>());
        //}

        //[Test]
        //public void Should_RunAllTests8Times_When_ThereAre_TwoDocuments_Containing_TwoTestClasses_With_TwoTestMethodsEach()
        //{
        //    // given
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
        //    var rewrittenItemsByProject = new Dictionary<Project, List<RewrittenItemInfo>>();
        //    var workspace = new AdhocWorkspace();

        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);

        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    rewrittenItemsByProject[project1] = new List<RewrittenItemInfo>
        //    {
        //        new RewrittenItemInfo("path1", syntaxTree1),
        //        new RewrittenItemInfo("path2", syntaxTree1)
        //    };

        //    var testClasses = new ClassDeclarationSyntax[2];
        //    testClasses[0] = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot().GetClassDeclarationSyntax();
        //    testClasses[1] = CSharpSyntaxTree.Create((CSharpSyntaxNode)syntaxTree1.GetRoot()).GetRoot().GetClassDeclarationSyntax();

        //    var testFixtureDetails = new TestFixtureDetails();
        //    TestCase[] testCases = new TestCase[2] { new TestCase(testFixtureDetails), new TestCase(testFixtureDetails) };
        //    testCases[0].SyntaxNode = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    testCases[0].SyntaxNode = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        //    testFixtureDetails.Cases.AddRange(testCases);

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails(testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);
        //    _sut.GetTestFixtureDetails(testClasses[1], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);

        //    // when
        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    _lineCoverageCalc.CalculateForAllTests(rewriteResult);

        //    // then
        //    _testExecutorScriptEngineMock.Received(8).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //        Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>());
        //}

        //private bool DoesContainExpectedCompilationItems(IEnumerable<CompilationItem> expectedCompilationItems, Dictionary<Project, List<RewrittenItemInfo>> rewrittenItemsByProject)
        //{
        //    var compilationItems = expectedCompilationItems as CompilationItem[] ?? expectedCompilationItems.ToArray();

        //    if (compilationItems.Count() != rewrittenItemsByProject.Count)
        //        return false;

        //    foreach (Project project in rewrittenItemsByProject.Keys)
        //    {
        //        var item = compilationItems.SingleOrDefault(x => x.Project == project);

        //        if (item?.SyntaxTrees.Length != rewrittenItemsByProject[project].Count)
        //            return false;

        //        for (int i = 0; i < item.SyntaxTrees.Length; i++)
        //        {
        //            if (item.SyntaxTrees[i] != rewrittenItemsByProject[project][i].SyntaxTree)
        //                return false;
        //        }
        //    }

        //    return true;
        //}

        //[Test]
        //public void Should_RunTestsFromRewrittenDocument_When_RewrittenDocumentIsTestFixture()
        //{
        //    // given
        //    const string documentPath = "c:\\1.cs";
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
        //    auditVariablesMap.Map["1"] = new AuditVariablePlaceholder("path1", string.Empty, 0);

        //    var workspace = new AdhocWorkspace();
        //    var project1 = workspace.AddProject("foo1.dll", LanguageNames.CSharp);
        //    var syntaxTree1 = CSharpSyntaxTree.ParseText("class Class1{ [TestFixture]void Test1(){}}");

        //    var testClasses = new[] { syntaxTree1.GetRoot().GetClassDeclarationSyntax() };
        //    MethodDeclarationSyntax testMethod = syntaxTree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(syntaxTree1.GetRoot()).Returns(testClasses);
        //    _sut.GetTestFixtureDetails(testClasses[0], Arg.Any<ISemanticModel>()).Returns(testFixtureDetails);

        //    _testExecutorScriptEngineMock.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //    Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>()).
        //    Returns(new TestRunResult(new[] { "1" }, false, null));

        //    _solutionExplorerMock.GetProjectByDocument(documentPath).Returns(project1);

        //    // when

        //    RewrittenDocument rewrittenDocument = new RewrittenDocument(auditVariablesMap, syntaxTree1, documentPath);
        //    _lineCoverageCalc.CalculateForDocument(rewrittenDocument, project1);

        //    // then
        //    _testExecutorScriptEngineMock.Received(1).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //       testFixtureDetails.Cases[0], Arg.Any<AuditVariablesMap>());
        //}

        //[Test]
        //public void Should_RunTestsCoveringTheDocument_When_RewrittenDocumentIsNotTextFixture()
        //{
        //    // given
        //    const string documentPath = "c:\\Helper.cs";
        //    AuditVariablesMap auditVariablesMap = new AuditVariablesMap();
        //    auditVariablesMap.Map["1"] = new AuditVariablePlaceholder("path1", string.Empty, 0);

        //    var workspace = new AdhocWorkspace();
        //    var project1 = workspace.AddProject("LogicProject", LanguageNames.CSharp);
        //    var documentTree = CSharpSyntaxTree.ParseText("class Helper{ public void Logic(){}}");
        //    var testCoveringDocumentTree = CSharpSyntaxTree.ParseText("class HelperTests{ public void LogicTest(){}}");

        //    LineCoverage[] allSolutionCoverage = { new LineCoverage() };
        //    allSolutionCoverage[0].Path = "LogicProject.Helper.Helper.Logic";
        //    allSolutionCoverage[0].TestDocumentPath = "c:\\TestDocumentPath.cs";
        //    _solutionExplorerMock.OpenFile(allSolutionCoverage[0].TestDocumentPath).Returns(testCoveringDocumentTree);

        //    _coverageStoreMock.ReadAll().Returns(allSolutionCoverage);

        //    MethodDeclarationSyntax testMethod = testCoveringDocumentTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        //    TestFixtureDetails testFixtureDetails = new TestFixtureDetails();
        //    testFixtureDetails.Cases.AddRange(new[] { new TestCase(testFixtureDetails) { SyntaxNode = testMethod } });

        //    _sut.GetTestClasses(documentTree.GetRoot()).Returns(new ClassDeclarationSyntax[0]);
        //    _sut.GetTestFixtureDetails(testCoveringDocumentTree.GetRoot().GetClassDeclarationSyntax(), Arg.Any<ISemanticModel>())
        //        .Returns(testFixtureDetails);

        //    _testExecutorScriptEngineMock.RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //    Arg.Any<TestCase>(), Arg.Any<AuditVariablesMap>()).
        //    Returns(new TestRunResult(new[] { "1" }, false, null));

        //    _solutionExplorerMock.GetProjectByDocument(allSolutionCoverage[0].TestDocumentPath).Returns(project1);

        //    // when
        //    RewrittenDocument rewrittenDocument = new RewrittenDocument(auditVariablesMap, documentTree, documentPath);
        //    _lineCoverageCalc.CalculateForDocument(rewrittenDocument, project1);

        //    // then
        //    _testExecutorScriptEngineMock.Received(1).RunTest(Arg.Any<MetadataReference[]>(), Arg.Any<Assembly[]>(),
        //       testFixtureDetails.Cases[0], Arg.Any<AuditVariablesMap>());
        //}
    }
}