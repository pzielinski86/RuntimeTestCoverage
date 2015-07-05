using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using Microsoft.CSharp;

namespace TestCoverage
{
    public class TestExecutorScriptEngine : MarshalByRefObject
    {      
        public Dictionary<string, bool> RunTest(MetadataReference[] references,Assembly[] assemblies, string className, SyntaxNode method, AuditVariablesMap auditVariablesMap)
        {            
            string methodName = method.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;

            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(string.Format("dynamic testFixture = new {0}();", className));

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(string.Format("testFixture.{0}();", methodName));
            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch{}");

            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesDictionaryName));

            ScriptOptions options = new ScriptOptions();
            options = options.AddReferences(references).AddReferences(assemblies).AddNamespaces("Math.Tests");

            ScriptState state;

            try
            {
                state = CSharpScript.Run(scriptBuilder.ToString(), options);
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x=>x.GetMessage()).ToArray());
            }

            var coverageAudit = (Dictionary<string, bool>) state.Variables["auditLog"].Value;
            return coverageAudit;
        }
    }
}