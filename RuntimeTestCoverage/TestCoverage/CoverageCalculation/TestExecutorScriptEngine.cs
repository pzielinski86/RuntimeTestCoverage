using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    public class TestExecutorScriptEngine : MarshalByRefObject, ITestExecutorScriptEngine
    {
        private string[] _references = null;
        private Script<object> _runnerScript;

        public ITestRunResult RunTest(string[] references,
            TestExecutionScriptParameters testExecutionScriptParameters)
        {
            throw new NotImplementedException();
        }

        public ITestRunResult[] RunTestFixture(string[] references, TestFixtureExecutionScriptParameters pars)
        {
            if (_references == null || _references.Length != references.Length)
            {
                // todo: clean-up code to remove hardcoded dlls like mscorlib.
                var options = ScriptOptions.Default.
                    WithReferences(references.Where(x => !x.Contains("mscorlib.dll"))).
                    AddReferences(typeof(int).Assembly).
                    AddImports("System", "System.Reflection", "System.Linq");

                _references = references;

                string runnerScriptCode = Resources.TestRunnerScriptCode;
                _runnerScript = CSharpScript.Create(runnerScriptCode, options, typeof(TestFixtureExecutionScriptParameters));
            }

            ScriptState state = null;

            try
            {
                state = _runnerScript.RunAsync(pars).Result;
            }
            catch (CompilationErrorException e)
            {
                throw new TestCoverageCompilationException(e.Diagnostics.Select(x => x.GetMessage()).ToArray());
            }

            var output = (dynamic)state.GetVariable("output").Value;
            
            return GetResults(output);
        }

        private TestRunResult[] GetResults(dynamic output)
        {
            var results = new List<TestRunResult>(output.Count);

            foreach (var testResults in output)
            {
                string testName = testResults.TestName;                
                string errorMessage = testResults.ErrorMessage;
                var auditVariables = testResults.Variables;

                var result = new TestRunResult(testName, GetVariables(auditVariables), errorMessage);
                results.Add(result);
            }

            return results.ToArray();
        }

        private AuditVariablePlaceholder[] GetVariables(dynamic dynamicAuditVariables)
        {
            var variables = new AuditVariablePlaceholder[dynamicAuditVariables.Length];
            int i = 0;

            foreach (var dynamicVar in dynamicAuditVariables)
            {
                var value = dynamicVar;

                var variable = new AuditVariablePlaceholder(value.DocumentPath,
                    value.NodePath,
                    value.Span,
                    value.ExecutionCounter);

                variables[i++] = variable;
            }

            return variables;
        }
    }
}