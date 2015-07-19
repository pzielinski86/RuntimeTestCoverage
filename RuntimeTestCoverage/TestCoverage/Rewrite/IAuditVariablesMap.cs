using System.Collections.Generic;

namespace TestCoverage.Rewrite
{
    public interface IAuditVariablesMap
    {
        string AddVariable(AuditVariablePlaceholder auditVariablePlaceholder);
        string GenerateSourceCode();
        string AuditVariablesClassName { get; }
        string AuditVariablesDictionaryName { get; }
        Dictionary<string, AuditVariablePlaceholder> Map { get; }
    }
}