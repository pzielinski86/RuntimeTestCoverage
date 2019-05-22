using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TestCoverage;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string TestSubjectSlnPath = @"C:\projects\new\RuntimeTestCoverage\RuntimeTestCoverage\RuntimeTestCoverage.sln";

        private static void Main(string[] args)
        {
            Config.SetSolution(TestSubjectSlnPath);

            var engine = new SolutionCoverageEngine();
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += Workspace_WorkspaceFailed;
            workspace.OpenSolutionAsync(TestSubjectSlnPath).Wait();
     
        

            engine.Init(workspace);
            
            for (int i = 0; i < 1; i++)
            {
                Console.WriteLine("***Scenario - START***");
                TestForAllDocuments(engine);
                TestForOneMethod(engine);
                Console.WriteLine("***Scenario - END***");
                Console.WriteLine();
            }
        }

        private static void Workspace_WorkspaceFailed(object sender, Microsoft.CodeAnalysis.WorkspaceDiagnosticEventArgs e)
        {
            Console.WriteLine(e.Diagnostic.ToString());
            Console.WriteLine();
        }

        private static void TestForOneMethod(SolutionCoverageEngine engine)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            string documentPath =
                @"C:\projects\RuntimeTestCoverage\RuntimeTestCoverage\TestCoverage.Tests\NUnitTestExtractorTests.cs";
            string documentContent = File.ReadAllText(documentPath);

            var positions = engine.CalculateForDocument("TestCoverage.Tests", documentPath, documentContent);

            Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
            Console.WriteLine("Rewrite&run selected method.Time: {0}", stopwatch.ElapsedMilliseconds);
        }

        private static void TestForAllDocuments(SolutionCoverageEngine engine)
        {
            var memoryBefore = GC.GetTotalMemory(false);
            Stopwatch stopwatch = Stopwatch.StartNew();

            var positions = engine.CalculateForAllDocumentsAsync().Result;

            Console.WriteLine("Documents: {0}", positions.CoverageByDocument.Count);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);
            Console.WriteLine("Memory: {0}", (GC.GetTotalMemory(false) - memoryBefore) / 1024 / 1024);
        }
    }
}
