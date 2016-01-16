using System;
using System.IO;

namespace TestCoverage
{
    public static class PathHelper
    {
        public static string GetCoverageDllName(string assemblyName)
        {
            if (assemblyName.EndsWith("_COVERAGE.dll"))
                return assemblyName;

            return $"{assemblyName}_{"COVERAGE"}.dll";
        }

        public static bool AreEqual(string path1, string path2)
        {
            return NormalizePath(path1) == NormalizePath(path2);
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }
    }
}