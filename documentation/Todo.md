- [ ] Move ReduxDevToolsBehavoir to top of pipeline.  So when doing time travel we can disable actions from actually being executed.

- [ ] Move JavaScriptInterop Folder out of Behaviors (It isn't a behavior) although ReduxDevTools depends on it.
- [x] Update RouteManager to subscribe to Route Change and update the URL if not already there.
This will let dev tools go back to pages.

- [ ] <del>Implement Routing Feature so the Route is in RouteState.</del>

- [x] Add in automated CI
- [x] Add tests

- [ ] Implement DevTools Handlers (Currently only Jump works)
See the MobX example for how to implement more ReduxDevTools functionality
https://github.com/zalmoxisus/mobx-remotedev/blob/master/src/monitorActions.js

- [ ] Improve usefulness of ILogger.
- [x] Add Feature to add Routing into State. (Could be extra lib) Blazor-State-Routing

- [ ] Document all public interfaces
- [ ] Check visibility on classes props etc... private public internal etc..
- [ ] Convert js to ts.  See Blazor package for example and Logging.
- [ ] Consider splitting Packages for DevTools as production we will not want to deploy that.Blazor-State-ReduxDevTools

- [ ] Review TODOs in source
- [ ] Update Samples to latest templates
- [ ] Extract common items to Directory.Build.props
- [x] <del>Add Name to IState (Pete's Idea nice option)</del> Not going to use as we are using Class.FullName to look up now.

- [ ] ReduxDevToolsBehavoir uses the pipeline but we probably don't care to log those interaction.
So Maybe we should add a Filter to ignore logging some request IReduxAction marker or something?
- [ ] CodeMaid Clean all before publish ()

- [ ] Update Source link to the now .net foundation version. https://github.com/dotnet/sourcelink/ 

- [ ] Add documentation on installing Selenium java and chromedriver.exe etc.
- [ ] Search with powergrep for "<LangVersion>"
    if blazor-state\Directory.Build.props has `<LangVersion>latest</LangVersion>` then do we need it in all the others?

- [ ] Automate deployment to MyGet of template nuget.
- [ ] Add vsts vsts.ci.yaml into the template project.
- [ ] Update global.json to latest
- [ ] Add DualMode.js to the template.
- [ ] Write Console App that updates the version for all of Blazor-State 
   I think changes in like 4 or 5 places.