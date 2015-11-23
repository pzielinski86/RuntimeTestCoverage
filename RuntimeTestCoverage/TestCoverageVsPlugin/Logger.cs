using Microsoft.VisualStudio.Shell.Interop;

namespace TestCoverageVsPlugin
{
    class Logger : ILogger
    {
        private readonly System.IServiceProvider _serviceProvider;

        public Logger(System.IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;            
        }

        public void Write(string message)
        {
            var logger = _serviceProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;

            logger.LogEntry((uint) __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, "RuntimeTestCoverage", message);
        }
    }
}