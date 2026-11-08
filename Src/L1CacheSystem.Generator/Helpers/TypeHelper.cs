using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace L1CacheSystem.Generator.Helpers
{
    internal static class CamelCaseHelper
    {
        internal static string ToCamelCase(this string name)
        {
            if (name == null)
            {
                return null;
            }

            var nameSpan = name.AsSpan();
            if (nameSpan.Length == 1)
            {
                return char.ToLowerInvariant(nameSpan[0]).ToString();
            }

            var result = new string('c', nameSpan.Length);
            unsafe
            {
                fixed (char* pSource = nameSpan)
                fixed (char* pDest = result)
                {
                    *pDest = char.ToLowerInvariant(*pSource);
                    Buffer.MemoryCopy(
                        source: pSource + 1,
                        destination: pDest + 1,
                        destinationSizeInBytes: (result.Length - 1) * sizeof(char),
                        sourceBytesToCopy: (nameSpan.Length - 1) * sizeof(char));
                }
            }

            return result;
        }
    }

    internal static class TypeHelper
    {
        internal static bool IsAssignableFrom(
            this INamedTypeSymbol type,
            string fullNamespace,
            string typeName)
        {
            var nestedStack = new Stack<INamedTypeSymbol>();
            nestedStack.Push(type);
            while (nestedStack.Count != 0)
            {
                var currentType = nestedStack.Pop();
                if (currentType.ContainingNamespace.GetFullNamespace() == fullNamespace && currentType.Name == typeName)
                {
                    return true;
                }
                if (currentType.BaseType != null)
                {
                    nestedStack.Push(currentType.BaseType);
                }
            }
            return false;
        }

        internal static string GetFullNamespace(this INamespaceSymbol namespaceSymbol)
        {
            var builder = new StringBuilder();
            var nestedStack = new Stack<INamespaceSymbol>();
            var currentNamespace = namespaceSymbol;
            while (currentNamespace != null)
            {
                nestedStack.Push(currentNamespace);
                currentNamespace = currentNamespace.ContainingNamespace;
            }

            while(nestedStack.Count != 0)
            {
                currentNamespace = nestedStack.Pop();
                if(currentNamespace.IsGlobalNamespace)
                {
                    continue;
                }

                builder.Append(currentNamespace.Name);
                if(nestedStack.Count != 0)
                {
                    builder.Append(".");
                }
            }

            return builder.ToString();
        }

        internal static string GetFullTypeName(
            this ITypeSymbol typeSymbol,
            bool replaceNullable = false,
            bool addQuestionNoatble = true)
        {
            if(replaceNullable && typeSymbol.IsNullableType())
            {
                var named = (INamedTypeSymbol)typeSymbol;
                var firstArg = (INamedTypeSymbol)named.TypeArguments[0];

                return $"{firstArg.NameSpaceWithName()}{(addQuestionNoatble ? "?" : "")}";
            }

            if(typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (typeSymbol.IsListType(out var elementListType))
                {
                    return $"System.Collections.Generic.List<{elementListType.NameSpaceWithName()}>";
                }

                return namedTypeSymbol.NameSpaceWithName();
            }

            if(typeSymbol.IsArrayType(out var elementArrType))
            {
                return $"{elementArrType.NameSpaceWithName()}[]";
            }

            throw new NotImplementedException();
        }

        private static string NameSpaceWithName(this INamedTypeSymbol namedTypeSymbol)
        {
            return $"{namedTypeSymbol.ContainingNamespace.GetFullNamespace()}.{namedTypeSymbol.Name}";
        }

        internal static bool IsArrayType(
            this ITypeSymbol typeSymbol,
            out INamedTypeSymbol elementType)
        {
            if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                elementType = (INamedTypeSymbol)arrayTypeSymbol.ElementType;
                return true;
            }

            elementType = null;
            return false;
        }

        internal static bool IsListType(
            this ITypeSymbol typeSymbol,
            out INamedTypeSymbol elementType)
        {
            if (!(typeSymbol is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsGenericType ||
                namedTypeSymbol.ConstructedFrom == null ||
                namedTypeSymbol.ConstructedFrom.NameSpaceWithName() != "System.Collections.Generic.List"
                )
            {
                elementType = null;
                return false;
            }

            elementType = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
            return true;
        }

        internal static bool IsNullableType(this ITypeSymbol typeSymbol)
        {
            if(!(typeSymbol is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsGenericType ||
                namedTypeSymbol.ConstructedFrom == null ||
                namedTypeSymbol.ConstructedFrom.NameSpaceWithName() != "System.Nullable"
                )
            {
                return false;
            }

            return true;
        }

        internal static void GetPropertyOrFieldName(
            this ITypeSymbol typeSymbol,
            string propertyName,
            out string findName,
            out ITypeSymbol findType)
        {
            var propertyLower = propertyName.ToLowerInvariant();
            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is IPropertySymbol propertySymbol && propertySymbol.Name.ToLowerInvariant() == propertyLower)
                {
                    findName = propertySymbol.Name;
                    findType = propertySymbol.Type;
                    return;
                }

                if (member is IFieldSymbol fieldSymbol && fieldSymbol.Name.ToLowerInvariant() == propertyLower)
                {
                    findName = fieldSymbol.Name;
                    findType = fieldSymbol.Type;
                    return;
                }
            }

            throw new Exception($"Type '{typeSymbol.GetFullTypeName()}' does not contain a member named '{propertyName}'");
        }
    }
}