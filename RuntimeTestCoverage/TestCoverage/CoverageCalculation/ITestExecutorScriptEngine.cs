using System.Threading.Tasks;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        ITestRunResult[] RunTestFixture(string[] references, TestFixtureExecutionScriptParameters pars);
    }
}