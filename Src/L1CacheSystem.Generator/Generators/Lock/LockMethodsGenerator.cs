using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            Usings(builder);
            Namespace(builder, cacheInfo);
            StartClass(builder, cacheInfo);

            PrivateVars(builder, cacheInfo);

            LockMethods(builder, cacheInfo);

            EndClass(builder);

            context.AddSource($"{partName}{cacheInfo.CacheName}Locks.g.cs", builder.ToString());
        }

        public static string GetLockKeyMethodName(string cacheName)
        {
            return $"Get{cacheName}LockKey";
        }

        public string GetLockMethodName(Param cacheInfo)
        {
            return $"Get{cacheInfo.CacheName}LockAsync";
        }

        public IEnumerable<string> GetPublicMethodDeclarations(Param cacheInfo)
        {
            yield return GetLockMethodDeclaration(cacheInfo);
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

        public static string GetLockDicName(string cacheName)
        {
            return $"_{cacheName.ToCamelCase()}Locks";
        }

        public static string HaveChangesVarName(string cacheName)
        {
            return $"_{cacheName.ToCamelCase()}HaveSomeChanges";
        }

        public static string MinExpiredDateName(string cacheName)
        {
            return $"_{cacheName.ToCamelCase()}MinExpired";
        }

        public static string HaveLockChangesVarName(string cacheName)
        {
            return $"_{cacheName.ToCamelCase()}HaveLockChanges";
        }

        public static string MinLockExpiredDateName(string cacheName)
        {
            return $"_{cacheName.ToCamelCase()}MinLockExpired";
        }

        private void PrivateVars(StringBuilder builder, Param cacheInfo)
        {
            builder.Append($@"
    private readonly ConcurrentDictionary<string, CacheLockInfo> {GetLockDicName(cacheInfo.CacheName)} = new();
    private int {HaveChangesVarName(cacheInfo.CacheName)} = 1; // It's simple: 1 means someone triggered a cache update. 0 means no one triggered a cache update.
    private DateTimeOffset? {MinExpiredDateName(cacheInfo.CacheName)} = null;

    private int {HaveLockChangesVarName(cacheInfo.CacheName)} = 1; // It's simple: 1 means someone triggered a GetLock. 0 means no one triggered a GetLock.
    private DateTimeOffset? {MinLockExpiredDateName(cacheInfo.CacheName)} = null;
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
    private static string {GetLockKeyMethodName(cacheInfo.CacheName)}({cacheInfo.KeyType.GetFullTypeName()} {cacheInfo.CacheName.ToCamelCase()}LockKey)
    {{
        return $""lock:{{{GetLockPrefixName(cacheInfo)}}}:{{{cacheInfo.CacheName.ToCamelCase()}LockKey}}"";
    }}

    public async {GetLockMethodDeclaration(cacheInfo)}
    {{
        try
        {{
            await GetLockAsync(
                {GetLockDicName(cacheInfo.CacheName)},
                {GetLockKeyMethodName(cacheInfo.CacheName)}({cacheInfo.CacheName.ToCamelCase()}Key),
                ""{cacheInfo.CacheName}"",
                lockGuard,
                {collectMetricsStr},
                cancellationToken)
                .ConfigureAwait(false);
        }}
        finally
        {{
            Interlocked.Exchange(location1: ref {HaveLockChangesVarName(cacheInfo.CacheName)}, value: 1);
        }}
    }}
");
        }

        private void EndClass(StringBuilder builder)
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