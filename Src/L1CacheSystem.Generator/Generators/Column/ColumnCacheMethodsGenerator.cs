using L1CacheSystem.Generator.AttributeInfo;
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
            Usings(builder, cacheInfo);
            Namespace(builder, cacheInfo);
            StartClass(builder, cacheInfo);

            PrivateVars(builder, cacheInfo);

            for (int i = 0; i < cacheInfo.ColumnInfos.Count; i++)
            {
                SetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
                TryGetMethod(builder, cacheInfo.KeyInfo, i, cacheInfo.ColumnInfos[i]);
            }

            EndClass(builder, cacheInfo);

            context.AddSource($"{partName}{cacheInfo.KeyInfo.CacheName}.g.cs", builder.ToString());
        }

        public IEnumerable<string> GetPublicMethodDeclarations(ColumnCacheInfo cacheInfo)
        {
            foreach (var columnInfo in cacheInfo.ColumnInfos)
            {
                yield return SetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
                yield return TryGetMethodDeclaration(cacheInfo.KeyInfo, columnInfo);
            }
        }

        private void Usings(StringBuilder builder, ColumnCacheInfo cacheInfo)
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

        private string VarIndexName(KeyColumnInfo keyInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}Indexes";
        }

        private string VarSlimLockName(KeyColumnInfo keyInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}SlimLock";
        }

        private string VarCollectionName(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
        {
            return $"_{keyInfo.CacheName.ToCamelCase()}{columnInfo.ColumnName}";
        }

        private string VarCollectionReverseName(KeyColumnInfo keyInfo, ColumnInfo columnInfo)
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
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef({VarIndexName(keyInfo)}, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {{
                var si = new SparseIndex();

                si.SetIndex({columnIndex}, true, {VarCollectionName(keyInfo, columnInfo)}.Count);
                {VarCollectionName(keyInfo, columnInfo)}.Add(value);
                {VarCollectionReverseName(keyInfo, columnInfo)}.Add(key);

                {VarIndexName(keyInfo)}.Add(key, si);
            }}
            else
            {{
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

        private void EndClass(StringBuilder builder, ColumnCacheInfo cacheInfo)
        {
            builder.Append($@"
}}
");
        }
    }
}