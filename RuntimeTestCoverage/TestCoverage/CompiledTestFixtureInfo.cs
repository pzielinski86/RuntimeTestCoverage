using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;
using TestCoverage.Rewrite;

namespace TestCoverage
{
    internal class CompiledTestFixtureInfo
    {
        public MetadataReference[] TestProjectReferences { get; set; }
        public _Assembly[] AllAssemblies { get; set; }
        public AuditVariablesMap AuditVariablesMap { get; set; }
        public string TestDocumentPath { get; set; }
        public ClassDeclarationSyntax TestClass { get; set; }
        public ISemanticModel SemanticModel { get; set; }

    }
}