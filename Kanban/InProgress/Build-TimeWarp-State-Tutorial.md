# Task 001: Build TimeWarp.State Tutorial

## Description
Create a comprehensive tutorial for TimeWarp.State that walks users through building a Blazor application with state management. The tutorial will demonstrate core concepts and best practices through practical examples.

## Requirements
- Tutorial must be clear and beginner-friendly
- Cover essential TimeWarp.State concepts:
  1. State class implementation and initialization
  2. Action definition and structure
  3. Handler implementation for state mutation
  4. Component integration with TimeWarpStateComponent
  5. Pipeline flow: Component -> Action -> Handler -> State
  6. State immutability and proper mutation patterns
- Include practical, working examples
- Provide clear explanations for each step
- Follow consistent naming convention:
  * Use 'Sample00' for first tutorial project
  * Subsequent tutorials will use 'Sample01', 'Sample02', etc.
  * Project names should align with tutorial directory numbers
- Cover Blazor render modes:
  * Interactive Auto (current tutorial)
  * Interactive Server (planned)
  * Interactive WebAssembly (planned)
  * Static (no state management needed)

## Checklist

### Design
- [x] Plan tutorial structure
- [x] Design basic counter example
- [x] Add render mode metadata to tutorial
- [ ] Plan additional render mode tutorials

### Implementation
- [x] Basic project setup instructions
- [x] Service configuration steps
- [x] Counter state implementation
- [x] Action and Handler implementation
- [x] UI integration
- [x] Standardize project naming convention

### Documentation
- [x] Basic tutorial documentation in Readme.md
- [ ] Create tutorials for Server and WebAssembly modes

### Review
- [ ] Tutorial walkthrough verification
- [ ] Test across all supported render modes

## Notes
- Base tutorial content exists in Samples/00-StateActionHandler/Readme.md
- Include common pitfalls and how to avoid them:
  1. Direct state mutation outside handlers
  2. Improper state initialization
  3. Missing service configuration
- Consider adding diagrams for:
  1. State-Action-Handler flow
  2. Component interaction with state
- Project naming follows pattern: Sample[XX]
- Each tutorial should clearly indicate its render mode
- Future tutorials needed for Server and WebAssembly modes

## Implementation Notes
- Initial counter example implemented and working
- Service configuration documented
- Basic state management flow demonstrated
- Project renamed from 'Sample' to 'Sample00'
- Added render mode metadata to tutorial
