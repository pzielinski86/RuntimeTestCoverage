using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TestCoverage.CoverageCalculation
{
    public class TestCase
    {
        public object[] Arguments { get; set; }
        public string MethodName { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public MethodDeclarationSyntax SyntaxNode { get; set; }

        public string CreateCallTestCode(string instanceName)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("{0}.{1}(", instanceName, MethodName);

            for (int i = 0; i < Arguments.Length; i++)
            {
                if (Arguments[i] is string)
                    stringBuilder.AppendFormat("\"{0}\"", Arguments[i]);
                else if (Arguments[i] is bool)
                    stringBuilder.Append(Arguments[i].ToString().ToLower());
                else if (Arguments[i] == null)
                    stringBuilder.Append("null");
                else
                    stringBuilder.Append(Arguments[i]);

                if (i != Arguments.Length - 1)
                    stringBuilder.Append(", ");
            }

            stringBuilder.Append(");");

            return stringBuilder.ToString();
        }
    }
}