import { DotNet } from "./DotNet";

export const BlazorStateName: string = 'BlazorState';

export class BlazorState {

  async DispatchRequest(request) {
    const requestAsJson = JSON.stringify(request);
    console.log(`Dispatching request: ${requestAsJson}`);

    const assemblyName = 'BlazorState';
    const methodName = 'Handle';
    //const requestAsString = Blazor.platform.toDotNetString(requestAsJson);
    await DotNet.invokeMethodAsync(assemblyName, methodName, requestAsJson);
  }
}