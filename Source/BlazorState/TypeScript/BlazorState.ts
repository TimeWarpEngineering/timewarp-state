import { JsonRequestHandlerMethodName, JsonRequestHandlerName } from './Constants.js';

export class BlazorState {

  // Holds the .NET instance of JsonRequestHandler
  jsonRequestHandler: any;
  
  // Holds the Redux DevTools instance
  reduxDevTools: any;

  /**
   * Dispatches a JSON request to the .NET backend.
   * @param {string} requestTypeFullName - The full name of the request type.
   * @param {any} request - The request payload.
   */
  async DispatchRequest(requestTypeFullName: string, request: any) {
    // Convert the request payload to a JSON string
    const requestAsJson = JSON.stringify(request);

    console.log(`Dispatching request of Type ${requestTypeFullName}: ${requestAsJson}`);

    // Invoke the .NET method to handle the request
    await this.jsonRequestHandler.invokeMethodAsync(JsonRequestHandlerMethodName, requestTypeFullName, requestAsJson);
  }
}

export const blazorState = new BlazorState();
