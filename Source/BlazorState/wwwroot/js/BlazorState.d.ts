import { DotNetReference } from './DotNetReference';
import { ReduxDevTools } from "./ReduxDevTools";
export declare class BlazorState {
    jsonRequestHandler: DotNetReference;
    reduxDevTools: ReduxDevTools;
    DispatchRequest(requestTypeFullName: string, request: unknown): Promise<void>;
}
export declare const blazorState: BlazorState;
