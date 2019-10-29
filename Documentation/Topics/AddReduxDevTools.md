---
uid: BlazorState:AddReduxDevTools.md
title: Add Redux Dev Tools
---

## Add Redux Dev Tools to your project

[ReduxDevTools](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd) are a chrome extension that let you view the `Action`s and `State` before and after each `Action` is handled.
This is quite handy for debugging.

> [!NOTE]
> Support for "TimeTravel" via ReduxDevTools is being removed from Blazor-State as this feature, although cool for demos, does little to assist in debugging and requires more code to implement and maintain.

### Enable in Blazor-State

> [!WARNING]
> Redux Dev Tools should NOT be enabled in production.

Redux Dev Tools are disabled by default.  Update the options passed to the `AddBlazorState` extension method to set `UseReduxDevToolsBehavior` to true, as show here in the test application:

[!code-csharp[Startup](../../Tests/TestApp/Client/Startup.cs?highlight=29 "Code Link")]

### Initialize in App Component

ReduxDevTools requires Javascript interop. Similar to Routing and Javascript interop one must inject and initialize `ReduxDevToolsInterop` in the base Application component.

See example from the test app below:
[!code-csharp[Startup](../../Tests/TestApp/Client/App.razor.cs?highlight=13,21 "Code Link")]

Now run your app and Open the Redux Dev Tools (a tab in Chrome Dev Tools) and you should see Actions as they are executed.
