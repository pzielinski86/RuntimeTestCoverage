﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TestCoverage.Storage;

namespace TestCoverage.Rewrite
{
    public class SolutionRewriter
    {
        private readonly IRewrittenDocumentsStorage _rewrittenDocumentsStorage;
        private readonly IAuditVariablesRewriter _auditVariablesRewriter;

        public SolutionRewriter(IRewrittenDocumentsStorage rewrittenDocumentsStorage, IAuditVariablesRewriter auditVariablesRewriter)
        {
            _rewrittenDocumentsStorage = rewrittenDocumentsStorage;
            _auditVariablesRewriter = auditVariablesRewriter;
        }

        public RewrittenDocument RewriteDocumentWithAssemblyInfo(Project currentProject, Project[] allProjects, string documentPath, string documentContent)
        {
            var attrs = CreateInternalVisibleToAttributeList(currentProject, allProjects);
            var rewrittenDocument = RewriteDocument(currentProject, documentPath, documentContent, attrs);

            return rewrittenDocument;
        }

        private RewrittenDocument RewriteDocument(Project project, string documentPath, string documentContent, AttributeListSyntax attrs)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(documentContent, CSharpParseOptions.Default.WithPreprocessorSymbols("FRAMEWORK"));
            SyntaxNode syntaxNode = syntaxTree.GetRoot();

            var rewrittenDocument = _auditVariablesRewriter.Rewrite(project.Name, documentPath, syntaxNode);

            if (attrs != null)
                rewrittenDocument.AddAttributeLists(attrs);

            _rewrittenDocumentsStorage.Store(project.Solution.FilePath, project.Name, documentPath, rewrittenDocument.SyntaxTree.GetRoot());

            return rewrittenDocument;
        }

        public RewriteResult RewriteAllClasses(IEnumerable<Project> projects)
        {
            var rewrittenItems = new Dictionary<Project, List<RewrittenDocument>>();
            var allProjects = projects.ToArray();

            foreach (Project project in allProjects)
            {
                _rewrittenDocumentsStorage.Clear(project.Name);

                var internalVisibleToAttrDoc = CreateInternalVisibleToAttributeList(project, allProjects);
                int i = 0;

                foreach (Document document in project.Documents)
                {
                    // TODO: Find a better way to avoid conflicts for system classes (AuditVariablesAutoGenerated941C, AuditVariable)
                    if (document.Name == "InternalTypes.cs")
                        continue;

                    SyntaxNode syntaxNode = document.GetSyntaxRootAsync().Result;

                    // attach InternalsVisibleToAttribute only to the first document
                    var attributes = i == 0 ? internalVisibleToAttrDoc : null;

                    RewrittenDocument rewrittenDocument = RewriteDocument(project, document.FilePath, syntaxNode.ToFullString(), attributes);

                    if (!rewrittenItems.ContainsKey(document.Project))
                        rewrittenItems[document.Project] = new List<RewrittenDocument>();

                    rewrittenItems[document.Project].Add(rewrittenDocument);
                    i++;
                }
            }

            return new RewriteResult(rewrittenItems);
        }

        private AttributeSyntax CreateInternalVisibleToAttribute(Project project)
        {
            var name = SyntaxFactory.ParseName("System.Runtime.CompilerServices.InternalsVisibleTo");
            var arguments = SyntaxFactory.ParseAttributeArgumentList($"(\"{PathHelper.GetCoverageDllName(project.Name)}\")");
            var attribute = SyntaxFactory.Attribute(name, arguments);

            return attribute;
        }

        private AttributeListSyntax CreateInternalVisibleToAttributeList(Project project, Project[] allProjects)
        {
            var testProjects = allProjects.Where(x => x.ProjectReferences.Any(y => y.ProjectId == project.Id)).ToArray();
            if (testProjects.Length == 0)
                return null;

            List<AttributeSyntax> attrs = testProjects.Select(CreateInternalVisibleToAttribute).ToList();


            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.AddRange(attrs);

            var assemblyLevelSpecifier =
                SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword));

            return SyntaxFactory.AttributeList(assemblyLevelSpecifier, attributeList);
        }
    }
}