using System;
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

            List<int> allPositions = new List<int>();

            foreach (SyntaxNode testClass in testClasses)
            {
                int[] positions = RunAllTests(testClass, compilation, rewriteResult.AuditVariablesMap);
                allPositions.AddRange(positions);
            }

            return allPositions.ToArray();
        }

        public int[] CalculateForTest(SyntaxTree[] allTrees, AuditVariablesMap auditVariablesMap, string solutionPath, string documentContent, string className, string methodName)
        {
            SyntaxTree auditVariablesTree = CSharpSyntaxTree.ParseText(auditVariablesMap.ToString());

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);

            ClassDeclarationSyntax classNode = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(d => d.Identifier.Text == className);

            MethodDeclarationSyntax methodNode =
                classNode.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Single(d => d.Identifier.Text == methodName);


            CSharpCompilation compilation = Compile(allTrees, auditVariablesTree, GetAllReferences(solutionPath));
            TestExecutorScriptEngine executor = new TestExecutorScriptEngine();

            Dictionary<string, bool> setVariables = executor.RunTest(compilation, className, methodNode, auditVariablesMap.AuditVariablesClassName, auditVariablesMap.AuditVariablesDictionaryName);

            return setVariables.Select(x => auditVariablesMap.Map[x.Key]).ToArray();
        }

        private static MetadataReference[] GetAllReferences(string solutionPath)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Solution solutionToAnalyze = workspace.OpenSolutionAsync(solutionPath).Result;
            var allReferences = solutionToAnalyze.Projects.SelectMany(p => p.MetadataReferences).Distinct().ToArray();

            return allReferences;
        }
        private static int[] RunAllTests(SyntaxNode testClass, Compilation compilation, AuditVariablesMap auditVariablesMap)
        {
            string className = testClass.ChildTokens().Single(t => t.Kind() == SyntaxKind.IdentifierToken).ValueText;
            SyntaxNode[] testMethods = GetTestMethods(testClass);

            var executor = new TestExecutorScriptEngine();

            List<int> variables = new List<int>();

            foreach (SyntaxNode testMethod in testMethods)
            {
                Dictionary<string, bool> setVariables = executor.RunTest(compilation, className, testMethod, auditVariablesMap.AuditVariablesClassName, auditVariablesMap.AuditVariablesDictionaryName);

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
