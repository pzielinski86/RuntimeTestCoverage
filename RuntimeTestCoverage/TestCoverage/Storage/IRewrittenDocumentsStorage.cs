using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TestCoverage.Storage
{
    public interface IRewrittenDocumentsStorage
    {
        IEnumerable<SyntaxTree> GetRewrittenDocuments(string solutionPath,string projectName, params string[] excludedDocuments);
        void Store(string solutionPath,string projectName,string docPath, SyntaxNode documentContent);
        void Clear(string projectName);
    }
}