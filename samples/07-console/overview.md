# Console sample

A console process that references `TimeWarp.State` only. It does not reference `TimeWarp.State.Blazor`.

```bash
dotnet run --project samples/07-console/sample-07-console/sample-07-console.csproj
```

The program prints `Count=5` and exits 0. Restore uses the local package feed (`artifacts/packages`) after `dev pack`, because samples consume the packed `TimeWarp.State` package.
