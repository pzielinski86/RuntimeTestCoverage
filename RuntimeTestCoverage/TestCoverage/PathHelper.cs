
using System;

namespace TestCoverage
{
    public static class PathHelper
    {
        public static string GetRewrittenFilePath(string documentPath)
        {
            return documentPath + ".coverage";
        }

        public static string GetCoverageDllName(string assemblyName)
        {
            return string.Format("{0}_{1}.dll", assemblyName, "COVERAGE");
        }
    }
}