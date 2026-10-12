import { DotNetReference } from './dot-net-reference';
export declare class TimeWarpState {
    jsonRequestHandler: DotNetReference;
    DispatchRequest(requestTypeFullName: string, request: unknown): Promise<void>;
}
export declare const timeWarpState: TimeWarpState;
