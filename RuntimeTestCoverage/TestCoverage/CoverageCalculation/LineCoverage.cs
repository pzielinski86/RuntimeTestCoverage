using Microsoft.CodeAnalysis;
using System;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class LineCoverage
    {
        public string ErrorMessage { get; set; }
        public int Span { get; set; }
        public string NodePath { get; set; }
        public string TestPath { get; set; }
        public string DocumentPath { get; set; }
        public string TestDocumentPath { get; set; }
        public bool IsSuccess { get; set; }

        public static LineCoverage EvaluateAuditVariable(
            AuditVariablePlaceholder variableName,
            SyntaxNode testMethodNode,
            string testProjectName,
            string testDocName)
        {
            LineCoverage lineCoverage = new LineCoverage
            {
                TestPath = NodePathBuilder.BuildPath(testMethodNode, testDocName, testProjectName),
                NodePath = variableName.NodePath,
                Span = variableName.SpanStart
            };

            return lineCoverage;
        }
    }
}