using LiveCoverageVsPlugin.Logging;
using log4net;
using System;
using System.Diagnostics;

namespace LiveCoverageVsPlugin.Performance
{
    public class Benchmark : IDisposable
    {
        private ILog logger;
        private Stopwatch stopwatch;
        private readonly string metricName;

        public Benchmark(string metricName)
        {
            logger = LogFactory.GetLogger("PERFORMANCE_LOGGER");
            Start();
            this.metricName = metricName;
        }

        public void Start()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public void Stop()
        {
            logger.Debug(string.Format("{0} {1}", metricName, stopwatch.ElapsedMilliseconds));
        }
 
        public void Dispose()
        {
            Stop();
        }

        public static void Profile(string metricName, Action method)
        {
            using (new Benchmark(metricName))
            {
                method();
            }
        }
    } 
}
