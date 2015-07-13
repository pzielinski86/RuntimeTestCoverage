using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    internal class TestExecutorScriptEngine : MarshalByRefObject
    {
        public Dictionary<string, bool> RunTest(MetadataReference[] references, Assembly[] assemblies, SyntaxNode method, AuditVariablesMap auditVariablesMap)
        {
            var classDeclarationSyntax = method.Ancestors().OfType<ClassDeclarationSyntax>().First();
            var namespaceDeclaration = classDeclarationSyntax.Ancestors().OfType<NamespaceDeclarationSyntax>().First();

            string className = classDeclarationSyntax.Identifier.Text;
            string @namespace = namespaceDeclaration.Name.ToString();
            string methodName = method.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;

            string script = CreateRunTestScript(className, methodName, auditVariablesMap);

            ScriptOptions options = new ScriptOptions();    

            options = options.AddReferences(references).AddReferences(assemblies).AddNamespaces(@namespace);

            ScriptState state;

            try
            {
                state = CSharpScript.Run(script, options);
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x => x.GetMessage()).ToArray());
            }

            var coverageAudit = (Dictionary<string, bool>)state.Variables["auditLog"].Value;
            return coverageAudit;
        }

        private static string CreateRunTestScript(string className, string methodName, AuditVariablesMap auditVariablesMap)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(string.Format("dynamic testFixture = new {0}();", className));

            scriptBuilder.Append("try\n{\n");

            ClearAudit(auditVariablesMap, scriptBuilder);
            CallTest(scriptBuilder, methodName);

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch{}");

            StoreAudit(auditVariablesMap, scriptBuilder);

            return scriptBuilder.ToString();
        }

        private static void StoreAudit(AuditVariablesMap auditVariablesMap, StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesDictionaryName));
        }

        private static void CallTest(StringBuilder scriptBuilder, string methodName)
        {
            scriptBuilder.AppendLine(string.Format("testFixture.{0}();", methodName));
        }

        private static void ClearAudit(AuditVariablesMap auditVariablesMap, StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("{0}.{1}.Clear();",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesDictionaryName));
        }
    }
}