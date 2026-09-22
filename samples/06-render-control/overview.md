---
uid: TimeWarp.State:06-RenderControl.md
title: TimeWarp.State render control sample
description: ShouldRender categories, parameter checks, and RegisterRenderTrigger
---

# TimeWarp.State render control sample

Blazor WebAssembly page for `TimeWarpStateComponent` render decisions.

How to run it, what each card skips, and when to leave `CaptureRenderCaller` off: [readme.md](readme.md).

```bash
./bin/dev pack
dotnet run --project samples/06-render-control/wasm/sample-06-wasm/sample-06-wasm.csproj --launch-profile http
```

Open `http://localhost:5296`. The text filter skips a new wrapper with the same text. The count-only card skips `Beat`.
