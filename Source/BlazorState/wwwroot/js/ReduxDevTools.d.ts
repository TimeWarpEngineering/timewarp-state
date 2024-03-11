import { BlazorState } from './BlazorState.js';
import type { Config } from '@redux-devtools/extension';
import type { ConnectResponse, ListenerMessage, ReduxDevtoolsExtension } from './ReduxDevToolsTypes';
import { Action } from 'redux';
export declare class ReduxDevTools {
    IsEnabled: boolean;
    DevTools: ConnectResponse;
    Extension: ReduxDevtoolsExtension;
    Config: Config;
    BlazorState: BlazorState;
    StackTrace: string | undefined;
    private IsInitialized;
    constructor(reduxDevToolsOptions: Config);
    Init(): void;
    GetExtension(): ReduxDevtoolsExtension | undefined;
    GetDevTools(): ConnectResponse | undefined;
    MapRequestType(message: ListenerMessage<unknown, Action>): string;
    MessageHandler: (message: ListenerMessage<unknown, Action>) => void;
    ReduxDevToolsDispatch(action: Action<string>, state: unknown, stackTrace: any): any;
    GetStackTraceForAction(_action: any): string;
}
