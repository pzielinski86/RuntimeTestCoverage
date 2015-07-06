using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public class SolutionTestCoverage
    {
        private readonly string _solutionPath;
        private readonly DTE _dte;
        private Dictionary<string, List<LineCoverage>> _solutionCoverage;
        private readonly AppDomainSetup _appDomainSetup;
        public const string TestcoverageDll = "TestCoverage.dll";
        public const string DomainName = "TestCoverage";

        public SolutionTestCoverage(string solutionPath, DTE dte)
        {
            _solutionPath = solutionPath;
            _dte = dte;
            _appDomainSetup = new AppDomainSetup { LoaderOptimization = LoaderOptimization.MultiDomain };

        }

        public Dictionary<string, List<LineCoverage>> SolutionCoverage
        {
            get { return _solutionCoverage; }
            set { _solutionCoverage = value; }
        }

        public void CalculateForAllDocuments()
        {
            AppDomain domain = null;

            try
            {
                domain = AppDomain.CreateDomain(DomainName, null, _appDomainSetup);

                var engine =
                    (SolutionCoverageEngine)
                        domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(SolutionCoverageEngine).FullName);

                engine.Init(_solutionPath);

                Dictionary<string, LineCoverage[]> coverage = engine.CalculateForAllDocuments();
                _solutionCoverage = coverage.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }

        }

        public void CalculateForDocument(string documentPath, string documentContent, int selectedPosition)
        {
            SyntaxNode syntaxNode = CSharpSyntaxTree.ParseText(documentContent).GetRoot();

            ClassDeclarationSyntax selectedClass = GetSelectedClassNode(syntaxNode, selectedPosition);
            string methodName = GetSelectedMethodName(syntaxNode, selectedPosition);

            Dictionary<string, LineCoverage[]> coverage;

            AppDomain domain = null;

            try
            {
                domain = AppDomain.CreateDomain(DomainName, null, _appDomainSetup);

                var engine =
                    (SolutionCoverageEngine)
                        domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(SolutionCoverageEngine).FullName);

                engine.Init(_solutionPath);

                object[] projects = (object[])_dte.ActiveSolutionProjects;

                Project selectedProject = projects.OfType<Project>().Single();

                coverage = engine.CalculateForTest(selectedProject.Name, documentPath, documentContent,
                    selectedClass.Identifier.Text,
                    methodName);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }

            string path = string.Format("{0}.{1}.{2}", GetSelectedNamespaceNode(selectedClass).Name,
                selectedClass.Identifier.Text, methodName);

            UpdateSolutionCoverage(coverage,path);
        }

        private void UpdateSolutionCoverage(Dictionary<string, LineCoverage[]> coverage,string currentPath)
        {
            foreach (string docPath in _solutionCoverage.Keys)
            {
                List<LineCoverage> documentCoverage = _solutionCoverage[docPath];

                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestPath == currentPath || documentCoverage[i].Path == currentPath)
                    {
                        documentCoverage.RemoveAt(i);
                        i--;
                    }
                }

                if (coverage.ContainsKey(docPath))
                    _solutionCoverage[docPath].AddRange(coverage[docPath]);
            }
        }
        
        private string GetSelectedMethodName(SyntaxNode syntaxNode, int selectedPosition)
        {
            var method = syntaxNode.DescendantNodes().
                 OfType<MethodDeclarationSyntax>().Reverse().
                 First(d => d.SpanStart <= selectedPosition);

            return method.Identifier.Text;
        }

        private NamespaceDeclarationSyntax GetSelectedNamespaceNode(SyntaxNode classNode)
        {
            SyntaxNode parent = classNode;

            while (!(parent is NamespaceDeclarationSyntax))
            {
                parent = parent.Parent;
            }

            return (NamespaceDeclarationSyntax)parent;
        }

        private ClassDeclarationSyntax GetSelectedClassNode(SyntaxNode syntaxNode, int selectedPosition)
        {
            ClassDeclarationSyntax selectedClass = syntaxNode.DescendantNodes().
                OfType<ClassDeclarationSyntax>().Reverse().
                First(d => d.SpanStart <= selectedPosition);

            return selectedClass;
        }
    }
}