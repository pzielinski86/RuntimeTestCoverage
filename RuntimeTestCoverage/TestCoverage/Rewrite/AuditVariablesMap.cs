﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TestCoverage.Rewrite
{
    public static class AuditVariablesMap
    {
        public static string GenerateVariablesListSourceCode()
        {
            StringBuilder classBuilder = new StringBuilder();

            classBuilder.AppendLine(string.Format("public static class {0}", AuditVariablesListClassName));
            classBuilder.AppendLine("{");

            string list = $"\tpublic static System.Collections.Generic.List<{AuditVariablePlaceholder.AuditVariableStructureName}> {AuditVariablesListName} = new  System.Collections.Generic.List<{AuditVariablePlaceholder.AuditVariableStructureName}>();";
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
            get { return "AuditVariables"; }
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
            codeBuilder.AppendLine("public System.String NodePath, DocumentPath;");
            codeBuilder.AppendLine("public int Span;");
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }
    }
}