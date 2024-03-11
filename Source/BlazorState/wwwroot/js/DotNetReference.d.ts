export interface DotNetReference {
    invokeMethodAsync(methodName: string, requestTypeFullName: string, requestAsJson: string): Promise<void>;
}
