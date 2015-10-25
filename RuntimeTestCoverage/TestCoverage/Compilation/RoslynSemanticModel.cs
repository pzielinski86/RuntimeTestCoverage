using Microsoft.CodeAnalysis;

namespace TestCoverage.Compilation
{
    class RoslynSemanticModel : ISemanticModel
    {
        private readonly SemanticModel _semanticModel;

        public RoslynSemanticModel(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public string GetSymbolName(SyntaxNode node)
        {
            var symbolInfo =_semanticModel.GetSymbolInfo(node);

            if (symbolInfo.Symbol == null || !symbolInfo.Symbol.CanBeReferencedByName)
                return null;

            return symbolInfo.Symbol.ToString();
        }
    }
}