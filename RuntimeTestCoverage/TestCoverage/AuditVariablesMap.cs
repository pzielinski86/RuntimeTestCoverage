using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public class AuditVariablesMap
    {
        private readonly Dictionary<string, int> _map = new Dictionary<string, int>();

        public string AddVariable(string path, int position)
        {
            string varName = string.Format("{0}_{1}", path, _map.Count);

            _map[varName] = position;

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

        public Dictionary<string, int> Map
        {
            get { return _map; }
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