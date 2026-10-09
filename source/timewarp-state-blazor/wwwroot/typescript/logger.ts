// Logger.ts
export interface LogStyles {
  info: string;
  success: string;
  warning: string;
  error: string;
  function: string;
}

export const logStyles: LogStyles = {
  info: "color: deepskyblue; font-weight: bold;",
  success: "color: limegreen; font-weight: bold;",
  warning: "color: darkorange; font-weight: bold;",
  error: "color: crimson; font-weight: bold;",
  function: "color: mediumorchid; font-weight: bold;",
};

export type LogLevel = keyof LogStyles;

export enum LogAction {
  Begin, 
  End    
}

export const log = (tag: string, message: string, level: LogLevel = 'info', action?: LogAction): void => {
  const style = logStyles[level];

  if (action === LogAction.Begin) {
    console.group(`%c${tag}`, style);
  }

  console.log(`%c${tag} ${message}`, style);

  if (action === LogAction.End) {
    console.groupEnd();
  }
};
