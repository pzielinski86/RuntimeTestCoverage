using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;

namespace TestCoverage.Rewrite
{
    public class RewriteResult
    {
        private readonly Dictionary<Project, List<RewrittenDocument>> _items;

        public RewriteResult(Dictionary<Project, List<RewrittenDocument>> items)
        {
            _items = items;
        }

        public Dictionary<Project, List<RewrittenDocument>> Items
        {
            get { return _items; }
        }

        public IEnumerable<CompilationItem> ToCompilationItems()
        {
            foreach (var project in Items.Keys)
            {
                yield return  new CompilationItem(project,Items[project].Select(i=>i.SyntaxTree).ToArray());
            }
        }
    }
}