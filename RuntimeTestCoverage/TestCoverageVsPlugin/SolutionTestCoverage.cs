using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;

namespace TestCoverageVsPlugin
{
    public class SolutionTestCoverage
    {
        private readonly string _solutionPath;
        private readonly DTE _dte;
        private Dictionary<string, List<LineCoverage>> _solutionCoverage;
        private readonly AppDomainSetup _appDomainSetup;
        private const string TestcoverageDll = "TestCoverage.dll";

        public SolutionTestCoverage(string solutionPath, DTE dte)
        {
            _solutionPath = solutionPath;
            _dte = dte;
            _appDomainSetup = new AppDomainSetup();
            _appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomain;

        }

        public Dictionary<string, List<LineCoverage>> SolutionCoverage
        {
            get { return _solutionCoverage; }
            set { _solutionCoverage = value; }
        }

        public void CalculateForAllDocuments()
        {
            var domain = AppDomain.CreateDomain("coverage", null, _appDomainSetup);

            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(LineCoverageEngine).FullName);

            engine.Init(_solutionPath);
            var coverage = engine.CalculateForAllDocuments();
            _solutionCoverage = coverage.ToDictionary(x => x.Key, x => x.Value.ToList());

            AppDomain.Unload(domain);

        }

        public void CalculateForDocument(string documentPath, string documentContent, int selectedPosition)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            ClassDeclarationSyntax selectedClass = GetSelectedClass(syntaxNode, selectedPosition);
            string methodName = GetSelectedMethod(syntaxNode, selectedPosition);

            var domain = AppDomain.CreateDomain("coverage", null, _appDomainSetup);

            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(LineCoverageEngine).FullName);

            engine.Init(_solutionPath);

            object[] projects = (object[])_dte.ActiveSolutionProjects;

            Project selectedProject = projects.OfType<Project>().Single();

            Dictionary<string, LineCoverage[]> coverage;

            try
            {
                coverage = engine.CalculateForTest(selectedProject.Name, documentPath, documentContent,
                    selectedClass.Identifier.Text,
                    methodName);
            }
            catch
            {
                return;
            }
            finally
            {
                AppDomain.Unload(domain);                
            }

            string path = string.Format("{0}.{1}.{2}", GetSelectedNamespace(selectedClass).Name,
                selectedClass.Identifier.Text, methodName);

            foreach (string docPath in _solutionCoverage.Keys)
            {
                var documentCoverage = _solutionCoverage[docPath];

                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestPath == path || documentCoverage[i].Path == path)
                    {
                        documentCoverage.RemoveAt(i);
                        i--;
                    }
                }

                if (coverage.ContainsKey(docPath))
                    _solutionCoverage[docPath].AddRange(coverage[docPath]);
            }
        }

        private string GetSelectedMethod(SyntaxNode syntaxNode, int selectedPosition)
        {
            var method = syntaxNode.DescendantNodes().
                 OfType<MethodDeclarationSyntax>().Reverse().
                 First(d => d.SpanStart <= selectedPosition);

            return method.Identifier.Text;
        }

        private NamespaceDeclarationSyntax GetSelectedNamespace(SyntaxNode classNode)
        {
            SyntaxNode parent = classNode;

            while (!(parent is NamespaceDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            return (NamespaceDeclarationSyntax)parent;
        }

        public ClassDeclarationSyntax GetSelectedClass(SyntaxNode syntaxNode, int selectedPosition)
        {
            ClassDeclarationSyntax selectedClass = syntaxNode.DescendantNodes().
                OfType<ClassDeclarationSyntax>().Reverse().
                First(d => d.SpanStart <= selectedPosition);

            return selectedClass;
        }
    }
}