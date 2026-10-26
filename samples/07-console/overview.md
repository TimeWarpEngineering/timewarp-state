# Console sample

A console process that references `TimeWarp.State` only. It does not reference `TimeWarp.State.Blazor`.

```bash
dotnet run --project samples/07-console/sample-07-console/sample-07-console.csproj
```

The program prints `Count=5` and exits 0. This sample project-references `TimeWarp.State`, so it builds without `dev pack`.
