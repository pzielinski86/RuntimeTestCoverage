using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace LiveCoverageVsPlugin
{
    public class CoverageDotDrawer
    {
        private readonly IReadOnlyCollection<LineCoverage> _lineCoverage;
        private readonly string _documentName;
        public string SourceCode { get; }

        public CoverageDotDrawer(IReadOnlyCollection<LineCoverage> lineCoverage, string sourceCode, string documentName)
        {
            _lineCoverage = lineCoverage;
            _documentName = documentName;
            SourceCode = sourceCode;
        }

        public List<CoverageDot> Draw(int[] lineStartPositions, bool areCalcsInProgress, string projectName)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceCode);
            var coverageDots = new List<CoverageDot>();
            int lineNumber = 0;

            foreach (var methodDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
            {
                if (methodDeclarationSyntax.Span.End < lineStartPositions[0])
                    continue;

                var methodDots = ProcessMethod(projectName, methodDeclarationSyntax, lineStartPositions,
                    areCalcsInProgress, ref lineNumber);

                coverageDots.AddRange(methodDots);

                if (lineNumber >= lineStartPositions.Length)
                    break;
            }

            return coverageDots;
        }

        private List<CoverageDot> ProcessMethod(string projectName,
            BaseMethodDeclarationSyntax methodDeclarationSyntax,
            int[] lineStartPositions,
            bool areCalcsInProgress,
            ref int lineNumber)
        {
            List<CoverageDot> coverageDots = new List<CoverageDot>();
            string methodPath = NodePathBuilder.BuildPath(methodDeclarationSyntax, _documentName, projectName);

            foreach (var statement in methodDeclarationSyntax.DescendantNodes().OfType<StatementSyntax>())
            {
                if (statement is BlockSyntax)
                    continue;

                if (!LoopUntilStartPositionIsFound(lineStartPositions, statement, ref lineNumber))
                    return coverageDots;

                if (lineStartPositions[lineNumber] == statement.FullSpan.Start)
                {
                    if (!LoopUntilLeadingTriviaIsSkipped(lineStartPositions, statement, ref lineNumber))
                        return coverageDots;

                    int span = statement.SpanStart - methodDeclarationSyntax.SpanStart;

                    CoverageDot dot = CreateDotCoverage(span, areCalcsInProgress, lineNumber, methodPath);

                    if (dot != null)
                        coverageDots.Add(dot);
                }
            }

            return coverageDots;
        }

        private bool LoopUntilLeadingTriviaIsSkipped(int[] lineStartPositions, StatementSyntax statement, ref int lineNumber)
        {
            while (lineStartPositions.Length > lineNumber + 1 && lineStartPositions[lineNumber + 1] < statement.SpanStart)
            {
                lineNumber++;
            }

            return lineNumber < lineStartPositions.Length;
        }

        private bool LoopUntilStartPositionIsFound(int[] lineStartPositions, StatementSyntax statement, ref int lineNumber)
        {
            while (lineNumber < lineStartPositions.Length && lineStartPositions[lineNumber] < statement.FullSpan.Start)
            {
                lineNumber++;
            }

            return lineNumber < lineStartPositions.Length;
        }

        private CoverageDot CreateDotCoverage(int span, bool areCalcsInProgress, int lineNumber, string methodPath)
        {
            Brush color;
            string tooltip;

            if (areCalcsInProgress)
            {
                color = Brushes.DarkGray;
                tooltip = "Calculating...";
            }
            else
            {
                LineCoverage coverage = GetCoverageBySpan(methodPath, span);

                if (coverage != null)
                {
                    if (coverage.IsSuccess)
                    {
                        color = Brushes.Green;
                        tooltip = "Passed";
                    }
                    else
                    {
                        color = Brushes.Red;
                        tooltip = coverage.ErrorMessage;
                    }
                }
                else
                {
                    color = Brushes.DarkOrange;
                    tooltip = "No coverage";
                }
            }

            var coverageDot = new CoverageDot
            {
                Color = color,
                LineNumber = lineNumber,
                Tooltip = tooltip
            };

            return coverageDot;
        }

        private LineCoverage GetCoverageBySpan(string methodPath, int span)
        {
            var coverage = _lineCoverage.
                Where(x => x.Span == span && x.NodePath == methodPath)
                .OrderBy(x => x.IsSuccess).FirstOrDefault();

            return coverage;
        }
    }
}
