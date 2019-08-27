type DotNetType = {
  invokeMethod<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): T,
  invokeMethodAsync<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): Promise<T>
}

export const DotNet: DotNetType = window["DotNet"];