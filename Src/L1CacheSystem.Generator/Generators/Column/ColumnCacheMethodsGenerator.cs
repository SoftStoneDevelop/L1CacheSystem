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
            SparseIndexStruct(builder, cacheInfo);
            SetFromViewMethod(builder, cacheInfo);

            DeleteMethod(builder, cacheInfo);

            RemoveExpired(builder, cacheInfo);
            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                SetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                TryGetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                DeleteColumnMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                ForeachByColumnMethod(builder, cacheInfo.KeyInfo, cacheInfo.ColumnInfos[i]);
                ForeachByColumnWithKeyMethod(builder, cacheInfo.KeyInfo, cacheInfo.ColumnInfos[i]);
            }

            EndClass(builder);

            context.AddSource($"{partName}{cacheInfo.KeyInfo.CacheName}.g.cs", builder.ToString());
        }

        public IEnumerable<string> GetPublicMethodDeclarations(ColumnCacheInfo cacheInfo)
        {
            yield return SetFromViewMethodDeclaration(cacheInfo);
            yield return DeleteMethodDeclaration(cacheInfo);

            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                yield return SetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return TryGetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return DeleteColumnMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
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
using System.Numerics;

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
    private readonly Dictionary<{cacheInfo.KeyInfo.KeyType.GetFullTypeName()}, {SparseIndexStructTypeName(cacheInfo.KeyInfo.CacheName)}> {VarIndexName(cacheInfo.KeyInfo)} = [];
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
            return $"void Set{keyInfo.CacheName}{columnInfo.ColumnName}({keyInfo.KeyType.GetFullTypeName()} key, {columnInfo.ColumnType.GetFullTypeName(replaceNullable: true, addQuestionNoatble: true)} value, TimeSpan? expiry = null)";
        }

        private void SetMethod(
            StringBuilder builder,
            KeyColumnInfo keyInfo,
            int columnIndex,
            ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {SetMethodDeclaration(keyInfo, columnInfo)}
    {{");

            if (columnInfo.MetricCollectFlags.HasFlag( Enums.MetricCollectBits.SetColumnCallTime))
            {
                builder.Append(@"
        var start = _timeProvider.GetTimestamp();");
            }

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetColumnCallsCount))
            {
                builder.Append($@"
        _metrics.SetColumnCallInc(""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        {VarSlimLockName(keyInfo)}.EnterWriteLock();
        try
        {{
            ref {SparseIndexStructTypeName(keyInfo.CacheName)} index = ref CollectionsMarshal.GetValueRefOrAddDefault({VarIndexName(keyInfo)}, key, out var exists);
            if (!exists)
            {{
                index = new();");

            if (keyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
                _metrics.IncItemsCount(""{keyInfo.CacheName}"");");
            }

            builder.Append($@"
            }}

            index.Expire =_timeProvider.GetUtcNow() + expiry;
            var indexProp = index.GetIndex({columnIndex});
            if (indexProp != -1)
            {{
                {VarCollectionName(keyInfo, columnInfo)}[indexProp] = value;
            }}
            else
            {{
                index.SetIndex({columnIndex}, true, {VarCollectionName(keyInfo, columnInfo)}.Count);");

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ColumnsCount))
            {
                builder.Append($@"
            _metrics.IncColumnsCount(""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
                {VarCollectionName(keyInfo, columnInfo)}.Add(value);
                {VarCollectionReverseName(keyInfo, columnInfo)}.Add(key);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitWriteLock();
");
            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetColumnCallTime))
            {
                builder.Append($@"
            _metrics.SetColumnCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
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
    {{");
            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.TryGetCallTime))
            {
                builder.Append(@"
        var start = _timeProvider.GetTimestamp();");
            }

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.TryGetCallsCount))
            {
                builder.Append($@"
        _metrics.TryGetColumnCallInc(""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        {VarSlimLockName(keyInfo)}.EnterReadLock();
        try
        {{
            ref {SparseIndexStructTypeName(keyInfo.CacheName)} index = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {{
                value = default!;
                return false;
            }}
            else
            {{
                var listIndex = index.GetIndex({columnIndex});
                if (listIndex == -1)
                {{
                    value = default!;
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
            {VarSlimLockName(keyInfo)}.ExitReadLock();");

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.TryGetCallTime))
            {
                builder.Append($@"
            _metrics.TryGetColumnCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        }}
    }}
");
        }

        private static string SetViewStructFullTypeName(ColumnCacheInfo cacheInfo)
        {
            return $"{cacheInfo.KeyInfo.ContainsType.Name}.{SetViewStructName(cacheInfo.KeyInfo.CacheName)}";
        }

        private static string SparseIndexStructTypeName(string cacheName)
        {
            return $"SparseIndex{cacheName}";
        }

        private void SparseIndexStruct(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    public struct {SparseIndexStructTypeName(cacheInfo.KeyInfo.CacheName)}
    {{
        public {SparseIndexStructTypeName(cacheInfo.KeyInfo.CacheName)}()
        {{
            _propsMask = 0;
            _indexes = [];
        }}

        private ulong _propsMask;
        private readonly List<int> _indexes;

        /// <summary>
        /// The date after which the values will be removed from the cache
        /// </summary>
        public DateTimeOffset? Expire {{ get; set; }} = null;

        /// <summary>
        /// Get index of property
        /// </summary>
        /// <returns>-1 if property not exist</returns>
        public readonly int GetIndex(byte propIndex)
        {{
            var indexInList = GetInnerIndex(propIndex);
            return indexInList == -1 ? -1 : _indexes[indexInList];
        }}

        /// <summary>
        /// Get index of property
        /// </summary>
        /// <returns>-1 if property not exist</returns>
        private readonly short GetInnerIndex(byte propIndex)
        {{
            if (HaveProp(propIndex))
            {{
                // 1 << 2 =                        0b0000_0100
                // (1 << 2) - 1 =                  0b0000_0011
                // _propsMask                      0b0000_0010
                // _propsMask & ((1 << 2) - 1) ==  0b0000_0010
                // PopCount == 1

                var mask = (1UL << propIndex) - 1;
                var indexBits = _propsMask & mask;

                var indexInList = (short)BitOperations.PopCount(indexBits);

                return indexInList;
            }}
            else
            {{
                return -1;
            }}
        }}

        public readonly bool HaveProp(byte propIndex)
        {{
            var bitMask = (1UL << propIndex);
            return (_propsMask & bitMask) != 0;
        }}

        public readonly int HavePropCount()
        {{
            return BitOperations.PopCount(_propsMask);
        }}

        public void SetIndex(byte propIndex, bool haveProp, int index)
        {{
            var bitMask = (1UL << propIndex);
            if (((_propsMask & bitMask) != 0) == haveProp)
            {{
                return;
            }}

            if (!haveProp)
            {{
                var indexInList = GetInnerIndex(propIndex);
                _indexes.RemoveAt(indexInList);
                _propsMask ^= bitMask;
            }}
            else
            {{
                // _propsMask =                   0b0110_0101
                // and _indexes.Count == 4
                // need insert(2)
                // propIndex = 3
                // bitMask ==                     0b0000_1000
                // _propsMask ^= bitMask;
                // _propsMask ==                  0b0110_1101
                // (bitMask) - 1 =                0b0000_0111
                // _propsMask & ((bitMask) - 1) = 0b0000_0101
                // insert index = 2

                _propsMask ^= bitMask;
                var beforeBits = _propsMask & (bitMask - 1);
                var indexInList = BitOperations.PopCount(beforeBits);
                _indexes.Insert(indexInList, index);
            }}
        }}

        public void UpdateIndex(byte propIndex, int newIndex)
        {{
            var indexInList = GetInnerIndex(propIndex);
            if (indexInList != -1)
            {{
                _indexes[indexInList] = newIndex;
            }}
        }}
    }}
");
        }

        private static string SetViewStructName(string cacheName)
        {
            return $"SetView{cacheName}";
        }

        private void SetViewStruct(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    public ref struct {SetViewStructName(cacheInfo.KeyInfo.CacheName)}<T>
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

        private void ForeachByColumnWithKeyMethod(StringBuilder builder, KeyColumnInfo keyInfo, ColumnInfo columnInfo)
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

        private void ForeachByColumnMethod(StringBuilder builder, KeyColumnInfo keyInfo, ColumnInfo columnInfo)
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

            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                ColumnInfo columnInfo = cacheInfo.ColumnInfos[i];
                builder.Append($@"
                            var column{keyInfo.CacheName}{columnInfo.ColumnName}Index = index.GetIndex({i});
                            if (column{keyInfo.CacheName}{columnInfo.ColumnName}Index != -1)
                            {{
                                InternalDelete{keyInfo.CacheName}{columnInfo.ColumnName}(ref index, column{keyInfo.CacheName}{columnInfo.ColumnName}Index);
                            }}
");
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

        private string DeleteColumnMethodDeclaration(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"void Delete{keyInfo.CacheName}{columnInfo.ColumnName}({keyInfo.KeyType.GetFullTypeName()} key)";
        }

        private void DeleteColumnMethod(
            StringBuilder builder,
            KeyColumnInfo keyInfo,
            int columnIndex,
            ColumnInfo columnInfo)
        {
            builder.Append($@"
    public {DeleteColumnMethodDeclaration(keyInfo, columnInfo)}
    {{");
            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteColumnCallTime))
            {
                builder.Append(@"
        var start = _timeProvider.GetTimestamp();");
            }

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteColumnCallsCount))
            {
                builder.Append($@"
        _metrics.DeleteColumnCallInc(""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        {VarSlimLockName(keyInfo)}.EnterWriteLock();
        try
        {{
            ref {SparseIndexStructTypeName(keyInfo.CacheName)} index = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, key);
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
                
                InternalDelete{keyInfo.CacheName}{columnInfo.ColumnName}(ref index,columnIndex);
            }}
        }}
        finally
        {{
            {VarSlimLockName(keyInfo)}.ExitWriteLock();");

            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.DeleteColumnCallTime))
            {
                builder.Append($@"
            _metrics.DeleteColumnCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        }}
    }}");
            builder.Append($@"

    private void InternalDelete{keyInfo.CacheName}{columnInfo.ColumnName}(ref {SparseIndexStructTypeName(keyInfo.CacheName)} index, int columnIndex)
    {{
        index.SetIndex({columnIndex}, false, columnIndex);");
            if (columnInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ColumnsCount))
            {
                builder.Append($@"
        _metrics.DecColumnsCount(""{keyInfo.CacheName}"", ""{columnInfo.ColumnName}"");");
            }

            builder.Append($@"
        if (columnIndex == {VarCollectionName(keyInfo, columnInfo)}.Count - 1)
        {{
            {VarCollectionName(keyInfo, columnInfo)}.RemoveAt(columnIndex);
            {VarCollectionReverseName(keyInfo, columnInfo)}.RemoveAt(columnIndex);
        }}
        else
        {{
            var endOfListIndex = {VarCollectionName(keyInfo, columnInfo)}.Count - 1;
            var swapId = {VarCollectionReverseName(keyInfo, columnInfo)}[endOfListIndex];
            {VarCollectionReverseName(keyInfo, columnInfo)}[columnIndex] = swapId;
            {VarCollectionReverseName(keyInfo, columnInfo)}.RemoveAt(endOfListIndex);

            ref {SparseIndexStructTypeName(keyInfo.CacheName)} swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, swapId);
            swapIndex.UpdateIndex({columnIndex}, columnIndex);
            {VarCollectionName(keyInfo, columnInfo)}[columnIndex] = {VarCollectionName(keyInfo, columnInfo)}[endOfListIndex];
            {VarCollectionName(keyInfo, columnInfo)}.RemoveAt(endOfListIndex);
        }}
    }}
");
        }

        private string SetFromViewMethodDeclaration(ColumnCacheInfo cacheInfo)
        {
            return $"void Set{cacheInfo.KeyInfo.CacheName}FromView<T>({cacheInfo.KeyInfo.KeyType.GetFullTypeName()} key, in {SetViewStructFullTypeName(cacheInfo)}<T> viewInfo, TimeSpan? expiry = null)";
        }

        private void SetFromViewMethod(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    public {SetFromViewMethodDeclaration(cacheInfo)}
    {{");
            if (cacheInfo.KeyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetFromViewCallTime))
            {
                builder.Append(@"
        var start = _timeProvider.GetTimestamp();");
            }

            if (cacheInfo.KeyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetFromViewCallsCount))
            {
                builder.Append($@"
        _metrics.SetFromViewCallInc(""{cacheInfo.KeyInfo.CacheName}"");");
            }

            builder.Append($@"
        {VarSlimLockName(cacheInfo.KeyInfo)}.EnterWriteLock();
        try
        {{
            ref {SparseIndexStructTypeName(cacheInfo.KeyInfo.CacheName)} index = ref CollectionsMarshal.GetValueRefOrAddDefault({VarIndexName(cacheInfo.KeyInfo)}, key, out var exists);
            if (!exists)
            {{
                index = new();");

            if (cacheInfo.KeyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.ItemsCount))
            {
                builder.Append($@"
                _metrics.IncItemsCount(""{cacheInfo.KeyInfo.CacheName}"");");
            }

            builder.Append($@"
            }}

            index.Expire =_timeProvider.GetUtcNow() + expiry;");

            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                ColumnInfo columnInfo = cacheInfo.ColumnInfos[i];
                builder.Append($@"
            if (viewInfo.{columnInfo.ColumnName} != SetViewOperation.None)
            {{
                var columnIndex = index.GetIndex({i});
                if (columnIndex == -1)
                {{
                    if (viewInfo.{columnInfo.ColumnName} == SetViewOperation.Set)
                    {{
                        index.SetIndex({i}, true, {VarCollectionName(cacheInfo.KeyInfo, columnInfo)}.Count);
                        {VarCollectionName(cacheInfo.KeyInfo, columnInfo)}.Add(viewInfo.Get{columnInfo.ColumnName}()!);
                        {VarCollectionReverseName(cacheInfo.KeyInfo, columnInfo)}.Add(key);
                    }}
                }}
                else
                {{
                    if (viewInfo.{columnInfo.ColumnName} == SetViewOperation.Set)
                    {{
                        {VarCollectionName(cacheInfo.KeyInfo, columnInfo)}[columnIndex] = viewInfo.Get{columnInfo.ColumnName}()!;
                    }}
                    else
                    {{
                        InternalDelete{cacheInfo.KeyInfo.CacheName}{columnInfo.ColumnName}(ref index, columnIndex);
                    }}
                }}
            }}
");
            }

            builder.Append($@"
        }}
        finally
        {{
            {VarSlimLockName(cacheInfo.KeyInfo)}.ExitWriteLock();");

            if (cacheInfo.KeyInfo.MetricCollectFlags.HasFlag(Enums.MetricCollectBits.SetFromViewCallTime))
            {
                builder.Append($@"
            _metrics.SetFromViewCallTime(_timeProvider.GetElapsedTime(start).TotalMicroseconds, ""{cacheInfo.KeyInfo.CacheName}"");");
            }

            builder.Append($@"
        }}
    }}
");
        }

        private string DeleteMethodDeclaration(ColumnCacheInfo cacheInfo)
        {
            return $"void Delete{cacheInfo.KeyInfo.CacheName}({cacheInfo.KeyInfo.KeyType.GetFullTypeName()} key)";
        }

        private void DeleteMethod(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
    public {DeleteMethodDeclaration(cacheInfo)}
    {{
        {VarSlimLockName(cacheInfo.KeyInfo)}.EnterWriteLock();
        try
        {{
            if (!{VarIndexName(cacheInfo.KeyInfo)}.Remove(key, out var index))
            {{
                return;
            }}");

            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                ColumnInfo columnInfo = cacheInfo.ColumnInfos[i];
                builder.Append($@"
            var column{cacheInfo.KeyInfo.CacheName}{columnInfo.ColumnName}Index = index.GetIndex({i});
            if (column{cacheInfo.KeyInfo.CacheName}{columnInfo.ColumnName}Index != -1)
            {{
                InternalDelete{cacheInfo.KeyInfo.CacheName}{columnInfo.ColumnName}(ref index, column{cacheInfo.KeyInfo.CacheName}{columnInfo.ColumnName}Index);
            }}
");
            }

            builder.Append($@"
        }}
        finally
        {{
            {VarSlimLockName(cacheInfo.KeyInfo)}.ExitWriteLock();
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