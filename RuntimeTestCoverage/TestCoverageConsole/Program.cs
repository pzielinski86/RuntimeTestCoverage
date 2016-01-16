using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TestCoverage;
using TestCoverage.Extensions;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string TestSubjectSlnPath = @"../../../../TestSolution/TestSolution.sln";

        private static void Main(string[] args)
        {
            Config.SetSolution(TestSubjectSlnPath);
            var engine = new SolutionCoverageEngine();
            engine.Init(TestSubjectSlnPath);

            for (int i = 0; i < 2; i++)
            {
                TestForAllDocuments(engine);
                TestForOneDocument(engine);
            }
        }

        private static void TestForOneDocument(SolutionCoverageEngine engine)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string documentPath =
                @"../../../../TestSolution/Math.Tests/MathHelperTests.cs";
            string documentContent = File.ReadAllText(documentPath);

            var positions = engine.CalculateForDocument("Math.Tests", documentPath, documentContent);

            Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
            Console.WriteLine("Rewrite&run selected method.Time: {0}", stopwatch.ElapsedMilliseconds);
        }

        private static void TestForAllDocuments(SolutionCoverageEngine engine)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var positions = engine.CalculateForAllDocumentsAsync().Result;

            Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
        }
    }
}
