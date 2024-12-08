# Task 017: Push To Production

## Description

Prepare TimeWarp.State for production release by updating version numbers, removing prerelease flags, and ensuring all production readiness requirements are met.

## Requirements

- Update version in Directory.Build.props from 11.0.0-beta.96+8.0.403 to 11.0.0
- Remove --prerelease flags from all sample project package references
- Ensure all documentation is up to date
- Verify all samples are working with production version
- Update release notes

## Checklist

### Design
- [ ] Review current version number and update plan
- [ ] Review all sample projects for prerelease dependencies

### Implementation
- [ ] Update TimeWarpStateVersion in Directory.Build.props to 11.0.0
- [ ] Remove --prerelease flags from all sample project package references:
  - [ ] Sample00-StateActionHandler
  - [ ] Sample01-ReduxDevTools
  - [ ] Sample02-ActionTracking
  - [ ] Sample03-Routing
- [ ] Run all samples to verify they work with production version
- [ ] Update packages.lock.json files if needed
- [ ] Run all tests to ensure everything passes
- [ ] Build and verify NuGet packages

### Documentation
- [ ] Update Release Notes for 11.0.0
- [ ] Review and update main documentation
- [ ] Update sample documentation to remove prerelease references
- [ ] Verify documentation build and deployment
- [ ] Update README.md if needed

### Review
- [ ] Review all breaking changes
- [ ] Verify all features are production-ready
- [ ] Check for any deprecated features or warnings
- [ ] Review performance metrics
- [ ] Review security implications
- [ ] Conduct final code review
- [ ] Test package installation in a new project

## Notes

- This release marks the transition from beta to production for version 11.0.0
- All samples should be updated to use the production version
- Documentation should reflect production status
- Consider creating a release checklist template for future releases

## Implementation Notes

- Add implementation notes as the task progresses
