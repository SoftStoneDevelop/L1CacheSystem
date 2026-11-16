using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Generators.Lock;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Threading;

namespace L1CacheSystem.Generator.Generators
{
    internal class CacheCommonMethodsGenerator
    {
        public void GenerateAndSave(
            SourceProductionContext context,
            INamedTypeSymbol containsType,
            ParamsBase[] inputParams, 
            StringBuilder builder,
            string partName)
        {
            Usings(builder);
            Namespace(builder, containsType);
            StartClass(builder, containsType);

            DisposeGeneratedMethod(builder, inputParams);
            RemoveExpiredGeneratedMethod(builder, inputParams);

            EndClass(builder);

            context.AddSource($"{partName}CommonMethods.g.cs", builder.ToString());
        }

        private void Usings(StringBuilder builder)
        {
            builder.Append(@"
using Microsoft.Extensions.Logging;
using L1CacheSystem.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
");
        }

        private void Namespace(StringBuilder builder, INamedTypeSymbol containsType)
        {
            builder.Append($@"
namespace {containsType.ContainingNamespace};
");
        }

        private void StartClass(StringBuilder builder, INamedTypeSymbol containsType)
        {
            builder.Append($@"
public partial class {containsType.Name}
{{
");
        }

        private void DisposeGeneratedMethod(StringBuilder builder, ParamsBase[] inputParams)
        {
            builder.Append($@"
    protected override void DisposeGenerated()
    {{");
            foreach (var inputParam in inputParams)
            {
                builder.Append($@"
        try
        {{
            foreach (var keyLock in {LockMethodsGenerator.GetLockDicName(inputParam.CacheTypeName)})
            {{
                keyLock.Value.Dispose();
            }}

            {LockMethodsGenerator.GetLockDicName(inputParam.CacheTypeName)}.Clear();
");
                if (inputParam is ParamsEntity paramsEntity)
                {
                    builder.Append($@"
            {paramsEntity.ItemsCollectionName}.Clear();");
                }
                else if (inputParam is ParamsColumn paramsColumn)
                {
                    foreach (var toDisposeName in paramsColumn.DisposedResourcesNames)
                    {
                        builder.Append($@"
            {toDisposeName}.Dispose();");
                    }

                    foreach (var toClearName in paramsColumn.CollectionsToClearNames)
                    {
                        builder.Append($@"
            {toClearName}.Clear();");
                    }
                }

                builder.Append($@"
        }}
        catch
        {{
            // ignore
        }}
");
            }

            builder.Append($@"
    }}
");
        }

        private void RemoveExpiredGeneratedMethod(StringBuilder builder, ParamsBase[] inputParams)
        {
            builder.Append($@"
    protected override void RemoveExpiredGenerated()
    {{
        var currentTime = _timeProvider.GetUtcNow();");
            foreach (var inputParam in inputParams)
            {
                string collectMetricsStr = inputParam.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount) ? "true" : "false";
                builder.Append($@"
        // If no one has touched the cache and, based on the latest scan,
        // it's not yet time to clear anything out,
        // then we leave the cache alone.
        var haveChanges{inputParam.CacheTypeName} = Interlocked.CompareExchange(location1: ref {LockMethodsGenerator.HaveChangesVarName(inputParam.CacheTypeName)}, value: 0, comparand: 1);
        if (haveChanges{inputParam.CacheTypeName} == 1 || currentTime > {LockMethodsGenerator.MinExpiredDateName(inputParam.CacheTypeName)})
        {{
");
                if (inputParam is ParamsEntity paramsEntity)
                {
                    builder.Append($@"
            {LockMethodsGenerator.MinExpiredDateName(inputParam.CacheTypeName)} = RemoveExpired(
                {paramsEntity.ItemsCollectionName},
                {LockMethodsGenerator.GetLockDicName(inputParam.CacheTypeName)},
                {paramsEntity.GetLockKeyMthodName},
                ""{paramsEntity.CacheTypeName}"",
                {collectMetricsStr});
");
                }
                else if (inputParam is ParamsColumn paramsColumn)
                {
                    builder.Append($@"
            {LockMethodsGenerator.MinExpiredDateName(inputParam.CacheTypeName)} = {paramsColumn.RemoveExpiredMethodName}({LockMethodsGenerator.GetLockDicName(inputParam.CacheTypeName)});");
                }

                builder.Append($@"
        }}

        var haveLockChanges{inputParam.CacheTypeName} = Interlocked.CompareExchange(location1: ref {LockMethodsGenerator.HaveLockChangesVarName(inputParam.CacheTypeName)}, value: 0, comparand: 1);
        if (haveLockChanges{inputParam.CacheTypeName} == 1 || currentTime > {LockMethodsGenerator.MinLockExpiredDateName(inputParam.CacheTypeName)})
        {{
            {LockMethodsGenerator.MinLockExpiredDateName(inputParam.CacheTypeName)} = RemoveUnusedLocks({LockMethodsGenerator.GetLockDicName(inputParam.CacheTypeName)});
        }}
");
            }

            builder.Append($@"
    }}
");
        }

        private void EndClass(StringBuilder builder)
        {
            builder.Append($@"
}}
");
        }

        public abstract class ParamsBase
        {
            /// <summary>
            /// The name of the method responsible for retrieving the lock name
            /// </summary>
            public string GetLockKeyMthodName { get; set; }

            /// <summary>
            /// Cache type name
            /// </summary>
            public string CacheTypeName { get; set; }

            public MetricCollectBits MetricCollectFlags { get; set; }
        }

        public class ParamsEntity : ParamsBase
        {
            /// <summary>
            /// The name of the variable that corresponds to the cache collection
            /// </summary>
            public string ItemsCollectionName { get; set; }
        }

        public class ParamsColumn : ParamsBase
        {
            public string RemoveExpiredMethodName { get; set; }

            public string[] CollectionsToClearNames { get; set; }

            public string[] DisposedResourcesNames { get; set; }
        }
    }
}
