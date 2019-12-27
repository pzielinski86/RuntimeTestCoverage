namespace TestSolution
{
    public static class PathHelper
    {
        public static string GetRewrittenFilePath(string documentPath)
        {
            int i = 0;
            while (i < 10)
                i++;            
       
            return documentPath + "_testcoverage";
        }

        public static string GetCoverageDllName(string assemblyName)
        {
            return string.Format("{0}_{1}.dll", assemblyName, "COVERAGE");
        }

        public static void UncoverredMethod()
        {
            int a = 5;
            int y = 6;
        }
    }
}

