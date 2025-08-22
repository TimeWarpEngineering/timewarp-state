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

[!code-csharp[Startup](../../Samples/01-ReduxDevTools/Wasm/Sample01Wasm/Program.cs?highlight=15 "Code Link")]

### Add ReduxDevTools Component

Add the ReduxDevTools Component to your App.Razor

See example from the sample app below:
[!code-csharp[Startup](../../Samples/01-ReduxDevTools/Wasm/Sample01Wasm/App.razor?highlight=15,19 "Code Link")]

Now run your app and Open the Redux Dev Tools and you should see Actions as they are executed.
