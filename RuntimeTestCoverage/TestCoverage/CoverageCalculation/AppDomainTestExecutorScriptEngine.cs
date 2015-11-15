using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public ITestRunResult RunTest(MetadataReference[] references,
            _Assembly[] assemblies,
            TestCase testCase)
        {
            string script = CreateRunTestScript(testCase);

            // todo: clean-up code to remove hardcoded dlls like mscorlib.
            var options = new ScriptOptions();
            options = options.
                AddReferences(references.Where(x => !x.Display.Contains("mscorlib.dll"))).
                AddReferences(assemblies.OfType<Assembly>()).
                AddReferences(typeof (int).Assembly).
                AddNamespaces("System", "System.Reflection");

            ScriptState state = null;

            try
            {
                state = CSharpScript.Run(script, options);
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x => x.GetMessage()).ToArray());
            }

            var coverageAudit = (dynamic)state.Variables["auditLog"].Value;
            string errorMessage = (string)state.Variables["errorMessage"].Value;
            bool assertionFailed = (bool)state.Variables["assertionFailed"].Value;

            return new TestRunResult(GetVariables(coverageAudit), assertionFailed, errorMessage);
        }

        private AuditVariablePlaceholder[] GetVariables(dynamic dynamicVariables)
        {
            var variables = new AuditVariablePlaceholder[dynamicVariables.Count];

            for (int i = 0; i < dynamicVariables.Count; i++)
            {
                var variable = new AuditVariablePlaceholder(dynamicVariables[i].DocumentPath,
                    dynamicVariables[i].NodePath,
                    dynamicVariables[i].Span);

                variables[i] = variable;
            }

            return variables;
        }

        private static string CreateRunTestScript(TestCase testCase)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            ClearAudit(scriptBuilder);
            scriptBuilder.AppendLine(testCase.TestFixture.CreateSetupFixtureCode("testFixture"));
            scriptBuilder.AppendLine("string errorMessage=null;");
            scriptBuilder.AppendLine("bool assertionFailed=false;");

            scriptBuilder.Append("try\n{\n");
            scriptBuilder.AppendLine(testCase.CreateCallTestCode("testFixture"));

            scriptBuilder.AppendLine("}");
            scriptBuilder.AppendLine("catch(TargetInvocationException e)" +
                                     "{" +
                                     "assertionFailed=true; " +
                                     "errorMessage=e.ToString();" +
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

        private static void ClearAudit( StringBuilder scriptBuilder)
        {
            scriptBuilder.AppendLine(string.Format("{0}.{1}.Clear();",
                AuditVariablesMap.AuditVariablesListClassName,
                AuditVariablesMap.AuditVariablesListName));
        }
    }
}