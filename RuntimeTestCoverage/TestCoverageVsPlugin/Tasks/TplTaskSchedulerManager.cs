using System.Threading.Tasks;

namespace TestCoverageVsPlugin.Tasks
{
    public class TplTaskSchedulerManager : ITaskSchedulerManager
    {
        public TaskScheduler FromSynchronizationContext()
        {
            return TaskScheduler.FromCurrentSynchronizationContext();
        }
    }
}