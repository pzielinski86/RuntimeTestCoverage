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
        public string SetupMethodName { get; set; }
        public string ClassScriptTypeName => "testFixtureType";
        public string FullClassName { get; set; }

        public string CreateSetupFixtureCode(string instanceName)
        {
            var codeBuilder = new StringBuilder();

            codeBuilder.AppendLine($"Type {ClassScriptTypeName} = Type.GetType(\"{FullClassName},{AssemblyName}\");");
            codeBuilder.AppendLine($"object {instanceName} = System.Activator.CreateInstance(testFixtureType);");

            if (SetupMethodName != null)
                codeBuilder.AppendLine($"{ClassScriptTypeName}.GetMethod(\"{SetupMethodName}\",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke({instanceName}, null);");

            return codeBuilder.ToString();
        }
    }
}