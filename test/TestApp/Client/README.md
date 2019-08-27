The client and server applications have parallel structures. 
Both utilize the MediatR Pipeline and the Command pattern of "Model in Model out."

# Client Project Folder Structure
All folders are optional if nothing will be in them feel free to delete.

* Behaviors: This is for custom middle-ware added to the mediator pipeline.
* Components: For shared components used in more than one Feature or Page.
* Features: Organized by State. See below for the structure of a `Feature`.
* Layouts: This contains the sites Layouts. (Often a site will only contain a single layout)
* Pages: Organized by Routes.
* wwwroot: static items used by the client css, js and the entry page (index.html) to the SPA.

## Features
Each Folder is named after the State to which it relates. Example *Features/Counter*

In this Folder you can have 
 * **Actions**: Contains Action and the ActionHandler Grouped in Folder by Action. 
   For Example:
      ```
      ├───Actions
      │   └───IncrementCount
      │           IncrementCounterAction.cs
      │           IncrementCounterHandler.cs
      ```
 * **Components**: 
   Components that only depend on this State.
   If other states are required then the component should be moved up the directory to a 
   `Components` folder that is a parent of all the dependent states.
 * **InteropDtos**: 
   JavaScript to C# DTOs.  If this Feature interacts with any JavaScript libraries 
   include the C# Interop classes here.  
   There should be a corresponding TypeScript version of these objects in the `Client.JS` Project.
 * **State**: The definition of the State object and any of its required classes.
 * **Features**: Contain child Features. (Child features have a dependency on this Feature).

## JavaScript Interop



# Logging
If no Logger is provided BlazorState would default to the "Null Logger".
Logging is done with Blazor Logging Extension
https://github.com/BlazorExtensions/Logging

# Configuration
In Program.cs add the following:
  services.AddBlazorState();
  
Add the following in your App.cshtml if you want to enable ReduxDevTools
@using BlazorState.Pipeline.ReduxDevTools
@inherits ReduxDevToolsComponent

The BaseComponent would not be required to inherit from a Blazor-State component, 
but is added here for simplicity.

# Extending the Pipeline 
There is a sample `MyBehavoir` to show how one can add their own behaviors (middle-ware) into the pipeline.

