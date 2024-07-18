# RenderSubscriptionContext

The `RenderSubscriptionContext` class provides Handlers with control over when subscriptions should be re-rendered.

## Purpose

This class offers a mechanism for Handlers to manage the automatic re-rendering of subscriptions for specific actions, allowing for fine-grained control over the rendering process.

## Usage in Action Handlers

To modify the default re-rendering behavior in your Handler, follow these steps:

TODO: Insert real example code and maybe a link

[Link to line 10](./path/to/file.cs:10)

## Cross-Middleware Communication

It's important to note that any middleware or handler within the pipeline can inject the `RenderSubscriptionContext` and thereby affect the re-rendering process. This capability enables cross-middleware communication, allowing different parts of the application to influence when and how subscriptions are re-rendered.
