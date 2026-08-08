# Add trusted-publishing probe mode to workflow.yml

## Description

Org 458-009 probe (NuGet has no policy-enumeration API): a workflow_dispatch
mode that runs ONLY the nuget/login OIDC exchange and stops — success proves an
active trusted publishing policy matches this repo + workflow.yml. Reference:
timewarp-nuru workflow.yml.

## Checklist

- [x] probe added to dispatch inputs
- [x] nuget/login if-condition extended with the probe clause
- [x] "Trusted publishing probe result" step added after login
- [x] Pipeline/heavy steps skip in probe mode (or were already mode-gated)
- [x] YAML validated

## Results

Implemented directly by the 458 orchestration session (2026-08-08) after the
delegated batch worker for this repo died mid-wave.

### How to validate

Smoke: after push, `gh workflow run workflow.yml -f mode=probe` → expect the
run to succeed with the probe-result step green. A failure at the NuGet login
step means the trusted publishing policy is missing/misconfigured on NuGet.org.
