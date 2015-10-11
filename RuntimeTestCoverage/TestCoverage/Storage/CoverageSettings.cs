using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
