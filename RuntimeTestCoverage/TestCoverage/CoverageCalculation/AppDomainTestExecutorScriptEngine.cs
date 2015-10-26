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
        public TestRunResult RunTest(MetadataReference[] references,
            Assembly[] assemblies,
            TestCase testCase,
            AuditVariablesMap auditVariablesMap)
        {
            string script = CreateRunTestScript(testCase, auditVariablesMap);

            // todo: clean-up code to remove hardcoded dlls like mscorlib.
            var options = new ScriptOptions();
            options = options.
                AddReferences(references.Where(x=>!x.Display.Contains("mscorlib.dll"))).
                AddReferences(assemblies).
                AddReferences(typeof(int).Assembly).
                AddNamespaces( "System", "System.Reflection").
                AddNamespaces(testCase.TestFixture.Namespace);

            ScriptState state;

            try
            {
                state = CSharpScript.Run(script, options);
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x => x.GetMessage()).ToArray());
            }

            var coverageAudit = (List<string>)state.Variables["auditLog"].Value;
            string errorMessage = (string)state.Variables["errorMessage"].Value;
            bool assertionFailed = (bool)state.Variables["assertionFailed"].Value;

            return new TestRunResult(coverageAudit.ToArray(), assertionFailed, errorMessage);
        }

        private static string CreateRunTestScript(TestCase testCase, AuditVariablesMap auditVariablesMap)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            ClearAudit(auditVariablesMap, scriptBuilder);
            scriptBuilder.AppendLine(testCase.TestFixture.CreateSetupFixtureCode("testFixture"));
            scriptBuilder.AppendLine("string errorMessage=null;");
            scriptBuilder.AppendLine("bool assertionFailed=false;");

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(testCase.CreateCallTestCode("testFixture"));

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch(TargetInvocationException e)" +
                                     "{" +
                                     "if(e.InnerException is NUnit.Framework.AssertionException)" +
                                     "assertionFailed=true; " +
                                     "else " +
                                     "errorMessage=e.Message;" +
                                     "}");

            StoreAudit(auditVariablesMap, scriptBuilder);

            return scriptBuilder.ToString();
        }

        private static void StoreAudit(AuditVariablesMap auditVariablesMap, StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("\nvar auditLog= {0}.{1};",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesListName));
        }

        private static void ClearAudit(AuditVariablesMap auditVariablesMap, StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("{0}.{1}.Clear();",
                auditVariablesMap.AuditVariablesClassName,
                auditVariablesMap.AuditVariablesListName));
        }
    }
}