using System.Linq;
using Microsoft.VisualStudio.Text;

namespace TestCoverageVsPlugin.Extensions
{
    public static class ITextChangeExtensions
    {
        public static bool AnyCodeChanges(this ITextChange change)
        {
            return AnyCodeChanges(change.NewText) || AnyCodeChanges(change.OldText);

        }

        private static bool AnyCodeChanges(string text)
        {
            return text.Any(x => x != '\n' && x != '\r' && x != ' ');
        }
    }
}