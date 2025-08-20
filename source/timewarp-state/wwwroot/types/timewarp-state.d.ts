import { DotNetReference } from './DotNetReference';
import { ReduxDevTools } from "./ReduxDevTools";
export declare class TimeWarpState {
    jsonRequestHandler: DotNetReference;
    reduxDevTools: ReduxDevTools;
    DispatchRequest(requestTypeFullName: string, request: unknown): Promise<void>;
}
export declare const timeWarpState: TimeWarpState;
