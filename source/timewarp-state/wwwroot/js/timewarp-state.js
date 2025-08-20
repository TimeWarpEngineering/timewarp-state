import { JsonRequestHandlerMethodName } from './Constants.js';
import { log } from './Logger.js';
export class TimeWarpState {
    jsonRequestHandler;
    reduxDevTools;
    async DispatchRequest(requestTypeFullName, request) {
        if (!this.jsonRequestHandler) {
            throw new Error('jsonRequestHandler is not initialized. Add ');
        }
        const requestAsJson = JSON.stringify(request);
        log("DispatchRequest", `Dispatching request of Type ${requestTypeFullName}: ${requestAsJson}`, "function");
        await this.jsonRequestHandler.invokeMethodAsync(JsonRequestHandlerMethodName, requestTypeFullName, requestAsJson);
    }
}
export const timeWarpState = new TimeWarpState();
//# sourceMappingURL=TimeWarpState.js.map