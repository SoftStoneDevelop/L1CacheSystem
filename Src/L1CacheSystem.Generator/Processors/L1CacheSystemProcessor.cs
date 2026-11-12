using L1CacheSystem.Generator.AttributeInfo;
using L1CacheSystem.Generator.Constants;
using L1CacheSystem.Generator.Generators;
using L1CacheSystem.Generator.Generators.Column;
using L1CacheSystem.Generator.Generators.Entity;
using L1CacheSystem.Generator.Generators.Lock;
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
        private readonly List<CacheInfo> _cacheInfos = new List<CacheInfo>();
        private readonly List<ColumnCacheInfo> _columnCacheInfos = new List<ColumnCacheInfo>();
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

                var tempColumnCache = new ColumnCacheInfo();
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

                    if (attributeData.AttributeClass.IsAssignableFrom(AttributeConstants.AttributeNamespace, AttributeConstants.KeyColumnCacheAttributeName))
                    {
                        if (!KeyColumnInfo.Create(context, attributeData.ConstructorArguments, containsType, out var keyColumnInfo))
                        {
                            return;
                        }

                        tempColumnCache.KeyInfo = keyColumnInfo;
                        continue;
                    }

                    if (attributeData.AttributeClass.IsAssignableFrom(AttributeConstants.AttributeNamespace, AttributeConstants.ColumnCacheAttributeName))
                    {
                        if (!ColumnInfo.Create(context, attributeData.ConstructorArguments, containsType, out var columnInfo))
                        {
                            return;
                        }

                        tempColumnCache.ColumnInfos.Add(columnInfo);
                        continue;
                    }
                }

                if (!tempColumnCache.IsEmpty())
                {
                    _columnCacheInfos.Add(tempColumnCache);
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
                        $"Duplicate cache name '{cacheInfo.CacheName}'",
                        DiagnosticSeverity.Error);
                    return;
                }
            }

            foreach (var columnCacheInfo in _columnCacheInfos)
            {
                if (!cacheNames.Add(columnCacheInfo.KeyInfo.CacheName))
                {
                    DiagnosticHelper.ReportDiagnostic(
                        context,
                        DiagnosticConsts.CacheNameDuplicate,
                        $"Duplicate cache name '{columnCacheInfo.KeyInfo.CacheName}'",
                        DiagnosticSeverity.Error);
                    return;
                }
            }

            var builder = new StringBuilder();
            var interfaceGeneratorParams = new Dictionary<INamedTypeSymbol, List<CacheInterfaceGenerator.Params>>(SymbolEqualityComparer.Default);
            var commonGeneratorParams = new List<CacheCommonMethodsGenerator.ParamsBase>();

            builder.Clear();
            EntityCache(builder, interfaceGeneratorParams, commonGeneratorParams, context, cancellationToken);

            builder.Clear();
            ColumnCache(builder, interfaceGeneratorParams, commonGeneratorParams, context, cancellationToken);

            var interfaceGenerator = new CacheInterfaceGenerator();
            foreach (var interfaceGeneratorParam in interfaceGeneratorParams)
            {
                cancellationToken.ThrowIfCancellationRequested();

                builder.Clear();
                interfaceGenerator.GenerateAndSave(
                    context,
                    interfaceGeneratorParam.Key,
                    interfaceGeneratorParam.Value.ToArray(),
                    builder,
                    "",
                    $"{interfaceGeneratorParam.Key.Name}");
            }

            builder.Clear();
            var commonMethodGenerator = new CacheCommonMethodsGenerator();
            commonMethodGenerator.GenerateAndSave(
                context,
                _containsType,
                commonGeneratorParams.ToArray(),
                builder,
                $"{_containsType.Name}");
        }

        private void ColumnCache(
            StringBuilder builder,
            Dictionary<INamedTypeSymbol, List<CacheInterfaceGenerator.Params>> interfaceGeneratorParams,
            List<CacheCommonMethodsGenerator.ParamsBase> commonGeneratorParams,
            SourceProductionContext context,
            CancellationToken cancellationToken)
        {
            var cacheGenerator = new ColumnCacheMethodsGenerator();
            var lockGenerator = new LockMethodsGenerator();

            foreach (var columnCacheInfo in _columnCacheInfos)
            {
                cancellationToken.ThrowIfCancellationRequested();

                builder.Clear();
                cacheGenerator.GenerateAndSave(
                    context,
                    columnCacheInfo,
                    builder,
                    $"{_containsType.Name}");

                var lockParam = new LockMethodsGenerator.Param
                {
                    CacheName = columnCacheInfo.KeyInfo.CacheName,
                    ContainsType = columnCacheInfo.KeyInfo.ContainsType,
                    KeyType = columnCacheInfo.KeyInfo.KeyType,
                    MetricCollectFlags = columnCacheInfo.KeyInfo.MetricCollectFlags
                };

                builder.Clear();
                lockGenerator.GenerateAndSave(
                    context,
                    lockParam,
                    builder,
                    $"{_containsType.Name}");

                commonGeneratorParams.Add(new CacheCommonMethodsGenerator.ParamsColumn
                {
                    GetLockKeyMthodName = LockMethodsGenerator.GetLockKeyMethodName(columnCacheInfo.KeyInfo.CacheName),
                    CacheTypeName = columnCacheInfo.KeyInfo.CacheName,
                    MetricCollectFlags = columnCacheInfo.KeyInfo.MetricCollectFlags,
                    RemoveExpiredMethodName = ColumnCacheMethodsGenerator.RemoveExpiredMethodName(columnCacheInfo.KeyInfo.CacheName),
                    CollectionsToClearNames = ColumnCacheMethodsGenerator.CollectionsToClearNames(columnCacheInfo).ToArray(),
                    DisposedResourcesNames = ColumnCacheMethodsGenerator.DisposedResourcesNames(columnCacheInfo).ToArray()

                });

                foreach (var declaration in cacheGenerator.GetPublicMethodDeclarations(columnCacheInfo))
                {
                    if (!interfaceGeneratorParams.TryGetValue(columnCacheInfo.KeyInfo.InterfaceType, out var parameters))
                    {
                        parameters = new List<CacheInterfaceGenerator.Params>(_cacheInfos.Count * CacheMethodsGenerator.PublicMethodCount);
                        interfaceGeneratorParams[columnCacheInfo.KeyInfo.InterfaceType] = parameters;
                    }

                    parameters.Add(new CacheInterfaceGenerator.Params
                    {
                        MethodDeclaration = declaration
                    });
                }

                foreach (var declaration in lockGenerator.GetPublicMethodDeclarations(lockParam))
                {
                    if (!interfaceGeneratorParams.TryGetValue(columnCacheInfo.KeyInfo.InterfaceType, out var parameters))
                    {
                        parameters = new List<CacheInterfaceGenerator.Params>(_cacheInfos.Count * CacheMethodsGenerator.PublicMethodCount);
                        interfaceGeneratorParams[columnCacheInfo.KeyInfo.InterfaceType] = parameters;
                    }

                    parameters.Add(new CacheInterfaceGenerator.Params
                    {
                        MethodDeclaration = declaration
                    });
                }
            }
        }

        private void EntityCache(
            StringBuilder builder,
            Dictionary<INamedTypeSymbol, List<CacheInterfaceGenerator.Params>> interfaceGeneratorParams,
            List<CacheCommonMethodsGenerator.ParamsBase> commonGeneratorParams,
            SourceProductionContext context,
            CancellationToken cancellationToken)
        {
            var cacheGenerator = new CacheMethodsGenerator();
            var lockGenerator = new LockMethodsGenerator();
            foreach (var cacheInfo in _cacheInfos)
            {
                cancellationToken.ThrowIfCancellationRequested();

                builder.Clear();
                cacheGenerator.GenerateAndSave(
                    context,
                    cacheInfo,
                    builder,
                    $"{_containsType.Name}");

                var lockParam = new LockMethodsGenerator.Param
                {
                    CacheName = cacheInfo.CacheName,
                    ContainsType = cacheInfo.ContainsType,
                    KeyType = cacheInfo.KeyType,
                    MetricCollectFlags = cacheInfo.MetricCollectFlags
                };

                builder.Clear();
                lockGenerator.GenerateAndSave(
                    context,
                    lockParam,
                    builder,
                    $"{_containsType.Name}");

                commonGeneratorParams.Add(new CacheCommonMethodsGenerator.ParamsEntity
                {
                    ItemsCollectionName = cacheGenerator.CollectionName(cacheInfo),
                    GetLockKeyMthodName = LockMethodsGenerator.GetLockKeyMethodName(lockParam.CacheName),
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

                foreach (var declaration in lockGenerator.GetPublicMethodDeclarations(lockParam))
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
        }

        public override void Reset()
        {
            _cacheInfos.Clear();
            _containsType = null;
        }
    }
}