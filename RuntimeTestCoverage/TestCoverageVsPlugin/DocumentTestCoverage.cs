using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;

namespace TestCoverageVsPlugin
{
    public class DocumentTestCoverage
    {
        private readonly string _solutionPath;
        private RewriteResult _allDDocumentsRewrite;

        public DocumentTestCoverage(string solutionPath)
        {
            _solutionPath = solutionPath;

        }

        public int[] CalculateForAllDocuments()
        {
            var domain = AppDomain.CreateDomain("coverage");
            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof(LineCoverageEngine).FullName);

            var positions= engine.CalculateForAllDocuments(_solutionPath);

            AppDomain.Unload(domain);

            return positions;
        }

        public int[] CalculateForDocument(string documentName, string documentContent, int selectedPosition)
        {
        
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();
            
            string selectedClassName = GetSelectedClass(syntaxNode, selectedPosition);
            string methodName = GetSelectedMethod(syntaxNode, selectedPosition);

            var domain = AppDomain.CreateDomain("coverage");
            
            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof(LineCoverageEngine).FullName);

            int[] coverage = engine.CalculateForTest(_solutionPath, documentName, selectedClassName, methodName);

            AppDomain.Unload(domain);
            return coverage;
        }

        private void Domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            
        }

        private string GetSelectedMethod(SyntaxNode syntaxNode, int selectedPosition)
        {
            var method = syntaxNode.DescendantNodes().
                 OfType<MethodDeclarationSyntax>().Reverse().
                 First(d => d.SpanStart <= selectedPosition);

            return method.Identifier.Text;
        }

        public string GetSelectedClass(SyntaxNode syntaxNode, int selectedPosition)
        {
            ClassDeclarationSyntax selectedClass = syntaxNode.DescendantNodes().
                OfType<ClassDeclarationSyntax>().Reverse().
                First(d => d.SpanStart <= selectedPosition);

            return selectedClass.Identifier.Text;
        }
    }
}