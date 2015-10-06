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
    public class AppDomainTestExecutorScriptEngine : MarshalByRefObject, ITestExecutorScriptEngine
    {
        public TestRunResult RunTest(MetadataReference[] references, Assembly[] assemblies, TestCase testCase, AuditVariablesMap auditVariablesMap)
        {                        
            string script = CreateRunTestScript(testCase.ClassName, testCase.MethodName, auditVariablesMap);

            ScriptOptions options = new ScriptOptions();

            options = options.AddReferences(references).AddReferences(assemblies).AddNamespaces(testCase.Namespace);

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
            bool assertionFailed = (bool)state.Variables["assertionFailed"].Value;
            string errorMessage = (string) state.Variables["errorMessage"].Value;           

            return new TestRunResult(coverageAudit.Keys.ToArray(),!assertionFailed, errorMessage);
        }

        private static string CreateRunTestScript(string className, string methodName, AuditVariablesMap auditVariablesMap)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(string.Format("dynamic testFixture = new {0}();", className));
            scriptBuilder.AppendLine("bool assertionFailed=false;");
            scriptBuilder.AppendLine("string errorMessage=null;");

            scriptBuilder.Append("try\n{\n");

            ClearAudit(auditVariablesMap, scriptBuilder);
            CallTest(scriptBuilder, methodName);

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch(NUnit.Framework.AssertionException e){assertionFailed=true;}");
            scriptBuilder.AppendLine("catch(System.Exception e){errorMessage=e.Message;}");

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