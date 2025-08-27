# Remove Redundant Render Mode Tracking

## Overview
Now that .NET 9 has introduced `RendererInfo` with properties like `Name`, `IsInteractive`, and components have `AssignedRenderMode`, our custom render mode tracking implementation has become redundant. We should refactor to use the built-in .NET 9 features instead.

## Tasks
- [ ] Remove custom `CurrentRenderMode` property from TimeWarpStateComponent
- [ ] Remove custom `ConfiguredRenderMode` property from TimeWarpStateComponent  
- [ ] Update all components to use `RendererInfo.Name` for current render mode
- [ ] Update all components to use `AssignedRenderMode` for the assigned mode
- [ ] Update all components to use `RendererInfo.IsInteractive` for interactivity check
- [ ] Remove any custom render mode detection logic
- [ ] Update tests to verify the new .NET 9 properties work correctly
- [ ] Update documentation to reflect the use of .NET 9 built-in features

## Notes
- Microsoft essentially implemented what we had already built, validating our approach
- Keep the RenderModeDisplay component for debugging purposes but have it use the .NET 9 properties
- This cleanup can wait until after beta.2 release