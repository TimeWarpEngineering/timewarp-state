# Sourcegraph Code Search Tool

You have access to the `sourcegraph` command which queries a local Sourcegraph instance via GraphQL API.

## Command Usage

```bash
sourcegraph '<GraphQL query>'
```

## Discovering Available Repositories

To see what repositories are indexed:

```bash
# Get total count
sourcegraph 'query { repositories { totalCount } }'

# List all repositories
sourcegraph 'query { repositories(first: 100) { nodes { name } } }'

# List repositories with their source type
sourcegraph 'query { repositories(first: 100) { nodes { name externalRepository { serviceType } } } }'
```

The instance indexes repositories from `/home/steventcramer/repos` via src serve-git, including various open source projects and TimeWarpEngineering repositories.

## Common Queries

### Search for code patterns
```bash
sourcegraph 'query { search(query: "context:global functionName", patternType: literal) { results { results { ... on FileMatch { file { path } repository { name } lineMatches { preview lineNumber } } } } } }'
```

### Search with regex
```bash
sourcegraph 'query { search(query: "context:global class\\s+\\w+Controller", patternType: regexp) { results { results { ... on FileMatch { file { path } repository { name } lineMatches { preview lineNumber } } } } } }'
```

### List repositories
```bash
sourcegraph 'query { repositories(first: 50) { nodes { name } } }'
```

### Search in specific repo
```bash
sourcegraph 'query { search(query: "context:global repo:^github\\.com/TimeWarpEngineering/timewarp-code searchTerm", patternType: literal) { results { results { ... on FileMatch { file { path } repository { name } lineMatches { preview lineNumber } } } } } }'
```

### Find TODOs
```bash
sourcegraph 'query { search(query: "context:global TODO|FIXME", patternType: regexp) { results { results { ... on FileMatch { file { path } repository { name } lineMatches { preview lineNumber } } } } } }'
```

## Notes
- Use this tool when the user asks about code in any of the indexed repositories
- The tool returns JSON results that you should parse and present clearly
- Authentication is already configured via environment variables
- For complex queries, results may be large - consider using `first: N` to limit results