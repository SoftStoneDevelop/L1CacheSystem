using L1CacheSystem.Generator.AttributeInfo;
using System.Collections.Generic;

namespace L1CacheSystem.Generator.Generators
{
    internal class ColumnCacheInfo
    {
        public KeyColumnInfo KeyInfo { get; set; }

        public List<ColumnInfo> ColumnInfos { get; set; } = new List<ColumnInfo>();

        public bool IsEmpty()
        {
            return KeyInfo == null;
        }
    }
}