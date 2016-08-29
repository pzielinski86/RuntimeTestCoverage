using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;

namespace TestCoverageVsPlugin.Logging
{
    class VisualStudioLogger : ILogger
    {
        private readonly System.IServiceProvider _serviceProvider;

        public VisualStudioLogger(System.IServiceProvider serviceProvider)
        {
            if(serviceProvider==null)
                throw new ArgumentNullException();

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

            Debug.Assert(logger!=null);

            logger.LogEntry((uint)msgType, "RuntimeTestCoverage", message);
        }        
    }
}