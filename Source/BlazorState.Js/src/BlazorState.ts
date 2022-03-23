import {  JsonRequestHandlerMethodName, JsonRequestHandlerName } from './Constants.js';

export class BlazorState {
  async DispatchRequest(requestTypeFullName: string, request: any ) {
    const requestAsJson = JSON.stringify(request);

    console.log(`Dispatching request of Type ${requestTypeFullName}: ${requestAsJson}`);
    await (<any>window[<any>JsonRequestHandlerName]).invokeMethodAsync(JsonRequestHandlerMethodName, requestTypeFullName, requestAsJson);
  }
}
