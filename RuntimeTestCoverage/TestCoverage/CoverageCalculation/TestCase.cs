using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestCase
    {
        public TestCase(TestFixtureDetails testFixture)
        {
            TestFixture = testFixture;
            Arguments=new string[0];
        }

        public TestFixtureDetails TestFixture { get; }

        public string[] Arguments { get; set; }
        public string MethodName { get; set; }

        public MethodDeclarationSyntax SyntaxNode { get; set; }
        public bool IsAsync { get; set; }

        public string CreateCallTestCode(string instanceName)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"{TestFixture.ClassScriptTypeName}.GetMethod(\"{MethodName}\"," +
                                 $"BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)." +
                                 $"Invoke({instanceName}, new object[]{{");

            for (int i = 0; i < Arguments.Length; i++)
            {         
                if (Arguments[i] == null)
                    stringBuilder.Append("null");
                else
                    stringBuilder.Append(Arguments[i]);

                if (i != Arguments.Length - 1)
                    stringBuilder.Append(", ");
            }

            stringBuilder.Append("})");

            if (IsAsync)
                return $"((dynamic){stringBuilder}).Wait();";

            return $"{stringBuilder};";
        }

        public string CreateRunTestScript()
        {
            StringBuilder scriptBuilder = new StringBuilder(); 

            ClearAudit(scriptBuilder);            
            scriptBuilder.AppendLine("string errorMessage=null;");
            scriptBuilder.AppendLine("bool ThrownException=false;");

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(TestFixture.CreateSetupFixtureCode("testFixture"));
            scriptBuilder.AppendLine(CreateCallTestCode("testFixture"));

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch(AggregateException e)" +
                                     "{" +
                                     "ThrownException=true; " +
                                     "errorMessage=e.InnerException.ToString();" +
                                     "}");
            scriptBuilder.AppendLine("catch(TargetInvocationException e)" +
                                     "{" +
                                     "ThrownException=true; " +
                                     "errorMessage=e.InnerException.ToString();" +
                                     "}");

            StoreAudit(scriptBuilder);

            return scriptBuilder.ToString();
        }


        private static void StoreAudit(StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                AuditVariablesMap.AuditVariablesListClassName,
                AuditVariablesMap.AuditVariablesListName));
        }

        private static void ClearAudit(StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("{0}.{1}.Clear();",
                AuditVariablesMap.AuditVariablesListClassName,
                AuditVariablesMap.AuditVariablesListName));
        }
    }
}