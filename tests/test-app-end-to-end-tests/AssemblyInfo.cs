#region Purpose
// Lets MSTest run the end-to-end tests in parallel at method level.
#endregion

#region Design
// Each test gets its own Playwright page, so tests do not share browser state.
#endregion

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
