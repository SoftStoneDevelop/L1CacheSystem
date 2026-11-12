using L1CacheSystem.Generator.Constants;
using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace L1CacheSystem.Generator.AttributeInfo
{
    internal class ColumnInfo
    {
        public ITypeSymbol ColumnType;

        public string ColumnName;

        public MetricCollectBits MetricCollectFlags;

        public INamedTypeSymbol ContainsType;

        internal static bool Create(
            SourceProductionContext context,
            ImmutableArray<TypedConstant> namedArguments,
            INamedTypeSymbol containsType,
            out ColumnInfo columnInfo)
        {
            if (namedArguments.Length != 3)
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametrsCount,
                    "The number of attribute parameters does not match",
                    DiagnosticSeverity.Error);

                columnInfo = null;
                return false;
            }

            columnInfo = new ColumnInfo();
            columnInfo.ContainsType = containsType;
            if (!columnInfo.FillColumnType(namedArguments[0]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 1 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!columnInfo.FillColumnName(namedArguments[1]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 2 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!columnInfo.FillMetricCollectFlags(namedArguments[2]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 3 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!columnInfo.Validate(context))
            {
                return false;
            }

            return true;
        }

        private bool Validate(SourceProductionContext context)
        {
            return true;
        }

        protected bool FillColumnName(TypedConstant argument)
        {
            if (!(argument.Type is INamedTypeSymbol strParam) ||
                strParam.Name != nameof(String))
            {
                return false;
            }

            ColumnName = (string)argument.Value;
            return true;
        }

        protected bool FillColumnType(TypedConstant argument)
        {
            if (!(argument.Value is ITypeSymbol typeParam))
            {
                return false;
            }

            ColumnType = typeParam;
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