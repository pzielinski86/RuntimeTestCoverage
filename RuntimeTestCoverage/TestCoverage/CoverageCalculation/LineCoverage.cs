using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Rewrite;

namespace TestCoverage.CoverageCalculation
{
    [Serializable]
    public class LineCoverage
    {
        public int Span { get; set; }
        public string Path { get; set; }
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
                Path = variableName.NodePath,
                Span = variableName.SpanStart
            };

            return lineCoverage;
        }
    }
}