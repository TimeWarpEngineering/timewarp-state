# Sync Configurable Files Workflow

This workflow automatically synchronizes configurable files from multiple repositories to child repositories, ensuring consistency across multiple projects. It features an enhanced repos-based configuration system with advanced path transformation capabilities.

## Overview

The sync workflow runs on a scheduled basis (default: every Monday at 9:00 AM UTC) and can also be triggered manually. It downloads specified files from configured repositories with intelligent path mapping and creates pull requests in child repositories when updates are detected.

## Files

- `sync-configurable-files.yml` - The main GitHub Actions workflow
- `sync-config.yml` - Configuration file defining which files to sync
- `SYNC_WORKFLOW.md` - This documentation file

## Configuration

### Enhanced Repos-Based Configuration

The workflow uses an enhanced configuration system via `.github/sync-config.yml` that supports:

- **Multiple repositories**: Sync from different repositories with specific configurations
- **Path transformations**: Automatic prefix removal and path mapping
- **Default behaviors**: Intelligent destination path defaults
- **Repository-specific settings**: Branch and transformation rules per repository
- **Schedule management**: Cron expression for automated runs
- **Pull request settings**: Configuration for generated PRs

Example enhanced configuration:
```yaml
# Global defaults
default_repo: 'TimeWarpEngineering/timewarp-architecture'
default_branch: 'master'

# Sync schedule
schedule:
  cron: '0 9 * * 1'  # Every Monday at 9:00 AM UTC

# Repository-specific configurations
repos:
  - repo: 'TimeWarpEngineering/timewarp-architecture'
    branch: 'master'
    path_transform:
      remove_prefix: 'TimeWarp.Architecture/'  # Auto-remove prefix from dest paths
    files:
      # When dest_path is omitted, defaults to source_path with prefix removal
      - source_path: 'TimeWarp.Architecture/.gitignore'
      - source_path: 'TimeWarp.Architecture/.editorconfig'
      - source_path: 'TimeWarp.Architecture/.github/workflow-templates/'
      
      # Explicit destination override
      - source_path: 'TimeWarp.Architecture/templates/custom.md'
        dest_path: 'docs/custom-template.md'

# Global sync options
sync_options:
  default_dest_to_source: true  # Auto-default dest_path to source_path
  overwrite_existing: true
  ignore_missing: false

# Files to exclude globally
exclude_files:
  - '.github/sync-config.yml'
```

### Manual Trigger Options

The workflow can be manually triggered with custom parameters:

- **parent_repo**: Override the parent repository
- **parent_branch**: Override the parent branch
- **files_to_sync**: Comma-separated list of files (overrides config file)
- **use_config_file**: Whether to use the configuration file (default: true)

## How It Works

1. **Enhanced Configuration Loading**: Loads repos-based settings from `sync-config.yml`
2. **Path Processing**: Applies path transformations and defaults destination paths
3. **Multi-Repository Download**: Downloads files from configured repositories with parallel processing
4. **Intelligent Path Mapping**: Maps source paths to destination paths with prefix removal
5. **File Comparison**: Compares downloaded files with current repository files using hash comparison
6. **PR Creation**: Creates a pull request with detailed changes if differences are found
7. **Comprehensive Summary**: Provides detailed summary including file specifications and transformations

## Supported File Types

The workflow can sync any type of file, including:

- Configuration files (`.gitignore`, `.editorconfig`, etc.)
- Workflow templates
- Build configuration files
- Documentation templates
- Code quality configurations

## Security Considerations

- Uses `GITHUB_TOKEN` or `SYNC_PAT` for authentication
- Only syncs explicitly configured files through repos-based structure
- Creates PRs for review rather than direct commits
- Supports `SYNC_PAT` for workflows requiring special permissions
- Automatically excludes workflow files when `SYNC_PAT` is not available
- Path transformations are controlled and validated

## Troubleshooting

### Common Issues

1. **Permission Denied**: Ensure the repository has proper permissions for the GitHub Actions bot
2. **File Not Found**: Check that files exist in the configured repositories at the specified branches
3. **PR Creation Failed**: Verify that the repository allows PR creation from GitHub Actions
4. **Configuration Parse Error**: Verify the repos-based configuration structure is correct
5. **Path Transform Issues**: Check that prefix removal settings match actual source paths

### Debugging

1. Check the workflow run logs for detailed error messages and file specifications
2. Verify the repos-based configuration file syntax with a YAML validator
3. Test path transformations manually to ensure they work as expected
4. Use manual trigger to test specific repository and file combinations
5. Review source repository permissions and branch access
6. Check if `SYNC_PAT` is needed for workflow files

## Best Practices

1. **Start Small**: Begin with a few critical files before expanding the configuration
2. **Review PRs**: Always review generated PRs before merging to verify transformations
3. **Test Configuration**: Use manual triggers to test repos-based configuration changes
4. **Use Path Transformations**: Leverage `remove_prefix` to clean up destination paths
5. **Default Behavior**: Rely on `default_dest_to_source` for simplified configuration
6. **Regular Maintenance**: Periodically review and update the repos-based sync configuration
7. **Security**: Keep sensitive files out of the sync configuration and use `SYNC_PAT` when needed

## Customization

### Changing the Schedule

Modify the cron expression in either the workflow file or configuration:

```yaml
schedule:
  cron: "0 9 * * 1"  # Every Monday at 9 AM UTC
```

### Adding Custom Logic

The workflow can be extended with additional steps for:

- Custom file processing
- Conditional synchronization
- Integration with other tools
- Notification systems

### Branch Protection

Consider setting up branch protection rules to:

- Require PR reviews
- Run status checks
- Enable auto-merge for trusted updates

## Enhanced Configuration Examples

### Sync Development Configuration with Path Transformation

```yaml
repos:
  - repo: 'TimeWarpEngineering/timewarp-architecture'
    branch: 'master'
    path_transform:
      remove_prefix: 'TimeWarp.Architecture/'
    files:
      - source_path: 'TimeWarp.Architecture/.gitignore'
      - source_path: 'TimeWarp.Architecture/.editorconfig'
      - source_path: 'TimeWarp.Architecture/.eslintrc.js'
      - source_path: 'TimeWarp.Architecture/.prettierrc.json'
```

### Multi-Repository Configuration

```yaml
repos:
  - repo: 'TimeWarpEngineering/timewarp-architecture'
    branch: 'master'
    path_transform:
      remove_prefix: 'TimeWarp.Architecture/'
    files:
      - source_path: 'TimeWarp.Architecture/Directory.Build.targets'
      - source_path: 'TimeWarp.Architecture/NuGet.config'
  
  - repo: 'TimeWarpEngineering/templates'
    branch: 'main'
    files:
      - source_path: 'workflows/ci.yml'
        dest_path: '.github/workflows/ci-template.yml'
      - source_path: 'docs/README.template.md'
```

### Complex Path Mapping Example

```yaml
sync_options:
  default_dest_to_source: true

repos:
  - repo: 'TimeWarpEngineering/timewarp-architecture'
    branch: 'master'
    path_transform:
      remove_prefix: 'TimeWarp.Architecture/'
    files:
      # Will become: .templates/ (prefix removed)
      - source_path: 'TimeWarp.Architecture/.templates/'
      
      # Explicit override ignores prefix removal
      - source_path: 'TimeWarp.Architecture/docs/CONTRIBUTING.md'
        dest_path: 'CONTRIBUTING.md'
      
      # Default behavior: source becomes dest with prefix removed
      - source_path: 'TimeWarp.Architecture/tools/build.ps1'  # → tools/build.ps1
```