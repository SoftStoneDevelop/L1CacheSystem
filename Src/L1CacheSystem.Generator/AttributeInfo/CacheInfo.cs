using L1CacheSystem.Generator.Constants;
using L1CacheSystem.Generator.Enums;
using L1CacheSystem.Generator.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace L1CacheSystem.Generator.AttributeInfo
{
    internal class CacheInfo
    {
        public ITypeSymbol CacheType;

        public ITypeSymbol KeyType;

        public INamedTypeSymbol InterfaceType;

        public string CacheName;

        public string KeyProperty;

        public MetricCollectBits MetricCollectFlags;

        public INamedTypeSymbol ContainsType;

        public string MethodName;

        public CopyMethodType MethodType;

        internal static bool Create(
            SourceProductionContext context,
            ImmutableArray<TypedConstant> namedArguments,
            INamedTypeSymbol containsType,
            out CacheInfo cacheInfo)
        {
            cacheInfo = null;
            if (namedArguments.Length != 7)
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametrsCount,
                    "The number of attribute parameters does not match",
                    DiagnosticSeverity.Error);
                return false;
            }

            cacheInfo = new CacheInfo();
            cacheInfo.ContainsType = containsType;
            if (!cacheInfo.FillCacheType(namedArguments[0]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 1 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillInterfaceType(namedArguments[1]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 2 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillCacheName(namedArguments[2]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 3 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillKeyProerty(namedArguments[3]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 4 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillMetricCollectFlags(namedArguments[4]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 5 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillMethodName(namedArguments[5]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 6 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.FillCopyMethodType(namedArguments[6]))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.IncorrectAttributeParametr,
                    $"Parameter 7 for {nameof(CacheInfo)} could not parse",
                    DiagnosticSeverity.Error);
                return false;
            }

            if (!cacheInfo.Validate(context))
            {
                return false;
            }

            return true;
        }

        private bool Validate(SourceProductionContext context)
        {
            if (string.IsNullOrWhiteSpace(KeyProperty))
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.KeyPropNotSet,
                    "The property used as a key for caching has not been specified within the type",
                    DiagnosticSeverity.Error);

                return false;
            }

            CacheType.GetPropertyOrFieldName(KeyProperty, out var findName, out var findType);
            if (findType == null)
            {
                DiagnosticHelper.ReportDiagnostic(
                    context,
                    DiagnosticConsts.KeyPropNotFoundInType,
                    $"Property '{nameof(KeyProperty)}' not found in type",
                    DiagnosticSeverity.Error);

                return false;
            }

            KeyType = findType;

            switch (MethodType)
            {
                case CopyMethodType.CopyConstructor:
                {
                    if (MethodName != null)
                    {
                        DiagnosticHelper.ReportDiagnostic(
                            context,
                            DiagnosticConsts.MethodNameAndCopyTypeWrong,
                            $"The property '{nameof(MethodName)}' is set, even though the copy type is via a constructor.",
                            DiagnosticSeverity.Error);

                        return false;
                    }

                    break;
                }

                case CopyMethodType.CopyMethodOnInstance:
                case CopyMethodType.CopyMethodOnType:
                {
                    if (MethodName == null)
                    {
                        DiagnosticHelper.ReportDiagnostic(
                            context,
                            DiagnosticConsts.MethodNameUnknown,
                            $"The property '{nameof(MethodName)}' is not set, even though the copy type is 'via method'",
                            DiagnosticSeverity.Error);

                        return false;
                    }

                    break;
                }

                case CopyMethodType.Unknown:
                default:
                {
                    DiagnosticHelper.ReportDiagnostic(
                        context,
                        DiagnosticConsts.CopyTypeUnknown,
                        $"Property '{nameof(MethodType)}': unknown copy method type",
                        DiagnosticSeverity.Error);
                    return false;
                }
            }

            return true;
        }

        protected bool FillKeyProerty(TypedConstant argument)
        {
            if (!(argument.Type is INamedTypeSymbol strParam) ||
                strParam.Name != nameof(String))
            {
                return false;
            }

            KeyProperty = (string)argument.Value;
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

        protected bool FillCacheType(TypedConstant argument)
        {
            if (!(argument.Value is ITypeSymbol typeParam))
            {
                return false;
            }

            CacheType = typeParam;
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

        protected bool FillMethodName(TypedConstant argument)
        {
            if (!(argument.Type is INamedTypeSymbol strParam) ||
                strParam.Name != nameof(String))
            {
                return false;
            }

            MethodName = (string)argument.Value;
            return true;
        }

        protected bool FillCopyMethodType(TypedConstant argument)
        {
            if (argument.Kind != TypedConstantKind.Enum ||
                !(argument.Type is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsAssignableFrom("L1CacheSystem.Attributes.Enums", "CopyMethodType"))
            {
                return false;
            }

            MethodType = (CopyMethodType)argument.Value;

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