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
        private int _currentSpan;
        public string SourceCode { get; }

        public CoverageDotDrawer(IReadOnlyCollection<LineCoverage> lineCoverage, string sourceCode, int currentSpan)
        {
            _lineCoverage = lineCoverage;
            _currentSpan = currentSpan;
            SourceCode = sourceCode;
        }

        public IEnumerable<CoverageDot> Draw(int[] positions, bool areCalcsInProgress)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceCode);
            var root = syntaxTree.GetRoot();

            int currentMethodIndex = 0;
            MethodDeclarationSyntax[] allMethods = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();

            int lineNumber = 0;

            foreach (var methodDeclarationSyntax in allMethods)
            {
                if (methodDeclarationSyntax.SpanStart < _currentSpan)
                    continue;
                

                foreach (var statement in methodDeclarationSyntax.DescendantNodes().OfType<StatementSyntax>())
                {
                    if (statement is BlockSyntax)
                        continue;

                    while (positions[lineNumber] < statement.FullSpan.Start)
                    {
                        lineNumber++;
                        if (positions.Length == lineNumber)
                            yield break;
                    }

               

                    if (positions[lineNumber] == statement.FullSpan.Start)
                    {
                        string statementText = statement.ToFullString();
                        string[] statementLines = statementText.Split(new[] {Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries);

                        while (positions.Length>lineNumber+1&&positions[lineNumber+1]<statement.SpanStart)
                        {
                            lineNumber++;

                            if (positions.Length == lineNumber)
                                yield break;
                        }


                        CoverageDot dot = CreateDotCoverage(statement, areCalcsInProgress, null, lineNumber);
                        if (dot != null)
                            yield return dot;
                    }
                
                }
            }

           //for (int i = 0; i < lines.Length; i++)
           // {
           //     _currentSpan = SourceCode.IndexOf(lines[i], _currentSpan + 1, StringComparison.Ordinal);
           //     if (_currentSpan == -1)
           //         continue;

           //     var lineSpan = new TextSpan(_currentSpan, lines[i].Length);

           //     var currentStatement = root.DescendantNodes(lineSpan).
           //         OfType<StatementSyntax>().Where(x => !(x is BlockSyntax)).
           //         Where(x => lineSpan.Start==x.FullSpan.Start).FirstOrDefault();


           //     var aa=root.DescendantNodes(lineSpan).
           //         OfType<StatementSyntax>().Where(x => !(x is BlockSyntax)).
           //         Where(x => lineSpan.Start == x.FullSpan.Start).Count();

           //     if (currentStatement==null)
           //         continue;

               

           //     currentMethodIndex = GetCurrentMethodIndex(allMethods, currentMethodIndex);

           //     if (currentMethodIndex >= allMethods.Length)
           //         break;

           //     var methodBlockSyntax = allMethods[currentMethodIndex].ChildNodes().OfType<BlockSyntax>().First();

           //     if (_currentSpan < methodBlockSyntax.FullSpan.Start)
           //         continue;

           //     CoverageDot dot = CreateDotCoverage(currentStatement,areCalcsInProgress, lines[i], i);
           //     if (dot != null)
           //         yield return dot;
           // }
        }

        private int GetCurrentMethodIndex(MethodDeclarationSyntax[] allMethods, int previousMethodIndex)
        {
            int currentMethodIndex = previousMethodIndex;
            var methodBlockSyntax = allMethods[previousMethodIndex].ChildNodes().OfType<BlockSyntax>().First();

            if (_currentSpan >= methodBlockSyntax.Span.End)
                currentMethodIndex++;

            return currentMethodIndex;
        }

        private CoverageDot CreateDotCoverage(StatementSyntax currentStatement, bool areCalcsInProgress, string currentLineText, int lineNumber)
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

        private static bool IsBracketOrEmptyLine(string currentLineText)
        {
            return currentLineText.Trim() == "}" || currentLineText.Trim() == "{" || string.IsNullOrEmpty(currentLineText.Trim());
        }
    }
}
