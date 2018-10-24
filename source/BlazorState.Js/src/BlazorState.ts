import {  JsonRequestHandlerMethodName, JsonRequestHandlerName } from './Constants';

export class BlazorState {
  async DispatchRequest(requestTypeFullName, request ) {
    const requestAsJson = JSON.stringify(request);

    console.log(`Dispatching request of Type ${requestTypeFullName}: ${requestAsJson}`);
    await window[JsonRequestHandlerName].invokeMethodAsync(JsonRequestHandlerMethodName, requestTypeFullName, requestAsJson);
  }
}