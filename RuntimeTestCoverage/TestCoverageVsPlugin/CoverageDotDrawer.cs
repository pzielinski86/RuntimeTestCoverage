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

        public List<CoverageDot> Draw(int[] lineStartPositions, bool areCalcsInProgress)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceCode);
            var coverageDots = new List<CoverageDot>();
            int lineNumber = 0;

            foreach (var methodDeclarationSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (methodDeclarationSyntax.SpanStart < lineStartPositions[0])
                    continue;

                if (!ProcessMethod(coverageDots, methodDeclarationSyntax, lineStartPositions, areCalcsInProgress, ref lineNumber))
                    break;
            }

            return coverageDots;
        }

        private bool ProcessMethod(List<CoverageDot> coverageDots, MethodDeclarationSyntax methodDeclarationSyntax, int[] lineStartPositions, bool areCalcsInProgress, ref int lineNumber)
        {
            foreach (var statement in methodDeclarationSyntax.DescendantNodes().OfType<StatementSyntax>())
            {
                if (statement is BlockSyntax)
                    continue;

                if (!LoopUntilStartPositionIsFound(lineStartPositions, statement, ref lineNumber))
                    return false;

                if (lineStartPositions[lineNumber] == statement.FullSpan.Start)
                {
                    if (!LoopUntilLeadingTriviaIsSkipped(lineStartPositions, statement, ref lineNumber))
                        return false;

                    CoverageDot dot = CreateDotCoverage(statement, areCalcsInProgress, lineNumber);

                    if (dot != null)
                        coverageDots.Add(dot);
                }
            }

            return true;
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
            var coverage = _lineCoverage.Where(x => x.Span == currentStatement.Span.Start)
                .OrderBy(x => x.IsSuccess).FirstOrDefault();

            return coverage;
        }
    }
}
