using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.CoverageCalculation
{
    [TestFixture]
    public class LineCoverageCalcTests
    {
        private ISolutionExplorer _solutionExplorerMock;
        private ICompiler _compilerMock;
        private LineCoverageCalc _lineCoverageCalc;

        [SetUp]
        public void Setup()
        {
            _compilerMock = NSubstitute.Substitute.For<ICompiler>();
            _solutionExplorerMock = NSubstitute.Substitute.For<ISolutionExplorer>();
            _lineCoverageCalc=new LineCoverageCalc(_solutionExplorerMock,_compilerMock);
        }

        //[Test]
        //public void Should_CompileProvidedDocuments()
        //{
        //    AuditVariablesMap auditVariablesMap=new AuditVariablesMap();
        //    var rewrittenItemsByProject=new Dictionary<Project, List<RewrittenItemInfo>>();
        //    Project project=new 

        //    rewrittenItemsByProject.Add();


        //    RewriteResult rewriteResult = new RewriteResult(rewrittenItemsByProject, auditVariablesMap);
        //    _lineCoverageCalc.CalculateForAllTests()
        //}
         
    }
}