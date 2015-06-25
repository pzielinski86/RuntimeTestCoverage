using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using TestCoverageSandbox;

namespace TestCoverage
{
    public class TestExecutorScriptEngine : MarshalByRefObject
    {      
        public Dictionary<string, bool> RunTest(Compilation compilation, string className, SyntaxNode method, string auditVariablesClassName, string auditVariablesDictionaryName)
        {
            var assembly=SaveTestCoverageDll(compilation);

            string methodName = method.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;

            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(string.Format("var testFixture = new {0}();", className));

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(string.Format("testFixture.{0}();", methodName));
            scriptBuilder.AppendLine("}");

            scriptBuilder.AppendLine("catch{}");

            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                auditVariablesClassName,
                auditVariablesDictionaryName));

            ScriptOptions options = new ScriptOptions();
            options = options.AddReferences(compilation.References).AddReferences(assembly).AddNamespaces("Math.Tests");

            ScriptState state = CSharpScript.Run(scriptBuilder.ToString(), options);

            var coverageAudit = (Dictionary<string, bool>) state.Variables["auditLog"].Value;
            return coverageAudit;
        }


        private static Assembly SaveTestCoverageDll(Compilation compilation)
        {
            string dllName = Guid.NewGuid().ToString();
            using (var stream = new FileStream(dllName, FileMode.Create))
            {
                EmitResult emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    throw new TestCoverageCompilationException(
                        emitResult.Diagnostics.Select(d => d.GetMessage()).ToArray());
                }
            }

            return Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), dllName));
        }

    }
}