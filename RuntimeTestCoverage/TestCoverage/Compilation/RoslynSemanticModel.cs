using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestCoverage.Extensions;

namespace TestCoverage.Compilation
{
    class RoslynSemanticModel : ISemanticModel
    {
        private readonly SemanticModel _semanticModel;

        public RoslynSemanticModel(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public string GetAssemblyName()
        {
            return _semanticModel.Compilation.AssemblyName;
        }

        public string GetFullName(ClassDeclarationSyntax classDeclarationSyntax)
        {

            return _semanticModel.GetDeclaredSymbol(classDeclarationSyntax).GetFullMetadataName();
        }

        public object GetConstantValue(SyntaxNode node)
        {
            var symbolInfo =_semanticModel.GetSymbolInfo(node);

            if (symbolInfo.Symbol == null)
                return null;

            Optional<object> value=_semanticModel.GetConstantValue(node);

            if (!value.HasValue)
                return null;

            return value.Value;
        }
    }
}