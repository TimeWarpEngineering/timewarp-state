import { BlazorState } from './BlazorState.js';
type traceType = (action: any) => string;
export declare class ReduxDevTools {
    IsEnabled: boolean;
    DevTools: any;
    Extension: any;
    Config: {
        name: string;
        trace: boolean | traceType;
        features: {
            pause: boolean;
            lock: boolean;
            persist: boolean;
            export: boolean;
            import: boolean;
            jump: boolean;
            skip: boolean;
            reorder: boolean;
            dispatch: boolean;
            test: boolean;
        };
    };
    BlazorState: BlazorState;
    StackTrace: string | undefined;
    private IsInitialized;
    constructor(reduxDevToolsOptions: any);
    Init(): void;
    GetExtension(): any;
    GetDevTools(): any;
    MapRequestType(message: any): any;
    MessageHandler: (message: any) => void;
    ReduxDevToolsDispatch(action: any, state: any, stackTrace: any): any;
    GetStackTraceForAction(action: any): string;
}
export {};
