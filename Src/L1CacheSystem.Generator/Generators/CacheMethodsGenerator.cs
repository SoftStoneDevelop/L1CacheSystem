using L1CacheSystem.Generator.AttributeInfo;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace L1CacheSystem.Generator.Generators
{
    internal class CacheMethodsGenerator
    {
        public const int PublicMethodCount = 5;

        public void GenerateAndSave(
            SourceProductionContext context,
            CacheInfo cacheInfo,
            StringBuilder builder,
            string usings,
            string partName)
        {
            Usings(builder, cacheInfo);
            Namespace(builder, cacheInfo);
            StartClass(builder, cacheInfo);

            PrivateVars(builder, cacheInfo);

            LockMethods(builder, cacheInfo);
            SetMethod(builder, cacheInfo);
            DeleteMethod(builder, cacheInfo);
            GetMethod(builder, cacheInfo);
            GetAllMethod(builder, cacheInfo);

            EndClass(builder, cacheInfo);

            context.AddSource($"{partName}{cacheInfo.CacheName}.g.cs", builder.ToString());
        }

        public string CollectionName(CacheInfo cacheInfo)
        {
            return $"_{cacheInfo.CacheName.ToCamelCase()}Collection";
        }

        public string GetLockKeyMethodName(CacheInfo cacheInfo)
        {
            return $"Get{cacheInfo.CacheName}LockKey";
        }

        public string GetLockMethodName(CacheInfo cacheInfo)
        {
            return $"Get{cacheInfo.CacheName}LockAsync";
        }

        public IEnumerable<string> GetPublicMethodDeclarations(CacheInfo cacheInfo)
        {
            yield return GetLockMethodDeclaration(cacheInfo);
            yield return SetMethodDeclaration(cacheInfo);
            yield return GetMethodDeclaration(cacheInfo);
            yield return GetAllMethodDeclaration(cacheInfo);
            yield return DeleteMethodDeclaration(cacheInfo);
        }

        private void Usings(StringBuilder builder, CacheInfo cacheInfo)
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

        private void Namespace(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
namespace {cacheInfo.ContainsType.ContainingNamespace};
");
        }

        private void StartClass(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
public partial class {cacheInfo.ContainsType.Name}
{{
");
        }

        private void PrivateVars(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
    private const string {GetLockPrefixName(cacheInfo)} = ""{cacheInfo.CacheName.ToLowerInvariant()}"";
    private readonly ConcurrentDictionary<{cacheInfo.KeyType.GetFullTypeName()}, CacheValue<{cacheInfo.CacheType.GetFullTypeName()}>> {CollectionName(cacheInfo)} = new();
");
        }

        private string GetLockPrefixName(CacheInfo cacheInfo)
        {
            return $"_prefix{cacheInfo.CacheName.ToCamelCase()}";
        }

        private string GetLockMethodDeclaration(CacheInfo cacheInfo)
        {
            return $"Task {GetLockMethodName(cacheInfo)}({cacheInfo.KeyType.GetFullTypeName()}" +
                $" {cacheInfo.CacheName.ToCamelCase()}Key," +
                $" ILockGuard lockGuard," +
                $" CancellationToken cancellationToken = default)";
        }

        private void LockMethods(StringBuilder builder, CacheInfo cacheInfo)
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

        private string SetMethodDeclaration(CacheInfo cacheInfo)
        {
            return $"void Set{cacheInfo.CacheName}({cacheInfo.CacheType.GetFullTypeName()} inValue, ValueOwnership valueOwnership = ValueOwnership.NotTransferred, TimeSpan? expiry = null)";
        }

        private void SetMethod(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
    public {SetMethodDeclaration(cacheInfo)}
    {{");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetCallTime))
            {
                builder.Append($@"
        var start = _timeProvider.GetTimestamp();");
            }

            builder.Append($@"
        {cacheInfo.CacheType.GetFullTypeName()} value;
        if (valueOwnership == ValueOwnership.NotTransferred)
        {{
            value = {CreateCopy("inValue", cacheInfo)};
        }}
        else
        {{
            value = inValue;
        }}

        var cacheItem = new CacheValue<{cacheInfo.CacheType.GetFullTypeName()}>(value, _timeProvider.GetUtcNow() + expiry);
        while (true)
        {{
            if ({CollectionName(cacheInfo)}.TryGetValue(value.{cacheInfo.KeyProperty}, out var currentItem))
            {{
                if (!{CollectionName(cacheInfo)}.TryUpdate(value.{cacheInfo.KeyProperty}, cacheItem, currentItem))
                {{
                    continue;
                }}

                break;
            }}
            else
            {{
                if ({CollectionName(cacheInfo)}.TryAdd(value.{cacheInfo.KeyProperty}, cacheItem))
                {{");

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
                    _metrics.IncItemsCount(""{cacheInfo.CacheName}"");");
            }

            builder.Append($@"
                    break;
                }}

                continue;
            }}
        }}
");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetCallsCount))
            {
                builder.Append($@"
        _metrics.SetCallInc(""{cacheInfo.CacheName}"", valueOwnership);");
            }

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetCallTime))
            {
                builder.Append($@"
        _metrics.SetCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.CacheName}"", valueOwnership);");
            }

            builder.Append($@"
    }}
");
        }

        private string DeleteMethodDeclaration(CacheInfo cacheInfo)
        {
            return $"void Delete{cacheInfo.CacheName}({cacheInfo.KeyType.GetFullTypeName()} key)";
        }

        private void DeleteMethod(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
    public {DeleteMethodDeclaration(cacheInfo)}
    {{");

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteCallTime))
            {
                builder.Append($@"
        var start = _timeProvider.GetTimestamp();");
            }

            builder.Append($@"
        if ({CollectionName(cacheInfo)}.TryRemove(key, out var _))
        {{");

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
            _metrics.DecItemsCount(""{cacheInfo.CacheName}"");");
            }

            builder.Append($@"
        }}");

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteCallTime))
            {
                builder.Append($@"
        _metrics.DeleteCallInc(""{cacheInfo.CacheName}"");");
            }

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteCallTime))
            {
                builder.Append($@"
        _metrics.DeleteCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.CacheName}"");");
            }

            builder.Append($@"
    }}
");
        }

        private string GetMethodDeclaration(CacheInfo cacheInfo)
        {
            return $"{cacheInfo.CacheType.GetFullTypeName()}? Get{cacheInfo.CacheName}({cacheInfo.KeyType.GetFullTypeName()} key, ReturnType returnType = ReturnType.Copy)";
        }

        private void GetMethod(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
    public {GetMethodDeclaration(cacheInfo)}
    {{");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetCallTime))
            {
                builder.Append($@"
        var start = _timeProvider.GetTimestamp();");
            }

            builder.Append($@"
        if ({CollectionName(cacheInfo)}.TryGetValue(key, out var result))
        {{
            if (returnType == ReturnType.Reference)
            {{");

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetCallsCount))
            {
                builder.Append($@"
                _metrics.GetCallInc(""{cacheInfo.CacheName}"", ReturnType.Reference);");
            }

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetCallTime))
            {
                builder.Append($@"
                _metrics.GetCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.CacheName}"", ReturnType.Reference);");
            }

            builder.Append($@"
                return result.Value;
            }}

            var value = {CreateCopy("result.Value", cacheInfo)};");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetCallsCount))
            {
                builder.Append($@"
            _metrics.GetCallInc(""{cacheInfo.CacheName}"", ReturnType.Copy);");
            }

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetCallTime))
            {
                builder.Append($@"
            _metrics.GetCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.CacheName}"", ReturnType.Copy);");
            }

            builder.Append($@"
            return value;
        }}

        return null;
    }}
");
        }

        private string GetAllMethodDeclaration(CacheInfo cacheInfo)
        {
            return $"{cacheInfo.CacheType.GetFullTypeName()}[] GetAll{cacheInfo.CacheName}(" +
                $"Func<{cacheInfo.CacheType.GetFullTypeName()}, bool>? predicate = null," +
                $" ReturnType returnType = ReturnType.Copy)";
        }

        private void GetAllMethod(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
    public {GetAllMethodDeclaration(cacheInfo)}
    {{");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetAllCallTime))
            {
                builder.Append($@"
        var start = _timeProvider.GetTimestamp();");
            }

            builder.Append($@"
        var result = new List<{cacheInfo.CacheType.GetFullTypeName()}>({CollectionName(cacheInfo)}.Count);
        foreach (var pair in {CollectionName(cacheInfo)}.Values)
        {{
            if (predicate != null && !predicate(pair.Value))
            {{
                continue;
            }}

            if (returnType == ReturnType.Reference)
            {{
                result.Add(pair.Value);
            }}
            else
            {{
                var value = {CreateCopy("pair.Value", cacheInfo)};
                result.Add(value);
            }}
        }}

");
            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetAllCallsCount))
            {
                builder.Append($@"
        _metrics.GetAllCallInc(""{cacheInfo.CacheName}"", returnType);");
            }

            if (cacheInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.GetAllCallTime))
            {
                builder.Append($@"
        _metrics.GetAllCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.CacheName}"", returnType);");
            }

            builder.Append($@"
        return result.ToArray();
    }}
");
        }

        private static string CreateCopy(string varName, CacheInfo cacheInfo)
        {
            switch (cacheInfo.MethodType)
            {
                case Enums.CopyMethodType.CopyConstructor:
                {
                    return $"new {cacheInfo.CacheType.GetFullTypeName()}({varName})";
                }

                case Enums.CopyMethodType.CopyMethodOnInstance:
                {
                    return $"{varName}.{cacheInfo.MethodName}()";
                }

                case Enums.CopyMethodType.CopyMethodOnType:
                {
                    return $"{cacheInfo.CacheType.GetFullTypeName()}.{cacheInfo.MethodName}({varName})";
                }

                case Enums.CopyMethodType.Unknown:
                default:
                {
                    throw new NotImplementedException("Unknown copy type for generation");
                }
            }
        }

        private void EndClass(StringBuilder builder, CacheInfo cacheInfo)
        {
            builder.Append($@"
}}
");
        }
    }
}