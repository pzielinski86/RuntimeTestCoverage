using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class TestCase
    {
        public TestCase(TestFixtureDetails testFixture)
        {
            TestFixture = testFixture;
        }

        public TestFixtureDetails TestFixture { get; }

        public string[] Arguments { get; set; }
        public string MethodName { get; set; }        
        public MethodDeclarationSyntax SyntaxNode { get; set; }

        public string CreateCallTestCode(string instanceName)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"{TestFixture.ClassScriptTypeName}.GetMethod(\"{MethodName}\",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke({instanceName}, new object[]{{");

            for (int i = 0; i < Arguments.Length; i++)
            {         
                if (Arguments[i] == null)
                    stringBuilder.Append("null");
                else
                    stringBuilder.Append(Arguments[i]);

                if (i != Arguments.Length - 1)
                    stringBuilder.Append(", ");
            }

            stringBuilder.Append("});");

            return stringBuilder.ToString();
        }
    }
}