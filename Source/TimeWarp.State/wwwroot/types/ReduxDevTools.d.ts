import { TimeWarpState } from './TimeWarpState.js';
type Config = any;
type ConnectResponse = any;
type ReduxDevtoolsExtension = any;
export declare class ReduxDevTools {
    IsEnabled: boolean;
    DevTools: ConnectResponse;
    Extension: ReduxDevtoolsExtension;
    Config: Config;
    TimeWarpState: TimeWarpState;
    StackTrace: string | undefined;
    private IsInitialized;
    constructor(reduxDevToolsOptions: Config);
    Init(): void;
    GetExtension(): ReduxDevtoolsExtension | undefined;
    GetDevTools(): ConnectResponse | undefined;
    MapRequestType(message: any): string;
    MessageHandler: (message: any) => void;
    ReduxDevToolsDispatch(action: any, state: unknown, stackTrace: any): any;
    GetStackTraceForAction(_action: any): string;
}
export {};
