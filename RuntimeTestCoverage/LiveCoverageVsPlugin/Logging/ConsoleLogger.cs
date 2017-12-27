using System;

namespace LiveCoverageVsPlugin.Logging
{
    class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            Console.WriteLine("Error: {0}",message);
        }

        public void Info(string message)
        {
            Console.WriteLine("Info: {0}", message);
        }
    }
}