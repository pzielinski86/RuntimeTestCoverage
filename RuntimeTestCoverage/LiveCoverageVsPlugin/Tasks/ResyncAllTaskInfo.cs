using System.Threading.Tasks;
using TestCoverage.Tasks;

namespace LiveCoverageVsPlugin.Tasks
{
    public class ResyncAllTaskInfo:ITaskCoverageInfo
    {       
        public Task<bool> ExecuteAsync(ITaskCoverageManager taskCoverageManager, IVsSolutionTestCoverage vsSolutionTestCoverage,
            IDocumentProvider documentProvider)
        {
            taskCoverageManager.RaiseEvent(new ResyncAllStarted());
            var task = vsSolutionTestCoverage.CalculateForAllDocumentsAsync();

            var finalTask = task.ContinueWith((finishedTask, y) =>
            {              
                taskCoverageManager.RaiseEvent(new ResyncAllCompleted());

                return finishedTask.Result;
            }, null, TaskSchedulerManager.Current.FromSynchronizationContext());

            return finalTask;
        }
    }
}