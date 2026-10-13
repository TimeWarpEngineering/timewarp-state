---
uid: TimeWarpState:Release.12.0.0-beta.13.md
title: Release 12.0.0-beta.13
---

## Release 12.0.0-beta.13

### TimeWarp.Mediator 14.0.0-beta.5

- **Mediator pins move to `14.0.0-beta.5`** (`TimeWarp.Mediator.Contracts`, `.Generators`, `.Analyzers`). This is the dogfood pass for the Mediator 14 repository-review changes: net10.0-only runtime and Contracts, the incremental generator pipeline, and the `TWM005`–`TWM011` diagnostics. The State build, the samples, and the trimmed publish report no `TWM` diagnostics.
- **Generated registration replaces the hand-written server pipeline registration in the test app.** Hosts call the generated `AddGeneratedMediator<TScope>()`; the generated extension types are `internal`. A test assembly that is a friend of two hosts still needs an `extern alias` to pick one (timewarp-mediator#63).
- **`Microsoft.CodeAnalysis.CSharp` stays at `4.14.0`.** It is the consumer compiler floor for the shipped analyzer and source generator; a test pins it.

No public API changed.
