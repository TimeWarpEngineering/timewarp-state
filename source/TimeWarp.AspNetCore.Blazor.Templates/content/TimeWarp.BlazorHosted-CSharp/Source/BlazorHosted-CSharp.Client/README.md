The client and server applications have parallel structures. 
Both utilize the MediatR Pipeline and the Command pattern of "Model in Model out."

# Folder Structure

* Behaviors: This is for custom middle-ware added to the mediator pipeline.
* Components: For shared components used in more than one Page.
* Features: Organized by State. See below.
* Layout: This is the sites main layout.
* Pages: Organized by Routes.  These are the top entry points into the SPA.
* wwwroot: static items used by the client css, js and the Entry page (index.html) to the SPA.

## Features
Each Folder is named after the State.
Items 
Example Features/Counter
which directly contains the State (CounterState)
In this Folder you can have 
 * Actions: The Action Handler Validator Mapper 
 * Components: Components that Utilize this State
 * InteropObjects: Javascript to C# 
 * 



# Logging
If no Logger is provided BlazorState would default to the "Null Logger".
Logging is done with Blazor Logging Extension
https://github.com/BlazorExtensions/Logging

# Configuration
In Program.cs add the following:
  services.AddBlazorState();
  
Add the following in your App.cshtml if you want to enable ReduxDevTools
@using BlazorState.Behaviors.ReduxDevTools
@inherits ReduxDevToolsComponent

The BaseComponent would not be required to inherit from a Blazor-State component, 
but is added here for simplicity.

# Extending the Pipeline 
There is a sample `MyBehavoir` to show how one can add their own behaviors (middle-ware) into the pipeline.

