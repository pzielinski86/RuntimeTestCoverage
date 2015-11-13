using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace TestCoverageVsPlugin
{
    public interface IDocumentFromTextSnapshotExtractor
    {
        SyntaxNode ExtactDocument(ITextSnapshot snapshot);
    }
}