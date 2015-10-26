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