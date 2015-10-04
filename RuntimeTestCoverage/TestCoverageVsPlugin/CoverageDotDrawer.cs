using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Formatting;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public class CoverageDotDrawer
    {
        private readonly IReadOnlyCollection<LineCoverage> _lineCoverage;
        public string SourceCode { get; }

        public CoverageDotDrawer(IReadOnlyCollection<LineCoverage> lineCoverage, string sourceCode)
        {
            _lineCoverage = lineCoverage;
            SourceCode = sourceCode;
        }

        public IEnumerable<CoverageDot> Draw(int[] lineStartPositions, bool areCalcsInProgress)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceCode);
            int lineNumber = 0;

            foreach (var methodDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (methodDeclarationSyntax.SpanStart < lineStartPositions[0])
                    continue;

                foreach (var statement in methodDeclarationSyntax.DescendantNodes().OfType<StatementSyntax>())
                {
                    if (statement is BlockSyntax)
                        continue;

                    while (lineStartPositions[lineNumber] < statement.FullSpan.Start)
                    {
                        lineNumber++;

                        if (lineStartPositions.Length == lineNumber)
                            yield break;
                    }

                    if (lineStartPositions[lineNumber] == statement.FullSpan.Start)
                    {
                        while (lineStartPositions.Length > lineNumber + 1 && lineStartPositions[lineNumber + 1] < statement.SpanStart)
                        {
                            lineNumber++;

                            if (lineStartPositions.Length == lineNumber)
                                yield break;
                        }

                        CoverageDot dot = CreateDotCoverage(statement, areCalcsInProgress, lineNumber);

                        if (dot != null)
                            yield return dot;
                    }
                }
            }
        }
        
        private CoverageDot CreateDotCoverage(StatementSyntax currentStatement, bool areCalcsInProgress, int lineNumber)
        {
            Brush color;

            if (areCalcsInProgress)
                color = Brushes.DarkGray;
            else
            {
                LineCoverage coverage = GetCoverageBySpan(currentStatement);

                if (coverage != null)
                    color = coverage.IsSuccess ? Brushes.Green : Brushes.Red;
                else
                    color = Brushes.Silver;
            }

            var coverageDot = new CoverageDot
            {
                Color = color,
                LineNumber = lineNumber
            };

            return coverageDot;
        }

        private LineCoverage GetCoverageBySpan(StatementSyntax currentStatement)
        {
            var coverage = _lineCoverage.FirstOrDefault(x => x.Span == currentStatement.Span.Start);

            return coverage;
        }
    }
}
