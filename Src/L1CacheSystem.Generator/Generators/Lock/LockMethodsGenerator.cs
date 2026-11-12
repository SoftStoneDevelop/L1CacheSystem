using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace L1CacheSystem.Generator.Generators.Lock
{
    internal class LockMethodsGenerator
    {
        public const int PublicMethodCount = 1;

        public void GenerateAndSave(
            SourceProductionContext context,
            Param cacheInfo,
            StringBuilder builder,
            string partName)
        {
            Usings(builder, cacheInfo);
            Namespace(builder, cacheInfo);
            StartClass(builder, cacheInfo);

            PrivateVars(builder, cacheInfo);

            LockMethods(builder, cacheInfo);

            EndClass(builder, cacheInfo);

            context.AddSource($"{partName}{cacheInfo.CacheName}Locks.g.cs", builder.ToString());
        }

        public string GetLockKeyMethodName(Param cacheInfo)
        {
            return $"Get{cacheInfo.CacheName}LockKey";
        }

        public string GetLockMethodName(Param cacheInfo)
        {
            return $"Get{cacheInfo.CacheName}LockAsync";
        }

        public IEnumerable<string> GetPublicMethodDeclarations(Param cacheInfo)
        {
            yield return GetLockMethodDeclaration(cacheInfo);
        }

        private void Usings(StringBuilder builder, Param cacheInfo)
        {
            builder.Append(@"
using Microsoft.Extensions.Logging;
using L1CacheSystem.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using L1CacheSystem;

#nullable enable
");
        }

        private void Namespace(StringBuilder builder, Param cacheInfo)
        {
            builder.Append($@"
namespace {cacheInfo.ContainsType.ContainingNamespace};
");
        }

        private void StartClass(StringBuilder builder, Param cacheInfo)
        {
            builder.Append($@"
public partial class {cacheInfo.ContainsType.Name}
{{
");
        }

        private void PrivateVars(StringBuilder builder, Param cacheInfo)
        {
            builder.Append($@"
    private const string {GetLockPrefixName(cacheInfo)} = ""{cacheInfo.CacheName.ToLowerInvariant()}"";
");
        }

        private string GetLockPrefixName(Param cacheInfo)
        {
            return $"_prefix{cacheInfo.CacheName.ToCamelCase()}";
        }

        private string GetLockMethodDeclaration(Param cacheInfo)
        {
            return $"Task {GetLockMethodName(cacheInfo)}({cacheInfo.KeyType.GetFullTypeName()}" +
                $" {cacheInfo.CacheName.ToCamelCase()}Key," +
                $" ILockGuard lockGuard," +
                $" CancellationToken cancellationToken = default)";
        }

        private void LockMethods(StringBuilder builder, Param cacheInfo)
        {
            string collectMetricsStr = cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetLockTime) ? "true" : "false";
            builder.Append($@"
    private static string {GetLockKeyMethodName(cacheInfo)}({cacheInfo.KeyType.GetFullTypeName()} {cacheInfo.CacheName.ToCamelCase()}LockKey)
    {{
        return $""lock:{{{GetLockPrefixName(cacheInfo)}}}:{{{cacheInfo.CacheName.ToCamelCase()}LockKey}}"";
    }}

    public async {GetLockMethodDeclaration(cacheInfo)}
    {{
        await GetLockAsync({GetLockKeyMethodName(cacheInfo)}({cacheInfo.CacheName.ToCamelCase()}Key), ""{cacheInfo.CacheName}"", lockGuard, {collectMetricsStr}, cancellationToken);
    }}
");
        }

        private void EndClass(StringBuilder builder, Param cacheInfo)
        {
            builder.Append($@"
}}
");
        }

        public class Param
        {
            public string CacheName { get; set; }

            public ITypeSymbol KeyType { get; set; }

            public MetricCollectBits MetricCollectFlags { get; set; }

            public INamedTypeSymbol ContainsType { get; set; }
        }
    }
}