using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestCoverage;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string TestSubjectSlnPath = @"C:\github\TestingSandbox\LeadGenDataService\LeadGenDataService.sln";

        private static void Main(string[] args)
        {
            ISolutionExplorer solutionExplorer=new SolutionExplorer(TestSubjectSlnPath);
            solutionExplorer.Open();

            using (var engine = new AppDomainSolutionCoverageEngine())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                engine.Init(solutionExplorer);
                var positions = engine.CalculateForAllDocuments();

                Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
                Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
            }

            using (var engine = new AppDomainSolutionCoverageEngine())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                engine.Init(solutionExplorer);

                string documentPath = @"C:\github\TestingSandbox\LeadGenDataService\src\LeadGenDataService.Utils\UkPhoneNumberNormalizer.cs";
                string documentContent = File.ReadAllText(documentPath);

                var positions = engine.CalculateForDocument("LeadGenDataService.Utils", documentPath,documentContent);

                Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
                Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
