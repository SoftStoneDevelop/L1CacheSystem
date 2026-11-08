using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace L1CacheSystem.Generator.Comparators
{
    internal class ByArrayComparer : IEqualityComparer<(ImmutableArray<TypeDeclarationSyntax> Nodes, Compilation compilation)>
    {
        internal static ByArrayComparer Default = new ByArrayComparer();

        public bool Equals(
           (ImmutableArray<TypeDeclarationSyntax> Nodes, Compilation compilation) left,
           (ImmutableArray<TypeDeclarationSyntax> Nodes, Compilation compilation) rigth)
        {
            if (left.Nodes.Length != rigth.Nodes.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Nodes.Length; i++)
            {
                var leftNode = left.Nodes[i];
                var rigthNode = rigth.Nodes[i];

                if (!leftNode.IsEquivalentTo(rigthNode))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode((ImmutableArray<TypeDeclarationSyntax> Nodes, Compilation compilation) obj)
        {
            int hash = 0;
            unchecked
            {
                for (int i = 0; i < obj.Nodes.Length; i++)
                {
                    hash += obj.Nodes[i].GetHashCode();
                }
            }

            return hash;
        }
    }
}