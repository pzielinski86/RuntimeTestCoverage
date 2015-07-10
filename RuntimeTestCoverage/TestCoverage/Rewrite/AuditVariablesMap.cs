using System;
using System.Collections.Generic;
using System.Text;

namespace TestCoverage.Rewrite
{
    public class AuditVariablesMap
    {
        private readonly Dictionary<string, AuditVariablePlaceholder> _map = new Dictionary<string, AuditVariablePlaceholder>();

        public string AddVariable(AuditVariablePlaceholder auditVariablePlaceholder)
        {
            string varName = string.Format("{0}_{1}", auditVariablePlaceholder.NodePath, auditVariablePlaceholder.SpanStart);

            _map[varName] = auditVariablePlaceholder;

            return varName;
        }
           
        public override string ToString()
        {
            return GenerateAuditClass();
        }

        public string AuditVariablesClassName
        {
            get { return "AuditVariables"; }
        }
        public string AuditVariablesDictionaryName
        {
            get { return "Coverage"; }
        }

        public Dictionary<string, AuditVariablePlaceholder> Map
        {
            get { return _map; }
        }

        public static string ExtractPathFromVariableName(string varName)
        {
            for (int i = varName.Length - 1; i >= 0; i--)
            {
                if (varName[i] == '_')
                {
                    return varName.Substring(0, i);
                }
            }

            throw new ArgumentException("Passed argument is not audit variable.");
        }

        public static int ExtractSpanFromVariableName(string varName)
        {
            for (int i = varName.Length - 1; i >= 0; i--)
            {
                if (varName[i] == '_')
                {
                    return Int32.Parse(varName.Substring(i+1, varName.Length - i-1));
                }
            }

            throw new ArgumentException("Passed argument is not audit variable.");
        }

        private string GenerateAuditClass()
        {
            StringBuilder classBuilder = new StringBuilder();

            classBuilder.AppendLine(string.Format("public static class {0}", AuditVariablesClassName));
            classBuilder.AppendLine("{");

            classBuilder.AppendLine(string.Format("\tpublic static System.Collections.Generic.Dictionary<string,bool> {0} = new  System.Collections.Generic.Dictionary<string,bool>();", AuditVariablesDictionaryName));

            classBuilder.AppendLine("}");

            return classBuilder.ToString();
        }    
    }
}