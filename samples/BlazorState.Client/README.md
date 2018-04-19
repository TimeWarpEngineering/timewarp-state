This example shows how the client and server parallel structures. 
Both utlizing the MediatR Pipeline and the Command pattern of Model in Model out.
# Nightly Builds
Currently we are running against nightly builds. On release of Blazor 0.4.0 we are hopeful we can change that.

# Logging
If no Logger is provided we will utilize the "Null Logger".
Logging was done with a development version of the Blazor Logging Extention
https://github.com/BlazorExtensions/Logging

For working version against nightly blazor nightly builds
https://github.com/BlazorExtensions/Logging/pull/6

See PR#  (will not be available until Blazor 0.4.0 also).
# Configuration
In Program.cs add the following:
  services.AddBlazorState();
  this uses the default options which include the CloneStateBehavior and ReduxDevToolsBehavior

Add the following in your App.cshtml if you want to enable ReduxDevTools
@using BlazorState.Behaviors.ReduxDevTools
@inherits ReduxDevToolsComponent

The BaseComponent would not be required to inherit from a Blazor-State component, 
but is added here for sipmlicity.

#Logging


# Extending the Pipeline 
There is a sample `MyBehavoir` to show how one can easily add their own behavoirs (middleware) into the pipeline.

