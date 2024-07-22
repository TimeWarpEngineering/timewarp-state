# Server Side

```console
dotnet new blazor --use-program-main --interactivity Server -n Sample
```

Add to Program.cs
```
// Program.cs
...
builder.Services.AddTimeWarpState();
...
```
