using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TestCoverage
{
    public class AuditVariablesMap
    {
        private readonly Dictionary<string, int> _map = new Dictionary<string, int>();

        public string AddVariable(int position, string documentName, SyntaxNode node)
        {
            string varName = string.Format("{0}_{1}", documentName, _map.Count);

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

        public void ClearByDocumentName(string documentName)
        {
            var keysToRemove = new List<string>();

            foreach (string key in _map.Keys)
            {
                if (key.StartsWith(documentName))
                    keysToRemove.Remove(key);
            }

            foreach (string key in keysToRemove)
                _map.Remove(key);
        }
    }
}