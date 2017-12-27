using System.Threading.Tasks;

namespace LiveCoverageVsPlugin.Tasks
{
    public class TplTaskSchedulerManager : ITaskSchedulerManager
    {
        public TaskScheduler FromSynchronizationContext()
        {
            return TaskScheduler.FromCurrentSynchronizationContext();
        }
    }
}