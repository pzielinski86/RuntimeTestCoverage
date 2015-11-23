using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Linq;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestExecutorScriptEngine : MarshalByRefObject,ITestExecutorScriptEngine
    {
        public ITestRunResult RunTest(string[] references,
            string code)
        {

            // todo: clean-up code to remove hardcoded dlls like mscorlib.
            var options = new ScriptOptions();
            options = options.
                AddReferences(references.Where(x => !x.Contains("mscorlib.dll"))).
                AddReferences(typeof(int).Assembly).
                AddNamespaces("System", "System.Reflection");

            ScriptState state = null;

            try
            {
                state = CSharpScript.Run(code, options);
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

    }
}