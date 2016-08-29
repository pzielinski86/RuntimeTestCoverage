using System.Threading.Tasks;

namespace TestCoverage.Tasks
{
    public interface ITaskCoverageInfo
    {
        Task<bool> ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage, IDocumentProvider documentProvider);
    }
}