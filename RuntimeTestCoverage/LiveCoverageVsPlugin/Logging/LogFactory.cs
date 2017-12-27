namespace LiveCoverageVsPlugin.Logging
{
    public class LogFactory
    {
        private static ILogger _currentLogger;
        private static readonly object _Sync = new object();

        public static ILogger CurrentLogger
        {
            get { return _currentLogger ?? (_currentLogger = new ConsoleLogger()); }
            set
            {
                lock (_Sync)
                {
                    _currentLogger = value;
                }
            }
        }
    }
}