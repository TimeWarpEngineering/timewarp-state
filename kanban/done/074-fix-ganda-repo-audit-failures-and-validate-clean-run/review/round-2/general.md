# Round 2 — general
**Date:** 2026-09-04
**Scope reviewed:** M1 fix delta (version SSOT assert) plus re-verify prior M1

## Summary

Uncommitted M1 fix adds `AssertVersionSsot` to DevCli `workflow` (PR and release) and matching pwsh asserts in `.github/workflows/workflow.yml` (ci job + release `extract_version`). Re-verified both literals are `12.0.0-beta.3`, XPath/`LocalName` reads hit the intended single nodes, and mismatch paths set exit/throw. Probe correctly skips `extract_version` (no pack/publish). No new defects in the fix delta.

## Issues

## Prior IDs

- M1 (suggestion): verified-fixed
