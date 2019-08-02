using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PerformanceMetricsParser
{
    class Program
    {
        private static readonly string MetricsPath = @"C:\Users\conta\AppData\Local\Microsoft\VisualStudio\16.0_1dff26d6Exp\Extensions\Piotr Zielinski\LiveCoverageVsPlugin\1.0\logs\Performance.txt";
        private static readonly Dictionary<string, List<int>> MetricsData = new Dictionary<string, List<int>>();

        static void Main(string[] args)
        {
            ExtractMetricsData();

            foreach (string metricName in MetricsData.Keys)
            {
                double averageTime = MetricsData[metricName].Average();
                int minTime = MetricsData[metricName].Min();
                int maxTime = MetricsData[metricName].Max();

                Console.WriteLine("{0, -70} - Min: {1} ms, Avg: {2} ms, Max: {3} ms", metricName, minTime, averageTime, maxTime);
            }
        }

        private static void ExtractMetricsData()
        {
            using (StreamReader stream = new StreamReader(File.OpenRead(MetricsPath)))
            {
                while (!stream.EndOfStream)
                {
                    string performanceEntryLine = stream.ReadLine();
                    parseLine(performanceEntryLine);
                }
            }
        }

        private static void parseLine(string performanceEntryLine)
        {
            string[] performanceEntryPartsBySpace = performanceEntryLine.Split(' ');

            string metricName = performanceEntryPartsBySpace[2];
            int timeInMs = int.Parse(performanceEntryPartsBySpace[3]);

            if (MetricsData.ContainsKey(metricName))
            {
                MetricsData[metricName].Add(timeInMs);
            }
            else
            {
                MetricsData.Add(metricName, new List<int>() { timeInMs });
            }
        }
    }
}
