using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using TestCoverage;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string TestSubjectSlnPath = @"../../../../TestSolution/TestSolution.sln";

        private static void Main(string[] args)
        {
            for (int i = 0; i < 2; i++)
            {
                using (var engine = new AppDomainSolutionCoverageEngine())
                {
                    engine.Init(TestSubjectSlnPath);

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    var positions = engine.CalculateForAllDocuments();

                    Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
                    Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
                }

                using (var engine = new AppDomainSolutionCoverageEngine())
                {
                    engine.Init(TestSubjectSlnPath);

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    string documentPath = @"C:\github\TestingSandbox\LeadGenDataService\src\LeadGenDataService.Utils\UkPhoneNumberNormalizer.cs";
                    string documentContent = File.ReadAllText(documentPath);

                    var positions = engine.CalculateForDocument("Math", documentPath, documentContent);

                    Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
                    Console.WriteLine("Rewrite&run selected document.Time: {0}", stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }
}
