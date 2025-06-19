# Task 031: Split NuGet Publishing to Release Workflow

## Description

- Split the CI/CD workflow to publish NuGet packages only on Release creation instead of merging to master branch
- This will provide better control over when packages are published and align with semantic versioning practices

## Requirements

- NuGet packages should only be published when a GitHub Release is created
- Master branch merges should run tests and build validation but not publish packages
- Release workflow should include version validation and proper tagging
- Maintain existing build and test automation on master branch

## Checklist

### Design
- [x] Analyze current GitHub Actions workflows
- [x] Design separate workflow for release publishing
- [x] Plan version management strategy

### Implementation
- [x] Create new release workflow (.github/workflows/release-publish.yml)
- [x] Modify existing master workflow to exclude NuGet publishing
- [x] Add release trigger configuration
- [x] Update NuGet package versioning logic
- [ ] Test workflow with draft release

### Documentation
- [ ] Update README.md with new release process
- [x] Document release workflow in Claude.md
- [x] Update DevOps documentation with release process

### Review
- [ ] Consider Security Implications (NuGet API keys, workflow permissions)
- [ ] Consider Performance Implications (workflow efficiency)
- [ ] Code Review of workflow changes
- [ ] Test release process end-to-end

## Notes

- Current workflow likely publishes on master branch merge
- Need to ensure GitHub Release creation triggers NuGet publishing
- Consider using GitHub's built-in release management features
- May need to update package versioning to align with release tags

## Implementation Notes

### Completed Changes:
1. **Analyzed existing workflows**: 
   - `release-build.yml` was doing both master builds AND releases
   - `ci-build.yml` handles PR validation only
   
2. **Split workflows**:
   - Renamed `release-build.yml` to `master-build.yml` - now only builds/tests on master
   - Created `release-publish.yml` - only publishes NuGet on GitHub release creation
   
3. **Key improvements**:
   - Master workflow now uses same build/test scripts as PR workflow for consistency
   - Release workflow validates version matches tag before publishing
   - Added publishing for all packages (Analyzer and SourceGenerator were missing)
   - Improved output variable syntax (deprecated ::set-output)

### Next Steps:
- Test with draft release to validate end-to-end workflow
- Update documentation with new release process