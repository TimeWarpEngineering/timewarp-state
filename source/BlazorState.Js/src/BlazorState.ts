import {  JsonRequestHandlerMethodName, JsonRequestHandlerName } from './Constants';

export class BlazorState {
  async DispatchRequest(request) {
    const requestAsJson = JSON.stringify(request);

    console.log(`Dispatching request: ${requestAsJson}`);
    await window[JsonRequestHandlerName].invokeMethodAsync(JsonRequestHandlerMethodName, requestAsJson);
  }
}