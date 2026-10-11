# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** same as framework

## Summary

The change makes the DevTools resource declare its own `http` endpoint, so it has an endpoint whether or not Aspire finds a launch profile. It also renames launch-settings folders to `Properties/`, the only path Aspire reads. Re-verified: `LabelTheDashboardLinkAndPublishTheEndpoint` asserts a single `http` endpoint with the profile port 5288, so Aspire merges a same-named `WithHttpEndpoint` into the profile endpoint and does not throw. `DeclareTheHttpEndpointWithoutALaunchProfile` covers the no-profile case. `dotnet fixie` in `tests/timewarp-state-devtools-tests` passed 30 tests. `ganda repo audit` exits 0; the `kebab-path-names` advisory lists only pre-existing paths. No lowercase `properties/` references remain outside kanban. The sample-04 `UseReduxDevTools` change is needed because the server now runs as Development and validates the container, and the comment explains why. Overall risk is low.

## Issues

None.
