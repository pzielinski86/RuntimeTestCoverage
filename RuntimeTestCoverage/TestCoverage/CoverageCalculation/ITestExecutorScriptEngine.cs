using System.Threading.Tasks;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        ITestRunResult RunTest(string[] references, TestExecutionScriptParameters testExecutionScriptParameters);
        ITestRunResult[] RunTestFixture(string[] references, TestFixtureExecutionScriptParameters pars);
    }
}