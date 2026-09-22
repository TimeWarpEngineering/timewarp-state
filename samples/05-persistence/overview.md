---
uid: TimeWarp.State:05-Persistence.md
title: TimeWarp.State Persistence sample
description: Session and local storage with PersistentState and LoadPersistentStateRequest
---

# TimeWarp.State Persistence sample

Blazor WebAssembly sample with one state in session storage and one in local storage.

How to run it, when to pick each store, and how keys behave when a payload is older than the type: [readme.md](readme.md).

```bash
./bin/dev pack
dotnet run --project samples/05-persistence/wasm/sample-05-wasm/sample-05-wasm.csproj --launch-profile http
```

Open `http://localhost:5295`. The draft is session storage (this tab). Accent and density are local storage (this origin).
