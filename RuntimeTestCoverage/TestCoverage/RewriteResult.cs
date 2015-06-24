namespace TestCoverage
{
    public class RewriteResult
    {
        private readonly RewrittenItemInfo[] _items;
        private readonly AuditVariablesMap _auditVariablesMap;

        public RewriteResult(RewrittenItemInfo[] items, AuditVariablesMap auditVariablesMap)
        {
            _items = items;
            _auditVariablesMap = auditVariablesMap;
        }

        public RewrittenItemInfo[] Items
        {
            get { return _items; }
        }

        public AuditVariablesMap AuditVariablesMap
        {
            get { return _auditVariablesMap; }
        }
    }
}