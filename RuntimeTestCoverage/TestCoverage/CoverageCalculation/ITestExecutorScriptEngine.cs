using System.Threading.Tasks;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        ITestRunResult RunTest(string[] references, string code);
    }
}