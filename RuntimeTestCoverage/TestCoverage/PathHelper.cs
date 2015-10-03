
using System;

namespace TestCoverage
{
    public static class PathHelper
    {
        public static string GetRewrittenFilePath(string documentPath)
        {
            return documentPath + "_testcoverage";
        }

        public static string GetCoverageDllName(string assemblyName)
        {
            return string.Format("{0}_{1}.dll", assemblyName, "COVERAGE");
        }
    }
}