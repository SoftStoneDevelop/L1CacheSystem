using L1CacheSystem.Generator.Processors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace L1CacheSystem.Generator
{
    internal class AttributeProcessor
    {
        private readonly L1CacheSystemProcessor _l1CacheSystemProcessor = new L1CacheSystemProcessor();

        public void TryFillFrom(
            SourceProductionContext context,
            TypeDeclarationSyntax type,
            Compilation compilation,
            INamedTypeSymbol containsType,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProcessAttributes(context, type.AttributeLists, compilation, containsType, cancellationToken);
        }

        private void ProcessAttributes(
            SourceProductionContext context,
            SyntaxList<AttributeListSyntax> attributes,
            Compilation compilation,
            INamedTypeSymbol containsType,
            CancellationToken cancellationToken)
        {
            _l1CacheSystemProcessor.ProcessAttributes(context, attributes, compilation, containsType, cancellationToken);
        }

        public void GenerateAndSaveMethods(SourceProductionContext context, CancellationToken cancellationToken)
        {
            _l1CacheSystemProcessor.GenerateAndSaveMethods(context, cancellationToken);
        }
    }
}
