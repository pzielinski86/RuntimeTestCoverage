
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
            return string.Format("{0}_testcoverage.dll", assemblyName);
        }
        public static string GetDllNameFromCoverageDll(string coverageDllName)
        {
            for (int i = coverageDllName.Length - 1; i >= 0; i--)
            {
                if (coverageDllName[i] == '_')
                {
                    return coverageDllName.Substring(0, i);
                }
            }

            return coverageDllName;
        }
    }
}