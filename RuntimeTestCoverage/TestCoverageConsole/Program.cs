using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestCoverage;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string RuntimetestcoverageSln = @"../../../../TestSolution/TestSolution.sln";

        private static void Main(string[] args)
        {
            var engine = new AppDomainSolutionCoverageEngine();
            engine.Init(RuntimetestcoverageSln);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var positions = engine.CalculateForAllDocuments();

            Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);

            engine=new AppDomainSolutionCoverageEngine();
            engine.Init(RuntimetestcoverageSln);

            stopwatch = Stopwatch.StartNew();

            string documentContent = File.ReadAllText(@"../../../../TestSolution/Math.Tests/MathHelperTests.cs");
            var documentPositions = engine.CalculateForTest("Math.Tests", Path.GetFullPath(@"../../../../TestSolution/Math.Tests/MathHelperTests.cs"),
                documentContent, "MathHelperTests",
                "DivideTestZero");

            Console.WriteLine("Positions: {0}", documentPositions.CoverageByDocument.Count);
            Console.WriteLine("Single document rewrite time: {0}", stopwatch.ElapsedMilliseconds);
        }
    }
}
