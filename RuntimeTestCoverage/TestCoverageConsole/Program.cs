using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using TestCoverage;

namespace TestCoverageConsole
{

    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            const string solutionPath = @"../../../../TestSolution/TestSolution.sln";          

            var domain = AppDomain.CreateDomain("coverage");
            var engine =(LineCoverageEngine)domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof (LineCoverageEngine).FullName);
            engine.Init(solutionPath);

            Stopwatch stopwatch = Stopwatch.StartNew();

            var positions = engine.CalculateForAllDocuments();

            Console.WriteLine("Positions: {0}", positions.Length);
            Console.WriteLine("Rewrite&run all projects.Time: {0}", stopwatch.ElapsedMilliseconds);

            AppDomain.Unload(domain);

            domain = AppDomain.CreateDomain("coverage");
            engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof (LineCoverageEngine).FullName);

            engine.Init(solutionPath);

            stopwatch = Stopwatch.StartNew();

            string documentContent = File.ReadAllText(@"../../../../TestSolution/Math.Tests/MathHelperTests.cs");
            var documentPositions = engine.CalculateForTest("Math.Tests", "MathHelperTests.cs",
                documentContent, "MathHelperTests",
                "DivideTestZero");

            Console.WriteLine("Positions: {0}", documentPositions.Length);
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
