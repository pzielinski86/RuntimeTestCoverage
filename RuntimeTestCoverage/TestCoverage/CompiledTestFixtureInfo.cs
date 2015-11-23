using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Compilation;

namespace TestCoverage
{
    internal class CompiledTestFixtureInfo
    {
        public string[] AllReferences { get; set; }
        public string TestDocumentPath { get; set; }
        public ClassDeclarationSyntax TestClass { get; set; }
        public ISemanticModel SemanticModel { get; set; }

    }
}