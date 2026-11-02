; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
PRIT005 | TypeValidation | Error | Unsupported wrapper declaration
PRIT006 | TypeValidation | Error | Reserved generated member conflict
PRIT007 | Serialization | Warning | LiteDB is referenced but the wrapped type has no built-in LiteDB bridge
