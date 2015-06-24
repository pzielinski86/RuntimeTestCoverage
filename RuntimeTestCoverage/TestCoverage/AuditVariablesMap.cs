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

        public string AddVariable(int position,SyntaxNode node)
        {
            string varName = "a" + _map.Count;

            _map[varName] = position;

            return varName;
        }

        private string GetNodePath(SyntaxNode node)
        {
            return node.Span.Start.ToString();
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