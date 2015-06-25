﻿using System.Linq;
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
            var rewritter = new SolutionRewritter();
            _allDDocumentsRewrite = rewritter.RewriteAllClasses(_solutionPath);

            var lineCoverageCalc = new LineCoverageCalc();
            return lineCoverageCalc.CalculateForAllTests(_solutionPath, _allDDocumentsRewrite);
        }

        public int[] CalculateForDocument(string documentName, string documentContent, int selectedPosition)
        {
            if (_allDDocumentsRewrite == null)
                return CalculateForAllDocuments();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            var rewritter = new SolutionRewritter();
            rewritter.RewriteTestClass(_allDDocumentsRewrite, documentName);

            var lineCoverageCalc = new LineCoverageCalc();
            string selectedClassName = GetSelectedClass(syntaxNode, selectedPosition);
            string methodName = GetSelectedMethod(syntaxNode, selectedPosition);

            int[] coverage = lineCoverageCalc.CalculateForTest(_allDDocumentsRewrite, _solutionPath, documentName, selectedClassName, methodName);

            return coverage;
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