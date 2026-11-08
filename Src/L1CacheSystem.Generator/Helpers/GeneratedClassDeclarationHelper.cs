using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace L1CacheSystem.Generator.Helpers
{
    internal static class GeneratedClassDeclarationHelper
    {
        public static ISymbol GetDeclaredSymbol(this SyntaxNode node, Compilation compilation)
        {
            var model = compilation.GetSemanticModel(node.SyntaxTree);
            return model.GetDeclaredSymbol(node);
        }

        internal static bool IsPatrial(this INamedTypeSymbol type)
        {
            foreach (var item in type.DeclaringSyntaxReferences)
            {
                var syntax = item.GetSyntax();
                if (!(syntax is ClassDeclarationSyntax classDeclarationSyntax))
                {
                    continue;
                }

                if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool GCIsStatic(this INamedTypeSymbol type)
        {
            foreach (var item in type.DeclaringSyntaxReferences)
            {
                var syntax = item.GetSyntax();
                if (!(syntax is ClassDeclarationSyntax classDeclarationSyntax))
                {
                    continue;
                }

                if (classDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                {
                    return true;
                }
            }

            return !type.IsPatrial();
        }

        internal static string GCPartialWordOrEmpty(this INamedTypeSymbol type)
        {
            return type.IsPatrial() ? "partial" : "";
        }
    }
}