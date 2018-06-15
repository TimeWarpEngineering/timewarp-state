export type LogObject = {
  logLevel: LogLevel;
  type: LogObjectType;
  payload: any;
  exception: string;
}

// Enum coming from Microsoft.Extensions.Logging
export enum LogLevel {
  Trace = 0,
  Debug = 1,
  Information = 2,
  Warning = 3,
  Error = 4,
  Critical = 5,
  None = 6
}

export enum LogObjectType {
  String = 0,
  Object = 1,
  Table = 2
}
