
namespace TestCoverage
{
    internal static class PathHelper
    {
        public static string GetRewrittenFilePath(string documentPath)
        {
            return documentPath + "_testcoverage";
        }

        public static string GetCoverageDllName(string assemblyName)
        {
            return string.Format("{0}_testcoverage.dll", assemblyName);
        }

    }
}