using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveCoverageVsPlugin.Performance
{
    class Benchmark
    {
        private const string CATEGORY_NAME = "LiveCoverageVsPlugin";
        private const string CALCULATE_ALL_COUNTER_NAME = "LiveCoverageVsPlugin.Performance.CalculateAllDocuments";

        private Dictionary<PerformanceMetric, PerformanceCounter> performanceCounters = new Dictionary<PerformanceMetric, PerformanceCounter>();

        public Benchmark()
        {
            InitCounters()
        }

        public void Increment(PerformanceMetric metric, long value)
        {
            performanceCounters[metric].Increment(value);
        }
        private void InitCounters(string counterName)
        {
            if (!PerformanceCounterCategory.Exists(CATEGORY_NAME))
            {
                string categoryHelp = "LiveCoverageVsPlugin related real time statistics";

                PerformanceCounterCategory.Create(CATEGORY_NAME, null);
                PerformanceCounterCategory customCategory = new PerformanceCounterCategory(CATEGORY_NAME);
            }

            PerformanceCounterCategory.Create(CATEGORY_NAME, null, PerformanceCounterCategoryType.SingleInstance, CALCULATE_ALL_COUNTER_NAME, null);
        }
    }
}
