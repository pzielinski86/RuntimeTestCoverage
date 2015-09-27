using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestCoverage;

namespace TestCoverageConsole
{
    internal class Program
    {
        private const string RuntimetestcoverageSln = @"../../../RuntimeTestCoverage.sln";

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var appDomainSetup=new AppDomainSetup();
            appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomain;

            var domain = AppDomain.CreateDomain("coverage",null, appDomainSetup);
            var engine =(SolutionCoverageEngine)domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof (SolutionCoverageEngine).FullName);
            engine.Init(RuntimetestcoverageSln);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var positions = engine.CalculateForAllDocuments();

            Console.WriteLine("Documents: {0}", positions.Count);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);

            AppDomain.Unload(domain);

            domain = AppDomain.CreateDomain("coverage", null, appDomainSetup);
            engine =
                (SolutionCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof (SolutionCoverageEngine).FullName);

            engine.Init(RuntimetestcoverageSln);

            stopwatch = Stopwatch.StartNew();

            string documentContent = File.ReadAllText(@"../../../../TestSolution/Math.Tests/MathHelperTests.cs");
            var documentPositions = engine.CalculateForTest("Math.Tests", Path.GetFullPath(@"../../../../TestSolution/Math.Tests/MathHelperTests.cs"),
                documentContent, "MathHelperTests",
                "DivideTestZero");

            Console.WriteLine("Positions: {0}", documentPositions.Count);
            Console.WriteLine("Single document rewrite time: {0}", stopwatch.ElapsedMilliseconds);
        }


        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("TestCoverage"))
            {
                string path = Assembly.GetExecutingAssembly().Location;
                path = System.IO.Path.GetDirectoryName(path);

                return Assembly.LoadFrom(System.IO.Path.Combine(path, "TestCoverage.dll"));
            }
            return null;
        }
    }
}
