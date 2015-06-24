using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;

namespace TestCoverage
{
    public class TestExecutorScriptEngine
    {
        public Dictionary<string, bool> RunTest(Compilation compilation, Assembly assembly, string className, SyntaxNode method, AuditVariablesMap auditVariablesMap)
        {
            string methodName = method.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;

            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(string.Format("var testFixture = new {0}();", className));

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(string.Format("testFixture.{0}();", methodName));
            scriptBuilder.AppendLine("}");

            scriptBuilder.AppendLine("catch{}");

            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesDictionaryName));

            ScriptOptions options = new ScriptOptions();
            options = options.AddReferences(compilation.References).AddReferences(assembly).AddNamespaces("Math.Tests");

            ScriptState state = CSharpScript.Run(scriptBuilder.ToString(), options);

            var coverageAudit = (Dictionary<string, bool>) state.Variables["auditLog"].Value;
            return coverageAudit;
        }
    }
}