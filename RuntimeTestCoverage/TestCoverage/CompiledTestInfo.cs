using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    internal class CompiledTestInfo
    {
        public MetadataReference[] TestProjectReferences { get; set; }
        public Assembly[] AllAssemblies { get; set; }
        public AuditVariablesMap AuditVariablesMap { get; set; }
        public string TestDocumentPath { get; set; }
        public ClassDeclarationSyntax TestClass { get; set; }
        public CompiledItem TestProjectCompilationItem { get; set; }

    }
}