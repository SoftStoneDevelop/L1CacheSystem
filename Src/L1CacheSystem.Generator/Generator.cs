using L1CacheSystem.Generator.Comparators;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace L1CacheSystem.Generator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // System.Diagnostics.Debugger.Launch();

            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                predicate: (s, _) => IsSyntaxTargetForGeneration(s),
                transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(m => m != null)
                .Collect()
                .Select((sel, _) => sel.Distinct())
                .SelectMany(
                (sel, _) =>
                    sel
                    .GroupBy(gr => gr.Identifier.ValueText)
                    .Select(grSel => grSel.ToImmutableArray()));

            var compilationAndClasses =
                classDeclarations.Combine(context.CompilationProvider)
                .WithComparer(ByArrayComparer.Default);

            context.RegisterSourceOutput(compilationAndClasses,
                (spc, source) => Execute(source.Item2, source.Item1, spc));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0;

        private static TypeDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var typeDeclaration = GetSemanticClass(context);

            return typeDeclaration;
        }

        private static TypeDeclarationSyntax GetSemanticClass(GeneratorSyntaxContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax))
            {
                return null;
            }

            var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;
            foreach (var attributeListSyntax in typeDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    IMethodSymbol attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
                    if (attributeSymbol == null)
                    {
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                    if (attributeContainingTypeSymbol.ContainingNamespace.GetFullNamespace().StartsWith("L1CacheSystem.Attributes"))
                    {
                        return typeDeclarationSyntax;
                    }
                }
            }

            return null;
        }

        //private static int _counter;

        private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> partialGroup, SourceProductionContext context)
        {
            // System.Diagnostics.Debugger.Launch();

            if (!partialGroup.Any())
            {
                return;
            }

            //var incremented = Interlocked.Increment(ref _counter);
            //if (partialGroup.First().Identifier.ValueText == "Tests_BigInteger_numeric")
            //{
            //   context.AddSource($"Aperf.cs", $@"//
            //// Counter: {incremented}
            //");
            //}

            var processor = new AttributeProcessor();
            foreach (var typeDeclarationSyntax in partialGroup)
            {
                processor.TryFillFrom(
                    context,
                    typeDeclarationSyntax,
                    compilation,
                    (INamedTypeSymbol)compilation.GetSemanticModel(typeDeclarationSyntax.SyntaxTree).GetDeclaredSymbol(typeDeclarationSyntax),
                    context.CancellationToken);
            }

            processor.GenerateAndSaveMethods(context, context.CancellationToken);
        }
    }
}