- [x] Update RouteManager to subscribe to Route Change and update the URL if not already there.
- [x] Search with powergrep for "<LangVersion>"
    if blazor-state\Directory.Build.props has `<LangVersion>preview</LangVersion>` then do we need it in all the others?
- [x] Extract common items to Directory.Build.props
- [x] ReduxDevToolsBehavoir uses the pipeline but we probably don't care to log those interaction.
  So Maybe we should add a Filter to ignore logging some request IReduxAction marker or something?
- [x] Move ReduxDevToolsBehavoir to top of pipeline.
      So when doing time travel we can disable actions from actually being executed.
- [x] Move JavaScriptInterop Folder out of Behaviors (It isn't a behavior) although ReduxDevTools depends on it.
      This will let dev tools go back to pages.
