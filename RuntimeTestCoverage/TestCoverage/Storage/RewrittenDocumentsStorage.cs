using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dapper;
using ErikEJ.SqlCe;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestCoverage.Storage
{
    public class RewrittenDocumentsStorage : IRewrittenDocumentsStorage
    {
        public IEnumerable<SyntaxTree> GetRewrittenDocuments(string solutionPath, string projectName, params string[] excludedDocuments)
        {
            string folder = GetProjectFolder(projectName);

            var rewrittenDocumentsToExclude = excludedDocuments.
                Select(x => Path.Combine(folder, GetDocumentFileName(solutionPath, x)));

            foreach (string file in Directory.GetFiles(folder).Where(x => !rewrittenDocumentsToExclude.Contains(x)))
            {
                var code = File.ReadAllText(file);

                yield return CSharpSyntaxTree.ParseText(code);
            }
        }

        public void Store(string solutionPath, string projectName, string docPath, SyntaxNode documentContent)
        {
            var docName = GetDocumentFileName(solutionPath, docPath);
            string folder = GetProjectFolder(projectName);
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, docName);

            using (var stream = new StreamWriter(File.Create(path)))
            {
                documentContent.WriteTo(stream);
            }
        }

        public void Clear(string projectName)
        {
            var folderPath = GetProjectFolder(projectName);

            if (Directory.Exists(folderPath))
                Directory.Delete(folderPath, true);
        }

        public void RemoveByDocument(string docPath, string projectName,string solutionPath)
        {
            var docName = GetDocumentFileName(solutionPath, docPath);
            string folder = GetProjectFolder(projectName);
            string path = Path.Combine(folder, docName);

            File.Delete(path);
        }

        private string GetDocumentFileName(string solutionPath, string docPath)
        {
            string docRelativePathToSolution = MakeRelative(docPath, Path.GetDirectoryName(solutionPath));

            string docName = docRelativePathToSolution.Replace("/", "_");
            return docName;
        }

        private static string GetProjectFolder(string projectName)
        {
            return Path.Combine(Config.WorkingDirectory, "RewrittenProjects", projectName);
        }

        private string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);

            return referenceUri.MakeRelativeUri(fileUri).ToString();
        }
    }
}