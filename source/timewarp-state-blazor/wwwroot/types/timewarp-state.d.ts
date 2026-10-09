import { DotNetReference } from './dot-net-reference';
import { ReduxDevTools } from "./redux-dev-tools";
export declare class TimeWarpState {
    jsonRequestHandler: DotNetReference;
    reduxDevTools: ReduxDevTools;
    DispatchRequest(requestTypeFullName: string, request: unknown): Promise<void>;
}
export declare const timeWarpState: TimeWarpState;
