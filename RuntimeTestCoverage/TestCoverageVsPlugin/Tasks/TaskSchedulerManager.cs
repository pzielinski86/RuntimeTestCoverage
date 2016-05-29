namespace TestCoverageVsPlugin.Tasks
{
    public static class TaskSchedulerManager
    {
        static TaskSchedulerManager()
        {
            Current = new TplTaskSchedulerManager();
        }

        public static ITaskSchedulerManager Current { get; set; }
    }
}