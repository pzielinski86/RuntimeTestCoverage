using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math
{
    public static class PathHelper
    {
        public static string GetRewrittenFilePath(string documentPath)
        {
            return documentPath + "_testcoverage1";
        }

        public static string GetCoverageDllName(string assemblyName)
        {
            return string.Format("{0}_{1}.dll", assemblyName, "COVERAGE");
        }
    }
}
