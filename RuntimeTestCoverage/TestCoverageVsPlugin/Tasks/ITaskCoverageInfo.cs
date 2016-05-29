using System.Threading.Tasks;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskCoverageInfo
    {
        string DocumentPath { get; }
        Task ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage, IDocumentProvider documentProvider);
    }
}