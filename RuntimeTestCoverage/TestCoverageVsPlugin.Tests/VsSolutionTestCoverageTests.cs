using System.Collections.Generic;
using System.Windows.Media.Media3D;
using NSubstitute;
using NUnit.Framework;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    public class VsSolutionTestCoverageTests
    {
        private VsSolutionTestCoverage _sut;
        private ISolutionCoverageEngine _solutionCoverageEngineMock;
        private ISolutionExplorer _solutionExplorerMock;
        private const string SolutionPath = "c:\\1.sln";
            
        [SetUp]
        public void Setup()
        {
            _solutionCoverageEngineMock = Substitute.For<ISolutionCoverageEngine>();
            _solutionExplorerMock = Substitute.For<ISolutionExplorer>();

            _sut =new VsSolutionTestCoverage(_solutionExplorerMock, ()=> _solutionCoverageEngineMock);
        }

        [Test]
        public void CalculateForDocument_Should_ClearCoverageForCurrentDocument()
        {
            // arrange
            const string documentPath = "MathHelperTests.cs";
            var lineCoverage=new LineCoverage();
            lineCoverage.TestPath = lineCoverage.Path = "CurrentProject.MathHelperTests";

            _solutionExplorerMock.GetProjectNameByDocument(Arg.Any<string>()).Returns("CurrentProject");

            _sut.SolutionCoverageByDocument.Add(documentPath,new List<LineCoverage>() { lineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new LineCoverage[0]));
            // act
            _sut.CalculateForDocument(documentPath,string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[documentPath].Count, Is.EqualTo(0));
        }

        [Test]
        public void CalculateForDocument_Should_ClearCoverageOfCodeCoveredByTest()
        {
            // arrange
            const string documentPath = "MathHelperTests.cs";

            var testLineCoverage = new LineCoverage();
            testLineCoverage.Path = "CurrentProject.MathHelperTests";
            testLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            var codeLineCoverage = new LineCoverage();
            codeLineCoverage.Path = "CurrentProject.MathHelper";
            codeLineCoverage.TestPath = "CurrentProject.MathHelperTests";

            _solutionExplorerMock.GetProjectNameByDocument(Arg.Any<string>()).Returns("CurrentProject");

            _sut.SolutionCoverageByDocument.Add(documentPath, new List<LineCoverage>() { testLineCoverage,codeLineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new LineCoverage[0]));
            // act
            _sut.CalculateForDocument(documentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[documentPath].Count, Is.EqualTo(0));
        }

        [Test]
        public void CalculateForDocument_Should_PopulateCoverageWithNewData_When_NewDataIsAvailable()
        {
            // arrange
            const string newDocumentPath = "MathHelper.cs";

            _solutionExplorerMock.GetProjectNameByDocument(Arg.Any<string>()).Returns("CurrentProject");

            var coverage = new LineCoverage {DocumentPath = newDocumentPath};

            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new[] { coverage }));

            // act
            _sut.CalculateForDocument("MathHelperTests.cs", string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath].Count, Is.EqualTo(1));
            Assert.That(_sut.SolutionCoverageByDocument[newDocumentPath][0], Is.EqualTo(coverage));
        }

        [Test]
        public void CalculateForDocument_ShouldNot_ClearCoverageOfUnrelatedDocuments()
        {
            // arrange
            const string documentPath = "EmployeeRepository.cs";
            var lineCoverage = new LineCoverage();
            lineCoverage.TestPath = lineCoverage.Path = "CurrentProject.MathHelperTests";

            _solutionExplorerMock.GetProjectNameByDocument(Arg.Any<string>()).Returns("CurrentProject");

            _sut.SolutionCoverageByDocument.Add(documentPath, new List<LineCoverage>() { lineCoverage });
            _solutionCoverageEngineMock.CalculateForDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).
                Returns(new CoverageResult(new LineCoverage[0]));
            // act
            _sut.CalculateForDocument(documentPath, string.Empty);

            // assert
            Assert.That(_sut.SolutionCoverageByDocument[documentPath].Count, Is.EqualTo(1));
        }
    }
}