using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using TestCoverage;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Storage;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    public class VsSolutionTestCoverageTests
    {
        private VsSolutionTestCoverage _sut;
        private ISolutionCoverageEngine _solutionCoverageEngineMock;
        private ICoverageStore _coverageStoreMock;
        private ILogger _logger;
        private readonly string _solutionPath = @"c:\Project.sln";

        [SetUp]
        public void Setup()
        {
            _solutionCoverageEngineMock = Substitute.For<ISolutionCoverageEngine>();
            _coverageStoreMock = Substitute.For<ICoverageStore>();
            _logger = Substitute.For<ILogger>();

            _sut = new VsSolutionTestCoverage(_solutionPath, () => _solutionCoverageEngineMock, _coverageStoreMock, _logger);
        }

        [Test]
        public void Init_ShouldCreateNewEngine_When_CurrentIsEqualToNull()
        {
            // arrange
            var engine1= Substitute.For<ISolutionCoverageEngine>();
            var engines = new Stack<ISolutionCoverageEngine>();
            engines.Push(engine1);

            _sut = new VsSolutionTestCoverage(_solutionPath, () => engines.Pop(), _coverageStoreMock, _logger);

            // act
            var newEngine = _sut.InitAsync(false);

            // assert
            Assert.That(engines.Count,Is.EqualTo(0));
            Assert.That(newEngine,Is.SameAs(engine1));
        }

        [Test]
        public void Init_ShouldCreateNewEngine_When_CurrentEngineWasDisposed()
        {
            // arrange
            var engine1 = Substitute.For<ISolutionCoverageEngine>();
            var engine2 = Substitute.For<ISolutionCoverageEngine>();
            engine1.IsDisposed.Returns(false);
            engine2.IsDisposed.Returns(true);

            var engines = new Stack<ISolutionCoverageEngine>();
            engines.Push(engine1);
            engines.Push(engine2);

            _sut = new VsSolutionTestCoverage(_solutionPath, () => engines.Pop(), _coverageStoreMock, _logger);
            _sut.InitAsync(false);
            // act
            var newEngine = _sut.InitAsync(false);

            // assert
            Assert.That(engines.Count, Is.EqualTo(0));
            Assert.That(newEngine, Is.SameAs(engine1));
        }

        [Test]
        public void Init_ShouldNotCreateNewEngine_When_CurrentIsNotNull()
        {
            // arrange
            var engine1 = Substitute.For<ISolutionCoverageEngine>();
            var engine2 = Substitute.For<ISolutionCoverageEngine>();
            var engines = new Stack<ISolutionCoverageEngine>();
            engines.Push(engine1);
            engines.Push(engine2);

            _sut = new VsSolutionTestCoverage(_solutionPath, () => engines.Pop(), _coverageStoreMock, _logger);
            _sut.InitAsync(false);

            // act
            var newEngine = _sut.InitAsync(false);

            // assert
            Assert.That(engines.Count, Is.EqualTo(1));
            Assert.That(newEngine, Is.SameAs(engine2));
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
        public void CalculateForDocument_Should_RemvoeOldCoverageValues()
        {
            // arrange
            const string testDocumentPath = "MathHelperTests.cs";

            var oldTestLineCoverage = new LineCoverage();
            oldTestLineCoverage.DocumentPath = testDocumentPath;
            oldTestLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            oldTestLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var newTestLineCoverage = new LineCoverage();
            newTestLineCoverage.DocumentPath = testDocumentPath;
            oldTestLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            oldTestLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add(testDocumentPath, new List<LineCoverage>() { oldTestLineCoverage });

            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new[] { newTestLineCoverage }));

            // act
            _sut.CalculateForDocument("CurrentProject", testDocumentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[testDocumentPath].First(), Is.EqualTo(newTestLineCoverage));
        }

        [Test]
        public void CalculateForDocument_Should_PopulateCoverageWithNewData_When_NewDataIsAvailable()
        {
            // arrange
            const string newDocumentPath = "MathHelper.cs";

            var coverage = new LineCoverage { DocumentPath = newDocumentPath };

            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new[] { coverage }));

            // act

            _sut.CalculateForDocument("CurrentProject", "MathHelperTests.cs", string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath][0], Is.EqualTo(coverage));
        }

        [Test]
        public void CalculateForDocument_ShouldNot_ClearCoverageOfUnrelatedDocuments()
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
            _sut.CalculateForDocument("CurrentProject", "test.xml", string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(0));
        }

        [Test]
        public void CalculateForAllDocuments_ShouldNot_ClearCoverageOfUnrelatedDocuments()
        {
            // arrange
            var testLineCoverage = new LineCoverage();
            testLineCoverage.NodePath = "CurrentProject.MathHelperTests";
            testLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var codeLineCoverage = new LineCoverage();
            codeLineCoverage.NodePath = "CurrentProject.MathHelper";
            codeLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add("doc1.xml", new List<LineCoverage>() { testLineCoverage, codeLineCoverage });
            _solutionCoverageEngineMock.CalculateForAllDocuments().
                Throws(new TestCoverageCompilationException(new string[0]));

            // act
            _sut.CalculateForAllDocuments();

            // assert
            Assert.That(_sut.SolutionCoverageByDocument.Count, Is.EqualTo(0));
        }

        [Test]
        public void CalculateForDocument_ShouldClearAllCoverage_When_CompilationExceptionIsThrown()
        {
            // arrange
            const string documentPath = "EmployeeRepository.cs";
            var lineCoverage = new LineCoverage();
            lineCoverage.TestPath = lineCoverage.NodePath = "CurrentProject.MathHelperTests";

            _sut.SolutionCoverageByDocument.Add(documentPath, new List<LineCoverage>() { lineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new LineCoverage[0]));
            // act
            _sut.CalculateForDocument("CurrentProject", documentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[documentPath].Count, Is.EqualTo(1));
        }
    }
}