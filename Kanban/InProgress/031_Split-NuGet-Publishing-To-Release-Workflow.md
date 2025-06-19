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
- [ ] Analyze current GitHub Actions workflows
- [ ] Design separate workflow for release publishing
- [ ] Plan version management strategy

### Implementation
- [ ] Create new release workflow (.github/workflows/release.yml)
- [ ] Modify existing master workflow to exclude NuGet publishing
- [ ] Add release trigger configuration
- [ ] Update NuGet package versioning logic
- [ ] Test workflow with draft release

### Documentation
- [ ] Update README.md with new release process
- [ ] Document release workflow in CLAUDE.md
- [ ] Update contributor guidelines

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

- Review .github/workflows/ directory for existing workflows
- Check how NuGet API keys are currently managed in GitHub secrets
- Ensure release workflow has proper permissions for NuGet publishing