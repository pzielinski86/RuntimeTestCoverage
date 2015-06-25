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
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string solutionPath = @"../../../../TestSolution/TestSolution.sln";

            var domain = AppDomain.CreateDomain("coverage");
            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof(LineCoverageEngine).FullName);

            Stopwatch stopwatch = Stopwatch.StartNew();

            int[] positions = engine.CalculateForAllDocuments(solutionPath);

            Console.WriteLine("Positions: {0}", positions.Length);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);

            AppDomain.Unload(domain);
            domain = AppDomain.CreateDomain("coverage");
            engine =
   (LineCoverageEngine)
       domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof(LineCoverageEngine).FullName);

            stopwatch = Stopwatch.StartNew();

            int[] documentPositions = engine.CalculateForTest(solutionPath, "MathHelperTests.cs", "MathHelperTests",
                "DivideTestZero");

            Console.WriteLine("Positions: {0}", documentPositions.Length);
            Console.WriteLine("Single document rewrite time: {0}", stopwatch.ElapsedMilliseconds);

        }
    }
}
