using L1CacheSystem.Generator.AttributeInfo;
using L1CacheSystem.Generator.Constants;
using L1CacheSystem.Generator.Generators;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace L1CacheSystem.Generator.Processors
{
    internal class L1CacheSystemProcessor : BaseAttributeProcessor
    {
        private List<CacheInfo> _cacheInfos = new List<CacheInfo>();
        private INamedTypeSymbol _containsType;

        public override void ProcessAttributes(
            SourceProductionContext context,
            SyntaxList<AttributeListSyntax> attributes,
            Compilation compilation,
            INamedTypeSymbol containsType,
            CancellationToken cancellationToken)
        {
            _containsType = containsType;
            foreach (var attributeListSyntax in attributes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var parentSymbol = attributeListSyntax.Parent.GetDeclaredSymbol(compilation);
                var parentAttributes = parentSymbol.GetAttributes();

                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    var attributeData = parentAttributes.First(f => f.ApplicationSyntaxReference.GetSyntax() == attributeSyntax);
                    if (attributeData.AttributeClass.IsAssignableFrom(AttributeConstants.AttributeNamespace, AttributeConstants.EntityCacheTypeAttributeName))
                    {
                        if (!CacheInfo.Create(context, attributeData.ConstructorArguments, containsType, out var cacheInfo))
                        {
                            return;
                        }

                        _cacheInfos.Add(cacheInfo);
                        continue;
                    }
                }
            }
        }

        public override void GenerateAndSaveMethods(SourceProductionContext context, CancellationToken cancellationToken)
        {
            var cacheNames = new HashSet<string>();
            foreach (var cacheInfo in _cacheInfos)
            {
                if (!cacheNames.Add(cacheInfo.CacheName))
                {
                    DiagnosticHelper.ReportDiagnostic(
                        context,
                        DiagnosticConsts.CacheNameDuplicate,
                        "Duplicate cache name",
                        DiagnosticSeverity.Error);
                    return;
                }
            }

            var cacheGenerator = new CacheMethodsGenerator();
            var builder = new StringBuilder();
            var commonGeneratorParams = new List<CacheCommonMethodsGenerator.Params>(_cacheInfos.Count);
            var interfaceGeneratorParams = new Dictionary<INamedTypeSymbol, List<CacheInterfaceGenerator.Params>>(SymbolEqualityComparer.Default);
            foreach (var cacheInfo in _cacheInfos)
            {
                builder.Clear();
                cacheGenerator.GenerateAndSave(
                    context,
                    cacheInfo,
                    builder,
                    "",
                    $"{_containsType.Name}");
                commonGeneratorParams.Add(new CacheCommonMethodsGenerator.Params
                {
                    ItemsCollectionName = cacheGenerator.CollectionName(cacheInfo),
                    GetLockKeyMthodName = cacheGenerator.GetLockKeyMethodName(cacheInfo),
                    CacheTypeName = cacheInfo.CacheName,
                    MetricCollectFlags = cacheInfo.MetricCollectFlags
                });

                foreach (var declaration in cacheGenerator.GetPublicMethodDeclarations(cacheInfo))
                {
                    if (!interfaceGeneratorParams.TryGetValue(cacheInfo.InterfaceType, out var parameters))
                    {
                        parameters = new List<CacheInterfaceGenerator.Params>(_cacheInfos.Count * CacheMethodsGenerator.PublicMethodCount);
                        interfaceGeneratorParams[cacheInfo.InterfaceType] = parameters;
                    }

                    parameters.Add(new CacheInterfaceGenerator.Params
                    {
                        MethodDeclaration = declaration
                    });
                }
            }

            builder.Clear();
            var commonMethodGenerator = new CacheCommonMethodsGenerator();
            commonMethodGenerator.GenerateAndSave(
                context,
                _containsType,
                commonGeneratorParams.ToArray(),
                builder,
                "",
                $"{_containsType.Name}");

            var interfaceGenerator = new CacheInterfaceGenerator();
            foreach (var interfaceGeneratorParam in interfaceGeneratorParams)
            {
                builder.Clear();
                interfaceGenerator.GenerateAndSave(
                    context,
                    interfaceGeneratorParam.Key,
                    interfaceGeneratorParam.Value.ToArray(),
                    builder,
                    "",
                    $"{interfaceGeneratorParam.Key.Name}");
            }
        }

        public override void Reset()
        {
            _cacheInfos.Clear();
            _containsType = null;
        }
    }
}