using System.Threading.Tasks;

namespace TestCoverage.CoverageCalculation
{
    public interface ITestExecutorScriptEngine
    {
        Task<ITestRunResult> RunTestAsync(string[] references, string code);
    }
}