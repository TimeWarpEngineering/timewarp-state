* Implement Routing Feature so the Route is in RouteState.
* Add in automated CI
* Add tests

* Implement DevTools Handlers (Currently only Jump works)
* Improve usefullness of ILogger.
* Add Feature to add Routing into State. (Could be extra lib) Blazor-State-Routing

* Document all public interfaces
* Check visibility on classes props etc... private public interal etc..
* Convert js to ts.  See Blazor package for example and Logging.
* Consider spliting Packages for DevTools as production we will not want to deploy that.Blazor-State-ReduxDevTools

* Review TODOs in source
* Update Samples to latest templates
* Extract common items to Directory.Build.props
* <del>Add Name to IState (Petes Idea nice option)</del> Not going to use as we are using Class.FullName to look up now.
* Move JavaScriptInterop Folder out of Behaviors (It isn't a behavoir) although ReduxDevTools depends on it.
* ReduxDevToolsBehavoir uses the pipeline but we probalby don't care to log those interaction.
So Maybe we should add a Filter to ignore logging some request IReduxAction marker or something?
* CodeMaid Clean all before publish ()

* See the MobX example for how to implement more ReduxDevTools functionality
https://github.com/zalmoxisus/mobx-remotedev/blob/master/src/monitorActions.js