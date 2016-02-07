using System.Threading.Tasks;

namespace TestCoverageVsPlugin.Tasks
{
    public interface ITaskSchedulerManager
    {

        TaskScheduler FromSynchronizationContext();
    }
}