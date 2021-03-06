﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace TestCoverage.Extensions
{
    public static class SyntaxNodeExtensions
    {
        public static string[] GetUsedNamespaces(this SyntaxNode node)
        {
            return node.Ancestors().OfType<CompilationUnitSyntax>().First().DescendantNodes().
              OfType<UsingDirectiveSyntax>().Select(x => x.Name.ToString()).ToArray();
        }
        public static MethodDeclarationSyntax[] GetPublicMethods(this SyntaxNode node)
        {
            return
                node.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(y => y.ValueText == "public"))
                    .ToArray();
        }

        public static string[] GetAllMethodNames(this SyntaxNode node)
        {
            return
                node.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>().Select(x=>x.Identifier.ValueText)
                    .ToArray();
        }

        public static MemberDeclarationSyntax GetParentMethod(this SyntaxNode node)
        {
            return node.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
        }

        public static ClassDeclarationSyntax GetParentClass(this SyntaxNode node)
        {
            return node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        }
        public static MethodDeclarationSyntax GetMethodAt(this SyntaxNode root, int position)
        {
            var method =
                root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>().
                    FirstOrDefault(x => x.Span.Start < position && x.Span.End > position);

            return method;
        }

        public static ClassDeclarationSyntax GetClassDeclarationSyntax(this SyntaxNode node)
        {
            return node.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
        }

        public static ClassDeclarationSyntax[] GetClassDeclarations(this SyntaxNode node)
        {
            return node.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
        }
    }
}