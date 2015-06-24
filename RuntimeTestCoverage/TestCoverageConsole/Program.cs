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
            const string solutionPath = @"C:\projects\RuntimeTestCoverage\TestSolution\TestSolution.sln";

            var rewritter=new SolutionRewritter();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RewriteResult rewriteResult = rewritter.RewriteAllClasses(solutionPath);

            foreach (RewrittenItemInfo item in rewriteResult.Items)
            {
             
                Console.WriteLine("Syntax tree for {0}:",item.DocumentName);   
                Console.WriteLine(item.Tree.ToString());

                Console.WriteLine("Audit variable mapping:");                
                Console.WriteLine("END---------------------END\n");
            }

            Console.WriteLine(rewriteResult.AuditVariablesMap.ToString());
            

            var lineCoverageCalc=new LineCoverageCalc();
            lineCoverageCalc.CalculateForAllTests(solutionPath,rewriteResult);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
        }
    }
}
