﻿using System.Text;

namespace TestCoverage.Rewrite
{
    public static class AuditVariablesMap
    {
        public static string GenerateVariablesListSourceCode()
        {
            StringBuilder classBuilder = new StringBuilder();

            classBuilder.AppendLine(string.Format("public static class {0}", AuditVariablesListClassName));
            classBuilder.AppendLine("{");

            string list = $"\tpublic static System.Collections.Generic.HashSet<{AuditVariablePlaceholder.AuditVariableStructureName}> {AuditVariablesListName} = new  System.Collections.Generic.HashSet<{AuditVariablePlaceholder.AuditVariableStructureName}>();";
            classBuilder.AppendLine(list);

            classBuilder.Append("}");

            return classBuilder.ToString();
        }

        public static string GenerateCode()
        {
            return GenerateAuditVariableSourceCode() + GenerateVariablesListSourceCode();
        }

        public static string AuditVariablesListClassName
        {
            get { return "AuditVariablesAutoGenerated941C"; }
        }

        public static string AuditVariablesListName
        {
            get { return "Coverage"; }
        }

        public static string GenerateAuditVariableSourceCode()
        {
            StringBuilder codeBuilder = new StringBuilder();
            codeBuilder.AppendLine($"public struct {AuditVariablePlaceholder.AuditVariableStructureName}");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("public int Span;");
            codeBuilder.AppendLine("public System.String DocumentPath,NodePath;");

            codeBuilder.AppendLine(@"        public override bool Equals(object obj)");
            codeBuilder.AppendLine(@"        {");
            codeBuilder.AppendLine(@"            return Equals((AuditVariable)obj);");
            codeBuilder.AppendLine(@"        }");
            codeBuilder.AppendLine(@"");
            codeBuilder.AppendLine(@"        public bool Equals(AuditVariable other)");
            codeBuilder.AppendLine(@"        {");
            codeBuilder.AppendLine(@"            return Span == other.Span && string.Equals(DocumentPath, other.DocumentPath);");
            codeBuilder.AppendLine(@"        }");
            codeBuilder.AppendLine(@"");
            codeBuilder.AppendLine(@"        public override int GetHashCode()");
            codeBuilder.AppendLine(@"        {");
            codeBuilder.AppendLine(@"            unchecked");
            codeBuilder.AppendLine(@"            {");
            codeBuilder.AppendLine(@"                return (Span*397) ^ (DocumentPath != null ? DocumentPath.GetHashCode() : 0);");
            codeBuilder.AppendLine(@"            }");
            codeBuilder.AppendLine(@"        }");

            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }
    }
}