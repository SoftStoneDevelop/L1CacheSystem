using L1CacheSystem.Generator.Enums;
using Microsoft.CodeAnalysis;
using System.Text;

namespace L1CacheSystem.Generator.Generators
{
    internal class CacheCommonMethodsGenerator
    {
        public void GenerateAndSave(
            SourceProductionContext context,
            INamedTypeSymbol containsType,
            Params[] inputParams, 
            StringBuilder builder,
            string usings,
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

        private void DisposeGeneratedMethod(StringBuilder builder, Params[] inputParams)
        {
            builder.Append($@"
    protected override void DisposeGenerated()
    {{");
            foreach (var inputParam in inputParams)
            {
                builder.Append($@"
        {inputParam.ItemsCollectionName}.Clear();");
            }

            builder.Append($@"
    }}
");
        }

        private void RemoveExpiredGeneratedMethod(StringBuilder builder, Params[] inputParams)
        {
            builder.Append($@"
    protected override void RemoveExpiredGenerated()
    {{");
            foreach (var inputParam in inputParams)
            {
                string collectMetricsStr = inputParam.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount) ? "true" : "false";
                builder.Append($@"
        RemoveExpired({inputParam.ItemsCollectionName}, {inputParam.GetLockKeyMthodName}, ""{inputParam.CacheTypeName}"", {collectMetricsStr});");
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

        public class Params
        {
            /// <summary>
            /// The name of the variable that corresponds to the cache collection
            /// </summary>
            public string ItemsCollectionName { get; set; }

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
    }
}
