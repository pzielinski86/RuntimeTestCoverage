using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage;

namespace TestCoverageVsPlugin
{
    public class DocumentResult : ResultItemBase
    {
        public ClassResult[] ClassResults { get; private set; }

        public DocumentResult(ClassResult[] classResults, string name) : base(name)
        {
            ClassResults = classResults;
        }
    }


    public abstract class ResultItemBase
    {
        public ResultItemBase(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }

    public class ProjectResult : ResultItemBase
    {
        public DocumentResult[] DocumentResults { get; set; }

        public ProjectResult(string name, DocumentResult[] documentResults) : base(name)
        {
            DocumentResults = documentResults;
        }
    }

    public class SolutionResult : ResultItemBase
    {
        public ProjectResult[] ProjectResults { get; set; }

        public SolutionResult(string name, ProjectResult[] projectResults) : base(name)
        {
            ProjectResults = projectResults;
        }
    }

    public class ClassResult : ResultItemBase
    {
        public MethodResult[] MethodResults { get; private set; }
        public ClassResult(MethodResult[] methodResults, string name) : base(name)
        {
            MethodResults = methodResults;
        }
    }

    public class MethodResult : ResultItemBase
    {
        public int[] CoveragePositions { get; set; }

        public MethodResult(string name, int[] coveragePositions) : base(name)
        {
            CoveragePositions = coveragePositions;
        }
    }
    public class DocumentTestCoverage
    {
        private readonly string _solutionPath;
        private List<LineCoverage> _coverage=new List<LineCoverage>();

        public DocumentTestCoverage(string solutionPath)
        {
            _solutionPath = solutionPath;

        }

        public void CalculateForAllDocuments()
        {
            var domain = AppDomain.CreateDomain("coverage");

            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof(LineCoverageEngine).FullName);

            _coverage = engine.CalculateForAllDocuments(_solutionPath).ToList();

            AppDomain.Unload(domain);

          
        }

        public IEnumerable<int> GetPositions(string[] paths)
        {
            return _coverage.Where(x => paths.Contains(x.Path)).Select(x => x.Span);
        }

        public void CalculateForDocument(string documentName, string documentContent, int selectedPosition)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent);
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            ClassDeclarationSyntax selectedClass = GetSelectedClass(syntaxNode, selectedPosition);
            string methodName = GetSelectedMethod(syntaxNode, selectedPosition);

            var domain = AppDomain.CreateDomain("coverage");

            var engine =
                (LineCoverageEngine)
                    domain.CreateInstanceFromAndUnwrap("TestCoverage.dll", typeof (LineCoverageEngine).FullName);

            var coverage = engine.CalculateForTest(_solutionPath, documentName, documentContent, selectedClass.Identifier.Text,
                methodName);

            AppDomain.Unload(domain);

            string path = string.Format("{0}.{1}.{2}.", GetSelectedNamespace(selectedClass).Name,
                selectedClass.Identifier.Text, methodName);

            for(int i=0;i<_coverage.Count;i++)
            {
                if (_coverage[i].TestPath == path||_coverage[i].Path==path)
                {
                    _coverage.RemoveAt(i);
                    i--;
                }
                
            }



            _coverage.AddRange(coverage);
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