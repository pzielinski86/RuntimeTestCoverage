using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using TestCoverage;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    public class SolutionTestCoverage
    {
        private SolutionExplorer _solutionExplorer;
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
            _solutionExplorer=new SolutionExplorer(_solutionPath);
            _solutionExplorer.Open();
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

                CoverageResult coverage = engine.CalculateForAllDocuments();
                _solutionCoverage = coverage.CoverageByDocument.ToDictionary(x => x.Key, x => x.Value.ToList());
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public Task CalculateForSelectedItemAsync(string documentPath, string documentContent, int selectedPosition)
        {
            return Task.Factory.StartNew(() => CalculateForSelectedItem(documentPath, documentContent, selectedPosition));
        }
    
        public void CalculateForSelectedItem(string documentPath, string documentContent, int selectedPosition)
        {
            SyntaxNode syntaxNode = CSharpSyntaxTree.ParseText(documentContent).GetCompilationUnitRoot();

            ClassDeclarationSyntax selectedClass = GetSelectedClassNode(syntaxNode, selectedPosition);
            if (selectedClass == null)
            {
                CalculateForDocument(documentPath, documentContent);
                return;
            }
            MethodDeclarationSyntax methodNode = GetSelectedMethodNode(selectedClass, selectedPosition);

            if (methodNode == null)
            {
                CalculateForDocument(documentPath, documentContent);
                return;
            }

            CoverageResult coverage;

            AppDomain domain = null;

            var selectedProject = _solutionExplorer.GetProjectByDocument(documentPath);

            try
            {
                domain = AppDomain.CreateDomain(DomainName, null, _appDomainSetup);

                var engine =
                    (SolutionCoverageEngine)
                        domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(SolutionCoverageEngine).FullName);

                engine.Init(_solutionPath);



                coverage = engine.CalculateForTest(selectedProject.Name, documentPath, documentContent,
                    selectedClass.Identifier.ToString(), methodNode.Identifier.ToString());
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }

            string path = string.Format("{0}.{1}.{2}.{3}.{4}", selectedProject.Name,
                Path.GetFileNameWithoutExtension(documentPath), GetSelectedNamespaceNode(selectedClass).Name,
                selectedClass.Identifier, methodNode.Identifier);

            UpdateSolutionCoverage(coverage);
        }

        public Task CalculateForDocumentAsync(string documentPath, string documentContent)
        {
            return Task.Factory.StartNew(() => CalculateForDocument(documentPath, documentContent));
        }

        public void CalculateForDocument(string documentPath, string documentContent)
        {
            CoverageResult coverage;

            AppDomain domain = null;

            var selectedProject = _solutionExplorer.GetProjectByDocument(documentPath);

            string path = string.Format("{0}.{1}", selectedProject.Name, Path.GetFileNameWithoutExtension(documentPath));
            ClearCoverage(path);

            try
            {
                domain = AppDomain.CreateDomain(DomainName, null, _appDomainSetup);

                var engine =
                    (SolutionCoverageEngine)
                        domain.CreateInstanceFromAndUnwrap(TestcoverageDll, typeof(SolutionCoverageEngine).FullName);

                engine.Init(_solutionPath);

                coverage = engine.CalculateForDocument(selectedProject.Name, documentPath, documentContent);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }            

            UpdateSolutionCoverage(coverage);

        }

    
        public void ClearCoverage(string path)
        {
            foreach (string docPath in _solutionCoverage.Keys)
            {
                List<LineCoverage> documentCoverage = _solutionCoverage[docPath];

                for (int i = 0; i < documentCoverage.Count; i++)
                {
                    if (documentCoverage[i].TestPath.StartsWith(path) ||
                        documentCoverage[i].Path.StartsWith(path))
                    {
                        documentCoverage.RemoveAt(i);
                        i--;
                    }
                }

            }
        }

        private void UpdateSolutionCoverage(CoverageResult coverage)
        {
            foreach (string docPath in coverage.CoverageByDocument.Keys)
            {
                if (coverage.CoverageByDocument.ContainsKey(docPath))
                    _solutionCoverage[docPath].AddRange(coverage.CoverageByDocument[docPath]);
                else
                    _solutionCoverage[docPath] = coverage.CoverageByDocument[docPath].ToList();
            }
        }        

        private MethodDeclarationSyntax GetSelectedMethodNode(SyntaxNode syntaxNode, int selectedPosition)
        {
            var method = syntaxNode.DescendantNodes().
                 OfType<MethodDeclarationSyntax>().Reverse().
                 First(d => d.SpanStart <= selectedPosition);

            return method;
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