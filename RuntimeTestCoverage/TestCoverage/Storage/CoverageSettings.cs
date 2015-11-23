using System.Collections.Generic;

namespace TestCoverage.Storage
{
    public class CoverageSettings
    {
        public CoverageSettings()
        {
            Projects=new List<TestProjectSettings>();
        }
        public List<TestProjectSettings> Projects { get; set; }
    }
}
