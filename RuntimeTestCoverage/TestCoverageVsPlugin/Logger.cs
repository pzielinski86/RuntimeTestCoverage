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

        public void Error(string message)
        {
            Log(message, __ACTIVITYLOG_ENTRYTYPE.ALE_ERROR);
        }

        public void Info(string message)
        {            
            Log(message, __ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION);
        }

        private void Log(string message, __ACTIVITYLOG_ENTRYTYPE msgType)
        {
            var logger = _serviceProvider.GetService(typeof(SVsActivityLog)) as IVsActivityLog;

            logger.LogEntry((uint)msgType, "RuntimeTestCoverage", message);
        }
    }
}