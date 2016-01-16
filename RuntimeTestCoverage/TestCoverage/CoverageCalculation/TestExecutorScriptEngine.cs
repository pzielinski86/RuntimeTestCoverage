using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestExecutorScriptEngine : MarshalByRefObject,ITestExecutorScriptEngine
    {
        public async Task<ITestRunResult> RunTestAsync(string[] references,
            string code)
        {

            // todo: clean-up code to remove hardcoded dlls like mscorlib.
            var options = ScriptOptions.Default.
                AddReferences(references.Where(x => !x.Contains("mscorlib.dll"))).
                AddReferences(typeof(int).Assembly).
                AddImports("System", "System.Reflection");

            ScriptState state = null;

            try
            {
                state = await CSharpScript.RunAsync(code, options);
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x => x.GetMessage()).ToArray());
            }

            var coverageAudit = (dynamic)state.GetVariable("auditLog").Value;
            string errorMessage = (string)state.GetVariable("errorMessage").Value;
            bool assertionFailed = (bool)state.GetVariable("assertionFailed").Value;

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