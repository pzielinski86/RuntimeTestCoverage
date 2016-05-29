using System.Threading.Tasks;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskCoverageInfo
    {
        string DocumentPath { get; }
        Task<bool> ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage, IDocumentProvider documentProvider);
    }
}