import { Platform } from './Platform';

type BlazorType = {
  registerFunction(identifier: string, implementation: Function),
  platform: Platform
};

export const Blazor: BlazorType = window["Blazor"];