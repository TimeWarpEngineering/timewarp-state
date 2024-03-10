export interface DotNetReference {
  invokeMethodAsync(methodName: string, requestTypeFullName: string, requestAsJson: string): Promise<void>;
}

// Optionally, if you have more methods or properties, add them here
