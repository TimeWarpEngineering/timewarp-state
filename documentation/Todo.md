* Move ReduxDevToolsBehavoir to top of pipeline.  
So when doing time travel we can disable actions from actually being executed.
* JsonRequestHandler should be in its own component `JavascriptInteropComponent`
And moved into the App.cshtml Singletons.
* Move JavaScriptInterop Folder out of Behaviors (It isn't a behavior) although ReduxDevTools depends on it.
* Update RouteManager to subscribe to Route Change and update the URL if not already there.
This will let dev tools go back to pages.

* <del>Implement Routing Feature so the Route is in RouteState.</del>

* Add in automated CI
* Add tests

* Implement DevTools Handlers (Currently only Jump works)
See the MobX example for how to implement more ReduxDevTools functionality
https://github.com/zalmoxisus/mobx-remotedev/blob/master/src/monitorActions.js

* Improve usefulness of ILogger.
* Add Feature to add Routing into State. (Could be extra lib) Blazor-State-Routing

* Document all public interfaces
* Check visibility on classes props etc... private public internal etc..
* Convert js to ts.  See Blazor package for example and Logging.
* Consider splitting Packages for DevTools as production we will not want to deploy that.Blazor-State-ReduxDevTools

* Review TODOs in source
* Update Samples to latest templates
* Extract common items to Directory.Build.props
* <del>Add Name to IState (Pete's Idea nice option)</del> Not going to use as we are using Class.FullName to look up now.

* ReduxDevToolsBehavoir uses the pipeline but we probably don't care to log those interaction.
So Maybe we should add a Filter to ignore logging some request IReduxAction marker or something?
* CodeMaid Clean all before publish ()
