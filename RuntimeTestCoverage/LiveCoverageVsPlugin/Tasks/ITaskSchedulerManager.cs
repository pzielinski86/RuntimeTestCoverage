using System.Threading.Tasks;

namespace LiveCoverageVsPlugin.Tasks
{
    public interface ITaskSchedulerManager
    {

        TaskScheduler FromSynchronizationContext();
    }
}