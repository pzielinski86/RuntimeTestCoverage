using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using TestCoverage;

namespace TestCoverageConsole
{
    class Program
    {
        static void Main(string[] args)
        {            
            const string solutionPath = @"../../../../TestSolution/TestSolution.sln";

            var rewritter=new SolutionRewritter();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RewriteResult rewriteResult = rewritter.RewriteAllClasses(solutionPath);

            foreach (RewrittenItemInfo item in rewriteResult.Items)
            {             
                DisplayRewrittenItem(item);
            }

            Console.WriteLine(rewriteResult.AuditVariablesMap.ToString());
            

            var lineCoverageCalc=new LineCoverageCalc();
            lineCoverageCalc.CalculateForAllTests(solutionPath,rewriteResult);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);

            RunTest(rewriteResult, "MathHelperTests.cs");
        }

        private static void DisplayRewrittenItem(RewrittenItemInfo item)
        {
            Console.WriteLine("Syntax tree for {0}:", item.Document.Name);
            Console.WriteLine(item.SyntaxTree.ToString());

            Console.WriteLine("Audit variable mapping:");
            Console.WriteLine("END---------------------END\n");
        }

        private static void RunTest(RewriteResult rewriteResult, string documentName)
        {
            Console.WriteLine("Rewriting {0}",documentName);

            Stopwatch stopwatch=Stopwatch.StartNew();


            var rewritter = new SolutionRewritter();
            var item = rewritter.RewriteTestClass(rewriteResult, documentName);
            DisplayRewrittenItem(item);

            Console.WriteLine("Time:{0}",stopwatch.ElapsedMilliseconds);
        }
    }
}
