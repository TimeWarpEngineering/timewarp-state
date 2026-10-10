; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
; TWS0009-TWS0012 stay here until the 12.0 release. Move them to AnalyzerReleases.Shipped.md at that release.

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TWS0010 | Design | Error | StateInheritanceAnalyzer
TWS0012 | Design | Error | StateReadOnlyPublicPropertiesAnalyzer
TWS0011 | Design | Warning | StateInheritanceAnalyzer
TWS0001 | TimeWarp.State | Error | TimeWarpStateActionAnalyzer
TWS0002 | Design | Warning | HandlerMustNotSendActionAnalyzer
TWS0003 | Design | Info | HandlerMustNotSendActionAnalyzer
TWS0004 | Design | Error | CatalogActionAnalyzer
TWS0005 | Design | Error | CatalogActionAnalyzer
TWS0006 | Design | Error | CatalogActionAnalyzer
TWS0007 | Design | Warning | CatalogActionAnalyzer
TWS0008 | Design | Error | CatalogActionAnalyzer
TWS0009 | Design | Error | StateImplementationAnalyzer
