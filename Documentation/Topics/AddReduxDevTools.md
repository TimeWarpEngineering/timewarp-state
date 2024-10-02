---
uid: TimeWarpState:AddReduxDevTools.md
title: Add Redux Dev Tools
---

## Add Redux Dev Tools to your project

[ReduxDevTools](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd) are a chrome extension that let you view the `Action`s and `State` before and after each `Action` is handled.
This is quite handy for debugging.

### Enable in TimeWarp.State

> [!WARNING]
> Redux Dev Tools should NOT be enabled in production.

Redux Dev Tools are disabled by default.  Update the options passed to the `AddTimeWarpState` extension method to set `UseReduxDevToolsBehavior` to true, as show here in the sample application:

[!code-csharp[Startup](../../Samples/00-StateActionsHandlers/Sample/Client/Program.cs?highlight=16 "Code Link")]

### Initialize in App Component

ReduxDevTools requires Javascript interop. Similar to Routing and Javascript interop one must inject and initialize `ReduxDevToolsInterop` in the base Application component.

See example from the sample app below:
[!code-csharp[Startup](../../Samples/01-StateActionsHandlers/Sample/Client/App.razor.cs?highlight=12,19 "Code Link")]

Now run your app and Open the Redux Dev Tools and you should see Actions as they are executed.
