using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace TestCoverage.Tasks
{
    public interface IDocumentProvider
    {
        SyntaxNode GetSyntaxNodeFromTextSnapshot(ITextSnapshot textSnapshot);
    }
}