using L1CacheSystem.Generator.Constants;
using Microsoft.CodeAnalysis;

namespace L1CacheSystem.Generator.Helpers
{
    internal static class DiagnosticHelper
    {
        internal static void ReportDiagnostic(
            SourceProductionContext context,
            string disgnocticId,
            string message,
            DiagnosticSeverity diagnosticSeverity)
        {
            try
            {
                var diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                    disgnocticId,
                    string.Empty,
                    message,
                    DiagnosticConsts.Source,
                    diagnosticSeverity,
                    true),
                    null);

                context.ReportDiagnostic(diagnostic);
            }
            catch
            {
            }
        }
    }
}