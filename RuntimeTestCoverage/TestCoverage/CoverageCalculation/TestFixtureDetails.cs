using System.Collections.Generic;
using System.Text;

namespace TestCoverage.CoverageCalculation
{
    public class TestFixtureDetails
    {
        public TestFixtureDetails()
        {
            Cases = new List<TestCase>();
        }
        public List<TestCase> Cases { get; set; }
        public string AssemblyName { get; set; }
        public string TestSetUpMethodName { get; set; }
        public string TestTearDownMethodName { get; set; }
        public string TestFixtureSetUpMethodName { get; set; }
        public string TestFixtureTearDownMethodName { get; set; }
        public string FullClassName { get; set; }
    }
}