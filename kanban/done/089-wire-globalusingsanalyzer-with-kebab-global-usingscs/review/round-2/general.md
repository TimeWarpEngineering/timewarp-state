# Round 2 — general
**Date:** 2026-09-21
**Scope reviewed:** branch task/089-wire-globalusingsanalyzer-with-kebab-global-usings vs origin/master (retarget product commit 45686cd9)

## Summary

Retarget drops BDSoftware GlobalUsingsAnalyzer and enables TW0007 via TimeWarp.SourceGenerators 1.0.0-beta.11 (CPM bump from beta.4), with kebab `global-usings.cs` and severity warning under `[*.cs]`. The first-implement using-fold is intact; `TreatWarningsAsErrors` stays false, so leftover one-file TW0007 warnings are expected and non-blocking. Risk is low: package pin, PackageReference, editorconfig, and `ganda repo audit` `global-usings-analyzer` all match the brief; Plus-tests build emits `warning TW0007` for Timers (and similar leftovers) with Build succeeded.

## Issues
