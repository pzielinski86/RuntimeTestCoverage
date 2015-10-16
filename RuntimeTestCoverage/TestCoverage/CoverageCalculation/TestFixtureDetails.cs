using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class TestFixtureDetails
    {
        public TestFixtureDetails()
        {
            Cases = new List<TestCase>();
        }
        public List<TestCase> Cases { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string SetupMethodName { get; set; }

        public string CreateSetupFixtureCode(string instanceName)
        {
            var codeBuilder = new StringBuilder();

            codeBuilder.AppendLine($"dynamic {instanceName} = new {ClassName}();");

            if (SetupMethodName != null)
                codeBuilder.AppendLine($"{instanceName}.{SetupMethodName}();");

            return codeBuilder.ToString();
        }
    }
}