class BlazorState {
  constructor() {
    this.assemblyName = 'BlazorState';
    this.namespace = 'BlazorState.Features.JavaScriptInterop';
    this.typeName = 'JavaScriptInstanceHelper';
    this.methodName = 'Handle';

    this.requestHandler =
      Blazor.platform.findMethod(this.assemblyName, this.namespace, this.typeName, this.methodName);
  }

  DispatchRequest(request) {
    const requestAsJson = JSON.stringify(request);
    console.log(`Dispatching request: ${requestAsJson}`);
    const requestAsString = Blazor.platform.toDotNetString(requestAsJson);
    Blazor.platform.callMethod(this.requestHandler, null, [requestAsString]);
  }
}