using L1CacheSystem.Generator.AttributeInfo;
using L1CacheSystem.Generator.Generators.Lock;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace L1CacheSystem.Generator.Generators.Column
{
    internal class ColumnCacheMethodsGenerator
    {
        public void GenerateAndSave(
            SourceProductionContext context,
            ColumnCacheInfo cacheInfo,
            StringBuilder builder,
            string partName)
        {
            Usings(builder);
            Namespace(builder, cacheInfo);
            StartClass(builder, cacheInfo);

            PrivateVars(builder, cacheInfo);

            SetViewStruct(builder, cacheInfo);
            RemoveExpired(builder, cacheInfo);
            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                SetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                TryGetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                DeleteMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                ForeachByColumn(builder, cacheInfo.KeyInfo, cacheInfo.ColumnInfos[i]);
                ForeachByColumnWithKey(builder, cacheInfo.KeyInfo, cacheInfo.ColumnInfos[i]);
            }

            EndClass(builder);

            context.AddSource($"{partName}{cacheInfo.KeyInfo.CacheName}.g.cs", builder.ToString());
        }

        public IEnumerable<string> GetPublicMethodDeclarations(ColumnCacheInfo cacheInfo)
        {
            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                yield return SetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return TryGetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return DeleteMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return ForeachByColumnKeyMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return ForeachByColumnWithKeyMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
            }
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
using System.Runtime.InteropServices;

#nullable enable
");
        }

        private void Namespace(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
namespace {cacheInfo.KeyInfo.ContainsType.ContainingNamespace};
");
        }

        private void StartClass(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
public partial class {cacheInfo.KeyInfo.ContainsType.Name}
{{
");
        }

        public static IEnumerable<string> CollectionsToClearNames(ColumnCacheInfo cacheInfo)
        {
            yield return VarIndexName(cacheInfo.KeyInfo);

            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                yield return VarCollectionName(cacheInfo.KeyInfo, columnInfo);
                yield return VarCollectionReverseName(cacheInfo.KeyInfo, columnInfo);
            }
        }

        public static IEnumerable<string> DisposedResourcesNames(ColumnCacheInfo cacheInfo)
        {
            yield return VarSlimLockName(cacheInfo.KeyInfo);
        }

        private static string VarIndexName(KeyColumnInfo keyInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}Indexes";
        }

        private static string VarSlimLockName(KeyColumnInfo keyInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}SlimLock";
        }

        private static string VarCollectionName(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}{columnInfo.ColumnName}";
        }

        private static string VarCollectionReverseName(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"{VarCollectionName(keyInfo, columnInfo)}Reverse";
        }

        private void PrivateVars(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    private readonly Dictionary<{cacheInfo.KeyInfo.KeyType.GetFullTypeName()}, SparseIndex> {VarIndexName(cacheInfo.KeyInfo)} = [];
    private readonly ReaderWriterLockSlim {VarSlimLockName(cacheInfo.KeyInfo)} = new();
");
            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                builder.Append($@"

    private readonly List<{cacheInfo.KeyInfo.KeyType.GetFullTypeName()}> {VarCollectionReverseName(cacheInfo.KeyInfo, columnInfo)} = [];
    private readonly List<{columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)}> {VarCollectionName(cacheInfo.KeyInfo, columnInfo)} = []; 
");
            }
        }

        private string SetMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"void Set{keyInfo.CacheName}{columnInfo.ColumnName}({keyInfo.KeyType.GetFullTypeName()} key, {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)} value)";
        }

        private void SetMethod(
            StringBuilder builder,
            KeyColumnInfo keyInfo,
            int columnIndex,
            ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {SetMethodDeclaration(keyInfo, columnInfo)}
    {{
        {VarSlimLockName(keyInfo)}.EnterWriteLock();
        try
        {{
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrAddDefault({VarIndexName(keyInfo)}, key, out _);
            var indexProp = index.GetIndex({columnIndex});
            if (indexProp != -1)
            {{
                {VarCollectionName(keyInfo, columnInfo)}[indexProp] = value;
            }}
            else
            {{
                index.SetIndex({columnIndex}, true, {VarCollectionName(keyInfo, columnInfo)}.Count);
                {VarCollectionName(keyInfo, columnInfo)}.Add(value);
                {VarCollectionReverseName(keyInfo, columnInfo)}.Add(key);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitWriteLock();
        }}
    }}
");
        }

        private string TryGetMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"bool TryGet{keyInfo.CacheName}{columnInfo.ColumnName}({keyInfo.KeyType.GetFullTypeName()} key, out {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)} value)";
        }

        private void TryGetMethod(
            StringBuilder builder,
            KeyColumnInfo keyInfo,
            int columnIndex,
            ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {TryGetMethodDeclaration(keyInfo, columnInfo)}
    {{
        {VarSlimLockName(keyInfo)}.EnterReadLock();
        try
        {{
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {{
                value = default;
                return false;
            }}
            else
            {{
                var listIndex = index.GetIndex({columnIndex});
                if (listIndex == -1)
                {{
                    value = default;
                    return false;
                }}
                else
                {{
                    value = {VarCollectionName(keyInfo, columnInfo)}[listIndex];
                    return true;
                }}
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitReadLock();
        }}
    }}
");
        }

        private void SetViewStruct(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    public ref struct SetView{cacheInfo.KeyInfo.CacheName}<T>
    {{
        public T Source {{ get; set; }}");

            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                builder.Append($@"
        public SetViewOperation {columnInfo.ColumnName} {{ get; set; }}
        public readonly Func<T, {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)}>? Func{columnInfo.ColumnName} {{ get; init; }}
        public readonly {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)} Get{columnInfo.ColumnName}() => Func{columnInfo.ColumnName}!(Source);
");
            }

            builder.Append($@"
    }}
");
        }

        private string ForeachByColumnWithKeyMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"void ForeachBy{keyInfo.CacheName}{columnInfo.ColumnName}(Action<{keyInfo.KeyType.GetFullTypeName()}, {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)}> valueAction)";
        }

        private void ForeachByColumnWithKey(StringBuilder builder, KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {ForeachByColumnWithKeyMethodDeclaration(keyInfo, columnInfo)}
    {{
        {VarSlimLockName(keyInfo)}.EnterReadLock();
        try
        {{
            var spanColumn = CollectionsMarshal.AsSpan({VarCollectionName(keyInfo, columnInfo)});
            var spanColumnReverse = CollectionsMarshal.AsSpan({VarCollectionReverseName(keyInfo, columnInfo)});
            for (int i = 0; i < spanColumn.Length; i++)
            {{
                valueAction(spanColumnReverse[i], spanColumn[i]);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitReadLock();
        }}
    }}
");
        }

        private string ForeachByColumnKeyMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"void ForeachBy{keyInfo.CacheName}{columnInfo.ColumnName}(Action<{columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)}> valueAction)";
        }

        private void ForeachByColumn(StringBuilder builder, KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {ForeachByColumnKeyMethodDeclaration(keyInfo, columnInfo)}
    {{
        {VarSlimLockName(keyInfo)}.EnterReadLock();
        try
        {{
            var spanColumn = CollectionsMarshal.AsSpan({VarCollectionName(keyInfo, columnInfo)});
            for (int i = 0; i < spanColumn.Length; i++)
            {{
                valueAction(spanColumn[i]);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitReadLock();
        }}
    }}
");
        }

        public static string RemoveExpiredMethodName(string cacheName)
        {
            return $"RemoveExpired{cacheName}";
        }

        private void RemoveExpired(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            var keyInfo = cacheInfo.KeyInfo;
            builder.Append($@"
    private void {RemoveExpiredMethodName(keyInfo.CacheName)}()
    {{
        {VarSlimLockName(keyInfo)}.EnterUpgradeableReadLock();
        try
        {{
            var currentTime = _timeProvider.GetUtcNow();
            var keysToDelete = new List<{keyInfo.KeyType.GetFullTypeName()}>();
            var keysToDeleteSpan = CollectionsMarshal.AsSpan(keysToDelete);
            var count = keysToDelete.Count;
            foreach (var pair in {VarIndexName(keyInfo)})
            {{
                if (!pair.Value.Expire.HasValue)
                {{
                    continue;
                }}

                if (currentTime > pair.Value.Expire)
                {{
                    if (keysToDeleteSpan.Length == count)
                    {{
                        CollectionsMarshal.SetCount(keysToDelete, count + 1);
                        keysToDeleteSpan = CollectionsMarshal.AsSpan(keysToDelete);
                    }}

                    keysToDeleteSpan[count++] = pair.Key;
                }}
            }}

            if (count == 0)
            {{
                return;
            }}

            {VarSlimLockName(keyInfo)}.EnterWriteLock();
            try
            {{
                for (var i = 0; i < count; i++)
                {{
                    var keyToDelete = keysToDeleteSpan[i];
                    var resource = {LockMethodsGenerator.GetLockKeyMethodName(keyInfo.CacheName)}(keyToDelete);
                    _locks.TryGetValue(resource, out var keyLock);
                    if (keyLock?.TryEnterBusy() == false)
                    {{
                        // lock busy
                        continue;
                    }}

                    try
                    {{
                        var inWait = keyLock?.InWait() ?? 0;
                        if (inWait != 0)
                        {{
                            continue;
                        }}

                        if ({VarIndexName(keyInfo)}.Remove(keyToDelete, out var index))
                        {{");

            if (keyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
                            _metrics.DecItemsCount(""{keyInfo.CacheName}"");");
            }

            builder.Append($@"
                            if (currentTime < index.Expire)
                            {{
                                {VarIndexName(keyInfo)}.Add(keyToDelete, index);");

            if (keyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
                                _metrics.IncItemsCount(""{keyInfo.CacheName}"");");
            }

            builder.Append($@"
                                continue;
                            }}");

            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                builder.Append($@"
                            // TODO {columnInfo.ColumnName}");
            }

            builder.Append($@"
                        }}
                    }}
                    finally
                    {{
                        if (keyLock?.TryExitBusy() == false)
                        {{
                            LogResourceExitBusyError(resource);
                        }}
                    }}
                }}
            }}
            finally
            {{
                {VarSlimLockName(keyInfo)}.ExitWriteLock();
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitUpgradeableReadLock();
        }}
    }}");

        }

        private string DeleteMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"void Delete{keyInfo.CacheName}{columnInfo.ColumnName}({keyInfo.KeyType.GetFullTypeName()} key)";
        }

        private void DeleteMethod(
            StringBuilder builder,
            KeyColumnInfo keyInfo,
            int columnIndex,
            ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {DeleteMethodDeclaration(keyInfo, columnInfo)}
    {{
        {VarSlimLockName(keyInfo)}.EnterWriteLock();
        try
        {{
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {{
                return;
            }}
            else
            {{
                var columnIndex = index.GetIndex({columnIndex});
                if (columnIndex == -1)
                {{
                    return;
                }}
                
                InternalDelete{keyInfo.CacheName}{columnInfo.ColumnName}(columnIndex);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitWriteLock();
        }}
    }}

    private void InternalDelete{keyInfo.CacheName}{columnInfo.ColumnName}(int columnIndex)
    {{
        if (columnIndex == {VarCollectionName(keyInfo, columnInfo)}.Count - 1)
        {{
            {VarCollectionName(keyInfo, columnInfo)}.RemoveAt(columnIndex);
            {VarCollectionReverseName(keyInfo, columnInfo)}.RemoveAt(columnIndex);
        }}
        else
        {{
            var endOfListIndex = {VarCollectionName(keyInfo, columnInfo)}.Count - 1;
            var swapId = _columnCacheChildReverse[endOfListIndex];
            {VarCollectionReverseName(keyInfo, columnInfo)}[columnIndex] = swapId;
            {VarCollectionReverseName(keyInfo, columnInfo)}.RemoveAt(endOfListIndex);

            ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, swapId);
            swapIndex.SetIndex({columnIndex}, false, columnIndex);
            {VarCollectionName(keyInfo, columnInfo)}[columnIndex] = {VarCollectionName(keyInfo, columnInfo)}[endOfListIndex];
            {VarCollectionName(keyInfo, columnInfo)}.RemoveAt(endOfListIndex);
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
    }
}