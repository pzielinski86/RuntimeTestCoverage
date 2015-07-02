using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using TestCoverage.Compilation;

namespace TestCoverage
{
    public class RewriteResult
    {
        private readonly Dictionary<Project, List<RewrittenItemInfo>> _items;
        private readonly AuditVariablesMap _auditVariablesMap;

        public RewriteResult(Dictionary<Project, List<RewrittenItemInfo>> items, AuditVariablesMap auditVariablesMap)
        {
            _items = items;
            _auditVariablesMap = auditVariablesMap;
        }

        public Dictionary<Project, List<RewrittenItemInfo>> Items
        {
            get { return _items; }
        }

        public AuditVariablesMap AuditVariablesMap
        {
            get { return _auditVariablesMap; }
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