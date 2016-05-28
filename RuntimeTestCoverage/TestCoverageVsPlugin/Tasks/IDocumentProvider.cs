using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace TestCoverageVsPlugin.Tasks
{
    public interface IDocumentProvider
    {
        SyntaxNode GetSyntaxNodeFromTextSnapshot(ITextSnapshot textSnapshot);
    }
}