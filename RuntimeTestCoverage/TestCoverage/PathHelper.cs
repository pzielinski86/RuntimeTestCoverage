
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
            if (assemblyName.EndsWith("_COVERAGE.dll"))
                return assemblyName;

            return $"{assemblyName}_{"COVERAGE"}.dll";
        }
    }
}