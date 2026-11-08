using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace L1CacheSystem.Generator.Processors
{
    internal abstract class BaseAttributeProcessor
    {
        public abstract void Reset();

        public abstract void ProcessAttributes(
            SourceProductionContext context,
            SyntaxList<AttributeListSyntax> attributes,
            Compilation compilation,
            INamedTypeSymbol containsType,
            CancellationToken cancellationToken);

        public abstract void GenerateAndSaveMethods(
            SourceProductionContext context,
            CancellationToken cancellationToken);
    }
}