using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;
using TestCoverage.Monitors;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    public class VsSolutionTestCoverageTests
    {
        private VsSolutionTestCoverage _sut;
        private ISolutionCoverageEngine _solutionCoverageEngineMock;
        private ICoverageStore _coverageStoreMock;
        private ISolutionWatcher _solutionWatcherMock;

        [SetUp]
        public void Setup()
        {
            _solutionCoverageEngineMock = Substitute.For<ISolutionCoverageEngine>();
            _coverageStoreMock = Substitute.For<ICoverageStore>();
            _solutionWatcherMock = Substitute.For<ISolutionWatcher>();

            Workspace workspace = new AdhocWorkspace();
            _sut = new VsSolutionTestCoverage(workspace, 
                _solutionCoverageEngineMock, 
                _coverageStoreMock, 
                _solutionWatcherMock);
        }


        [Test]
        public void LoadCurrentCoverage_Should_LoadDataForAllDocuments()
        {
            // arrange
            var doc1Coverage = new LineCoverage { DocumentPath = "doc1.cs" };
            var doc2Coverage = new LineCoverage { DocumentPath = "doc2.cs" };

            _coverageStoreMock.ReadAll().Returns(new[] { doc1Coverage, doc2Coverage });

            // act
            _sut.LoadCurrentCoverage();

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(2));
            Assert.That(_sut.SolutionCoverageByDocument["doc1.cs"].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument["doc2.cs"].Count, Is.EqualTo(1));

            Assert.That(_sut.SolutionCoverageByDocument["doc1.cs"].First(), Is.EqualTo(doc1Coverage));
            Assert.That(_sut.SolutionCoverageByDocument["doc2.cs"].First(), Is.EqualTo(doc2Coverage));
        }

        [Test]
        public void LoadCurrentCoverage_Should_ClearPreviousCoverage_BeforeLoadingDataFromStore()
        {
            // arrange
            var doc1Coverage = new LineCoverage { DocumentPath = "doc1.cs" };

            _coverageStoreMock.ReadAll().Returns(new[] { doc1Coverage });
            _sut.SolutionCoverageByDocument.Add("oldDocument.cs", new List<LineCoverage>());

            // act
            _sut.LoadCurrentCoverage();

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument.Keys.First(), Is.EqualTo("doc1.cs"));
        }

        [Test]
        public async Task CalculateForMethod_Should_RemoveOldCoverageValues()
        {
            // arrange
            const string code = "class MathHelperTests{" +
                                " [Test] public void Test() " +
                                "{}" +
                                "}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var method = tree.GetRoot().GetPublicMethods().First();

            const string testDocumentPath = "MathHelperTests.cs";

            var oldTestLineCoverage = new LineCoverage();
            oldTestLineCoverage.DocumentPath = testDocumentPath;
            oldTestLineCoverage.NodePath = "CurrentProject.MathHelperTests.Test";
            oldTestLineCoverage.TestPath = "CurrentProject.MathHelperTests.Test";

            var newTestLineCoverage = new LineCoverage();
            newTestLineCoverage.DocumentPath = testDocumentPath;
            newTestLineCoverage.NodePath = "CurrentProject.MathHelperTests.Test";
            newTestLineCoverage.TestPath = "CurrentProject.MathHelperTests.Test";

            _sut.SolutionCoverageByDocument.Add(testDocumentPath, new List<LineCoverage>() { oldTestLineCoverage });

            _solutionCoverageEngineMock.CalculateForMethod(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>()).
                Returns(new CoverageResult(new[] { newTestLineCoverage }));

            // act
            await _sut.CalculateForSelectedMethodAsync("CurrentProject", method);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].First(), Is.EqualTo(newTestLineCoverage));
        }

        [Test]
        public async Task When_CalculateForMethod_Fails_Should_MarkCoverageAsFaileddOnlyFromExecutedPath()
        {
            // arrange
            const string code = "class MathHelperTests{" +
                                " [Test] public void Test() " +
                                "{}" +
                                " [Test] public void Test2() " +
                                "{}" +
                                "}";

            var tree = CSharpSyntaxTree.ParseText(code);
            var method = tree.GetRoot().GetPublicMethods()[1];
            const string testDocumentPath = "MathHelperTests.cs";

            var oldTestLineCoverage1 = new LineCoverage();
            oldTestLineCoverage1.DocumentPath = testDocumentPath;
            oldTestLineCoverage1.NodePath = "CurrentProject..MathHelperTests.AnotherTest";
            oldTestLineCoverage1.TestPath = "CurrentProject..MathHelperTests.AnotherTest";

            var coverageToBeRecalculated = new LineCoverage();
            coverageToBeRecalculated.DocumentPath = testDocumentPath;
            coverageToBeRecalculated.NodePath = "CurrentProject..MathHelperTests.Test2";
            coverageToBeRecalculated.TestPath = "CurrentProject..MathHelperTests.Test2";

            _sut.SolutionCoverageByDocument.Add(testDocumentPath, new List<LineCoverage>() { oldTestLineCoverage1, coverageToBeRecalculated });

            _solutionCoverageEngineMock.CalculateForMethod(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>()).
                Throws(new TestCoverageCompilationException(new string[1]));

            // act
            await _sut.CalculateForSelectedMethodAsync("CurrentProject", method);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].Count, Is.EqualTo(2));
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath][0], Is.EqualTo(oldTestLineCoverage1));

            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath][1].NodePath, Is.EqualTo(coverageToBeRecalculated.NodePath));
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath][1].TestPath, Is.EqualTo(coverageToBeRecalculated.TestPath));
            Assert.IsFalse(_sut.SolutionCoverageByDocument[testDocumentPath][1].IsSuccess);
            Assert.IsNotNull(_sut.SolutionCoverageByDocument[testDocumentPath][1].ErrorMessage);
        }

        [Test]
        public async Task CalculateForMethod_ShouldNot_RemoveUnrelatedMethods()
        {
            // arrange
            const string code = "class MathHelperTests{" +
                                " [Test] public void Test() " +
                                "{}" +
                                "}";
            var tree = CSharpSyntaxTree.ParseText(code);
            var method = tree.GetRoot().GetPublicMethods().First();
            const string testDocumentPath = "MathHelperTests.cs";

            var oldTestLineCoverage = new LineCoverage();
            oldTestLineCoverage.DocumentPath = testDocumentPath;
            oldTestLineCoverage.NodePath = "CurrentProject.MathHelperTests.Test";
            oldTestLineCoverage.TestPath = "CurrentProject.MathHelperTests.Test";

            var newTestLineCoverage = new LineCoverage();
            newTestLineCoverage.DocumentPath = testDocumentPath;
            newTestLineCoverage.NodePath = "CurrentProject.MathHelperTests.Test2";
            newTestLineCoverage.TestPath = "CurrentProject.MathHelperTests.Test2";

            _sut.SolutionCoverageByDocument.Add(testDocumentPath, new List<LineCoverage>() { oldTestLineCoverage });

            _solutionCoverageEngineMock.CalculateForMethod(Arg.Any<string>(), Arg.Any<MethodDeclarationSyntax>()).
                Returns(new CoverageResult(new[] { newTestLineCoverage }));

            // act
            await _sut.CalculateForSelectedMethodAsync("CurrentProject", method);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].Count, Is.EqualTo(2));
        }

        [Test]
        public async Task CalculateForDocument_Should_RemoveOldCoverageValues()
        {
            // arrange
            const string testDocumentPath = "MathHelperTests.cs";

            var oldTestLineCoverage = new LineCoverage();
            oldTestLineCoverage.DocumentPath = testDocumentPath;
            oldTestLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            oldTestLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var newTestLineCoverage = new LineCoverage();
            newTestLineCoverage.DocumentPath = testDocumentPath;
            newTestLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            newTestLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add(testDocumentPath, new List<LineCoverage>() { oldTestLineCoverage });

            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new[] { newTestLineCoverage }));

            // act
            await _sut.CalculateForDocumentAsync("CurrentProject", testDocumentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].First(), Is.EqualTo(newTestLineCoverage));
        }

        [Test]
        public async Task CalculateForDocument_Should_PopulateCoverageWithNewData_When_NewDataIsAvailable()
        {
            // arrange
            const string newDocumentPath = "MathHelper.cs";

            var coverage = new LineCoverage { DocumentPath = newDocumentPath };

            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new[] { coverage }));

            // act

            await _sut.CalculateForDocumentAsync("CurrentProject", "MathHelperTests.cs", string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath][0], Is.EqualTo(coverage));
        }

        [Test]
        public async Task CalculateForDocument_ShouldNot_ClearCoverageOfUnrelatedDocuments()
        {
            // arrange
            var testLineCoverage = new LineCoverage();
            testLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            testLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var codeLineCoverage = new LineCoverage();
            codeLineCoverage.NodePath = "CurrentProject.MathHelper";
            codeLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add("doc1.xml", new List<LineCoverage>() { testLineCoverage, codeLineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Throws(new TestCoverageCompilationException(new string[0]));

            // act
            await _sut.CalculateForDocumentAsync("CurrentProject", "test.xml", string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CalculateForAllDocuments_ShouldNot_ClearCoverageOfUnrelatedDocuments()
        {
            // arrange
            var testLineCoverage = new LineCoverage();
            testLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            testLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var codeLineCoverage = new LineCoverage();
            codeLineCoverage.NodePath = "CurrentProject.MathHelper";
            codeLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add("doc1.xml", new List<LineCoverage>() { testLineCoverage, codeLineCoverage });
            _solutionCoverageEngineMock.CalculateForAllDocumentsAsync().
                Throws(new TestCoverageCompilationException(new string[0]));

            // act
            await _sut.CalculateForAllDocumentsAsync();

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CalculateForDocument_ShouldClearAllCoverage_When_CompilationExceptionIsThrown()
        {
            // arrange
            const string documentPath = "EmployeeRepository.cs";
            var lineCoverage = new LineCoverage();
            lineCoverage.TestPath = lineCoverage.NodePath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add(documentPath, new List<LineCoverage>() { lineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new LineCoverage[0]));
            // act
            await _sut.CalculateForDocumentAsync("CurrentProject", documentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[documentPath].Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveByFilePath_Should_RemoveAllCoverage_From_ThatFile()
        {
            // arrange
            _sut.SolutionCoverageByDocument.Add("Tests.cs", new List<LineCoverage>() { new LineCoverage() { DocumentPath = "Tests.cs" } });

            // act
            _solutionWatcherMock.DocumentRemoved+=Raise.EventWith(null,new DocumentRemovedEventArgs("Tests.cs"));

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveByFilePath_Should_RemoveCoverageFromOtherFiles_WhichAreRelatedTo_TheRemovedFile()
        {
            // arrange
            _sut.SolutionCoverageByDocument.Add("Tests.cs",
                new List<LineCoverage>() {new LineCoverage() {DocumentPath = "Tests.cs", TestPath = "Tests.Method"}});
            _sut.SolutionCoverageByDocument.Add("Sut.cs",
                new List<LineCoverage>() {new LineCoverage() {TestDocumentPath = "Tests.cs", TestPath = "Tests.Method"}});

            // act
            _solutionWatcherMock.DocumentRemoved += Raise.EventWith(null, new DocumentRemovedEventArgs("Tests.cs"));

            // assert
            Assert.IsFalse(_sut.SolutionCoverageByDocument.ContainsKey("Tests.cs"));
            Assert.That(_sut.SolutionCoverageByDocument["Sut.cs"].Count,Is.EqualTo(0));
        }
    }
}