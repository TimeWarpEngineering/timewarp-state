import './GlobalExports';
import { LogObject, LogObjectType, LogLevel } from './LogObject';

// Declare types here until we've Blazor.d.ts
export interface System_Object { System_Object__DO_NOT_IMPLEMENT: any };
export interface System_String extends System_Object { System_String__DO_NOT_IMPLEMENT: any }

interface Platform {
  toJavaScriptString(dotNetString: System_String): string;
}

type BlazorType = {
  registerFunction(identifier: string, implementation: Function),
  platform: Platform
};

const Blazor: BlazorType = window["Blazor"];


function initialize() {
  "use strict";
  console.log("Initialize Logging");

  Blazor.registerFunction('Blazor.Extensions.Logging.BrowserConsoleLogger.Log', function (logObjectValue) {
    const logObjectString = Blazor.platform.toJavaScriptString(logObjectValue);
    const logObject: LogObject = JSON.parse(logObjectString);

    if (!logObject) {
      console.error('Invalid logObject received: ', logObjectString ? logObjectString : '<null>');
      return;
    }

    var logMethod = console.log;

    // If we've a table, we'll print it as a table anyway, it is unlikely that the developer want to log errornous data as a table.
    if (logObject.type == LogObjectType.Table) {
      logMethod = console.table;
    } else {
      switch (logObject.logLevel) {
        case LogLevel.Trace:
          logMethod = console.trace;
          break;
        case LogLevel.Debug:
          logMethod = console.debug;
          break;
        case LogLevel.Warning:
          logMethod = console.warn;
          break;
        case LogLevel.Error:
        case LogLevel.Critical:
          logMethod = console.error;
          break;
      }
    }

    logMethod(logObject.payload);

    if (logObject.exception) {
      logMethod("Exception: ", logObject.exception);
    }
  });
}

initialize();
