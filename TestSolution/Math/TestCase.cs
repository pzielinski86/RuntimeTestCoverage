using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Math
{
    public class TestCase
    {
        public TestCase(TestFixtureDetails testFixture)
        {
            TestFixture = testFixture;
        }

        public TestFixtureDetails TestFixture { get; }

        public object[] Arguments { get; set; }
        public string MethodName { get; set; }
        public MethodDeclarationSyntax SyntaxNode { get; set; }

        public string CreateCallTestCode(string instanceName)
        {
            return null;
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendFormat("{0}.{1}(", instanceName, MethodName);

            for (int i = 0; i < Arguments.Length; i++)
            {
                if (Arguments[i] is string)
                    stringBuilder.AppendFormat("\"{0}\"", Arguments[i]);
                else if (Arguments[i] is bool)
                    stringBuilder.Append("test");
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
