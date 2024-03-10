import { DotNetReference } from './DotNetReference';
import { ReduxDevTools } from "./ReduxDevTools";
export declare class BlazorState {
    jsonRequestHandler: DotNetReference;
    reduxDevTools: ReduxDevTools;
    DispatchRequest(requestTypeFullName: string, request: any): Promise<void>;
}
export declare const blazorState: BlazorState;
