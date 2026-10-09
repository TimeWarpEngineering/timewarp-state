import {JsonRequestHandlerMethodName} from './constants.js';
import {DotNetReference} from './dot-net-reference';
import {log} from './logger.js';
import {ReduxDevTools} from "./redux-dev-tools";

export class TimeWarpState {

  // Holds the .NET instance of JsonRequestHandler
  jsonRequestHandler: DotNetReference;

  // Holds the Redux DevTools instance
  reduxDevTools: ReduxDevTools;

  // @ts-ignore
  /**
   * Dispatches a JSON request to the .NET backend.
   * @param {string} requestTypeFullName - The full name of the request type.
   * @param {any} request - The request payload.
   */
  async DispatchRequest(requestTypeFullName: string, request: unknown) {
    if (!this.jsonRequestHandler) {
      throw new Error('jsonRequestHandler is not initialized. Add ');
    }

    // Convert the request payload to a JSON string
    const requestAsJson = JSON.stringify(request);

    log("DispatchRequest", `Dispatching request of Type ${requestTypeFullName}: ${requestAsJson}`, "function",);

    // Invoke the .NET method to handle the request
    await this.jsonRequestHandler.invokeMethodAsync(JsonRequestHandlerMethodName, requestTypeFullName, requestAsJson);
  }
}

export const timeWarpState = new TimeWarpState();
