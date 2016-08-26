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
        public string ClassScriptTypeName => "testFixtureType";
        public string FullClassName { get; set; }
        public string FullQualifiedName => $"{FullClassName},{AssemblyName}";

        public string CreateSetupFixtureCode(string instanceName)
        {
            var codeBuilder = new StringBuilder();

            codeBuilder.AppendLine($"Type {ClassScriptTypeName} = Type.GetType(\"{FullClassName},{AssemblyName}\");");
            codeBuilder.AppendLine($"object {instanceName} = System.Activator.CreateInstance(testFixtureType);");

            if (TestSetUpMethodName != null)
                codeBuilder.AppendLine($"{ClassScriptTypeName}.GetMethod(\"{TestSetUpMethodName}\",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke({instanceName}, null);");

            return codeBuilder.ToString();
        }
    }
}