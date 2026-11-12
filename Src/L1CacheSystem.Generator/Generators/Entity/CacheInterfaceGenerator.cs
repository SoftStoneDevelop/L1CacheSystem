using Microsoft.CodeAnalysis;
using System;
using System.Text;

namespace L1CacheSystem.Generator.Generators.Entity
{
    internal class CacheInterfaceGenerator
    {
        public void GenerateAndSave(
            SourceProductionContext context,
            INamedTypeSymbol interfaceType,
            Params[] inputParams, 
            StringBuilder builder,
            string usings,
            string partName)
        {
            Usings(builder);
            Namespace(builder, interfaceType);
            StartInterface(builder, interfaceType);

            DeclareMethods(builder, inputParams);

            EndInterface(builder);

            context.AddSource($"{partName}CacheInterface.g.cs", builder.ToString());
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

        private void Namespace(StringBuilder builder, INamedTypeSymbol interfaceType)
        {
            builder.Append($@"
namespace {interfaceType.ContainingNamespace};
");
        }

        private void StartInterface(StringBuilder builder, INamedTypeSymbol interfaceType)
        {
            builder.Append($@"
public partial interface {interfaceType.Name}
{{
");
        }

        private void DeclareMethods(StringBuilder builder, Params[] inputParams)
        {
            foreach (var inputParam in inputParams)
            {
                builder.Append($@"
        {inputParam.MethodDeclaration};
");
            }
        }

        private void EndInterface(StringBuilder builder)
        {
            builder.Append($@"
}}
");
        }

        public class Params
        {
            public string MethodDeclaration { get; set; }
        }
    }
}