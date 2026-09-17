; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
StateInheritanceTypeArgumentRule | Design | Error | StateInheritanceAnalyzer
StateReadOnlyPublicPropertiesRule | Design | Error | StateReadOnlyPublicPropertiesAnalyzer
StateSealedClassRule | Design | Warning | StateInheritanceAnalyzer
TW0001 | TimeWarp.State | Error | TimeWarpStateActionAnalyzer
TW0002 | Design | Warning | HandlerMustNotSendActionAnalyzer
TW0003 | Design | Info | HandlerMustNotSendActionAnalyzer
TWS001 | Design | Error | StateImplementationAnalyzer
