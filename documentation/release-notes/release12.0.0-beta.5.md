---
uid: TimeWarpState:Release.12.0.0-beta.5.md
title: Release 12.0.0-beta.5
---

## Release 12.0.0-beta.5

### Fixes

- `ThrowIfNotTestAssembly` treats an assembly as a test assembly when its full name contains `test` (ordinal, ignore case). Kebab-case test assemblies such as `web-spa-integration-tests` pass. Consumers can drop `<AssemblyName>` overrides that existed only to capitalize `Test`.
- Test hosts can call `StateTestOptions.Enable()` instead of relying on the assembly name. Name sniffing remains a fallback in 12.0.0-beta.5 and is removed in the following release.
