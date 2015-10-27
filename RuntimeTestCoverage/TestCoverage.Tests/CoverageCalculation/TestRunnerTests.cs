using NSubstitute;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;

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

    }
}