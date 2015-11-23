using System;
using System.IO;

namespace TestCoverage
{
    public static class Config
    {
        private static string _workingDirectory;

        public static string WorkingDirectory
        {
            get
            {
                if(_workingDirectory==null)
                    throw new ArgumentException("Working directory has not been set yet.");

                return _workingDirectory;
            }
        }

        public static void SetSolution(string solutionPath)
        {
            _workingDirectory = Path.Combine(Path.GetDirectoryName(solutionPath), ".Coverage");
            Directory.CreateDirectory(_workingDirectory);
        }
    }
}
