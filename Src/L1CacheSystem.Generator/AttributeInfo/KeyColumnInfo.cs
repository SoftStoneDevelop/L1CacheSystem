using L1CacheSystem.Generator.Constants;
using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace L1CacheSystem.Generator.AttributeInfo
{
    internal class KeyColumnInfo
    {
        public ITypeSymbol KeyType;

        public INamedTypeSymbol InterfaceType;

        public string CacheName;

        public MetricCollectBits MetricCollectFlags;

        public INamedTypeSymbol ContainsType;

        internal static bool Create(
            SourceProductionContext context,
            ImmutableArray<TypedConstant> namedArguments,
            INamedTypeSymbol containsType,
            out KeyColumnInfo keyColumnInfo)
        {
            if (namedArguments.Length != 4)
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametrsCount,
                    "The number of attribute parameters does not match",
                    DiagnosticSeverity.Error);

                keyColumnInfo = null;
                return false;
            }

            keyColumnInfo = new KeyColumnInfo();
            keyColumnInfo.ContainsType = containsType;
            if (!keyColumnInfo.FillKeyType(namedArguments[0]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 1 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!keyColumnInfo.FillInterfaceType(namedArguments[1]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 2 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!keyColumnInfo.FillCacheName(namedArguments[2]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 3 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!keyColumnInfo.FillMetricCollectFlags(namedArguments[3]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 5 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!keyColumnInfo.Validate(context))
            {
                return false;
            }

            return true;
        }

        private bool Validate(SourceProductionContext context)
        {
            return true;
        }

        protected bool FillCacheName(TypedConstant argument)
        {
            if (!(argument.Type is INamedTypeSymbol strParam) ||
                strParam.Name != nameof(String))
            {
                return false;
            }

            CacheName = (string)argument.Value;
            return true;
        }

        protected bool FillKeyType(TypedConstant argument)
        {
            if (!(argument.Value is ITypeSymbol typeParam))
            {
                return false;
            }

            KeyType = typeParam;
            return true;
        }

        protected bool FillInterfaceType(TypedConstant argument)
        {
            if (!(argument.Value is INamedTypeSymbol typeParam))
            {
                return false;
            }

            if (typeParam.TypeKind != TypeKind.Interface)
            {
                return false;
            }

            InterfaceType = typeParam;
            return true;
        }

        protected bool FillMetricCollectFlags(TypedConstant argument)
        {
            if (argument.Kind != TypedConstantKind.Enum ||
                !(argument.Type is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsAssignableFrom("L1CacheSystem.Attributes.Enums", "MetricCollectBits"))
            {
                return false;
            }

            MetricCollectFlags = (MetricCollectBits)argument.Value;
            return true;
        }
    }
}