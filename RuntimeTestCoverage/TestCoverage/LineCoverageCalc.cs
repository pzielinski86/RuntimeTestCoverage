using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;

namespace TestCoverage
{

    public class LineCoverageCalc
    {
        public int[] CalculateForAllTests(string solutionPath, RewriteResult rewriteResult)
        {
            SyntaxTree auditVariablesTree = CSharpSyntaxTree.ParseText(rewriteResult.AuditVariablesMap.ToString());
            SyntaxTree[] allTrees = rewriteResult.Items.Select(i => i.SyntaxTree).ToArray();
            SyntaxNode[] testClasses = GetTestClasses(rewriteResult);

            CSharpCompilation compilation = Compile(allTrees, auditVariablesTree, GetAllReferences(solutionPath));
            Assembly assembly = SaveTestCoverageDll(compilation);

            foreach (SyntaxNode testClass in testClasses)
            {
                return RunAllTests(testClass, compilation, assembly, rewriteResult.AuditVariablesMap);
            }

            return new int[0];
        }

        public int[] CalculateForTest(RewriteResult rewriteResult,string solutionPath, string documentName, string className, string methodName)
        {
            SyntaxTree auditVariablesTree = CSharpSyntaxTree.ParseText(rewriteResult.AuditVariablesMap.ToString());
            SyntaxTree[] allTrees = rewriteResult.Items.Select(i => i.SyntaxTree).ToArray();
            var rewrittenItem = rewriteResult.Items.Single(i => i.Document.Name == documentName);

            ClassDeclarationSyntax classNode = rewrittenItem.SyntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);

            MethodDeclarationSyntax methodNode =
                classNode.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(d => d.Identifier.Text == methodName);

            CSharpCompilation compilation = Compile(allTrees, auditVariablesTree, GetAllReferences(solutionPath));
            Assembly assembly = SaveTestCoverageDll(compilation);


            var testExecutorScriptEngine=new TestExecutorScriptEngine();
            Dictionary<string, bool> setVariables = testExecutorScriptEngine.RunTest(compilation, assembly, className, methodNode, rewriteResult.AuditVariablesMap);

            return setVariables.Select(x => rewriteResult.AuditVariablesMap.Map[x.Key]).ToArray();
        }

        private static MetadataReference[] GetAllReferences(string solutionPath)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solutionToAnalyze = workspace.OpenSolutionAsync(solutionPath).Result;
            var allReferences = solutionToAnalyze.Projects.SelectMany(p => p.MetadataReferences).Distinct().ToArray();

            return allReferences;
        }
        private static int[] RunAllTests(SyntaxNode testClass, Compilation compilation, Assembly assembly, AuditVariablesMap auditVariablesMap)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var testExecutorScriptEngine = new TestExecutorScriptEngine();

            List<int> variables = new List<int>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = testExecutorScriptEngine.RunTest(compilation, assembly, className, testMethod, auditVariablesMap);

                variables.AddRange(setVariables.Select(x => auditVariablesMap.Map[x.Key]).ToArray());
            }

            return variables.ToArray();
        }

        private static SyntaxNode[] GetTestMethods(SyntaxNode testClass)
        {
            SyntaxNode[] testMethods = testClass.DescendantNodes().SelectMany(
                t =>
                    t.DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .Where(a => a.Name.ToString() == "Test")
                        .Select(a => a.Parent.Parent)).ToArray();

            return testMethods;
        }

        private static Assembly SaveTestCoverageDll(CSharpCompilation compilation)
        {
            using (var stream = new FileStream("TestCoverageCalculation.dll", FileMode.Create))
            {
                EmitResult emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    throw new TestCoverageCompilationException(
                        emitResult.Diagnostics.Select(d => d.GetMessage()).ToArray());
                }
            }

            return Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), "TestCoverageCalculation.dll"));
        }

        private static CSharpCompilation Compile(SyntaxTree[] allTrees, SyntaxTree auditVariablesTree, MetadataReference[] references)
        {
            CSharpCompilation compilation = CSharpCompilation.Create(
                "TestCoverageCalculation", allTrees.Union(new[] { auditVariablesTree }),
               references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return compilation;
        }

        private static SyntaxNode[] GetTestClasses(RewriteResult rewriteResult)
        {
            IEnumerable<SyntaxNode> allNodes = rewriteResult.Items.Select(i => i.SyntaxTree.GetRoot());

            return allNodes.SelectMany(
                        t => t.DescendantNodes()
                                .OfType<AttributeSyntax>()
                                .Where(a => a.Name.ToString() == "TestFixture")
                                .Select(a => a.Parent.Parent)).ToArray();
        }
    }
}
