# Round 1 — general
**Date:** 2026-09-21
**Scope reviewed:** branch `task/068-make-subscriptions-thread-safe-and-indexed` vs `origin/master`

## Summary

`Subscriptions` replaces the unlocked `List<Subscription>` with a forward index (`SubscriptionsByStateType`) and reverse index (`StateTypesByComponentId`) under one private `SyncRoot` lock. Snapshots for `ReRenderSubscribers` are taken under the lock; `ShouldReRender` / `ReRender` run outside it, then dead `WeakReference` cleanup re-enters the lock — addressing finding 13 (Blazor Server race/deadlock) and finding 20 (O(n) scans) without changing public `Add` / `Remove` / `ReRenderSubscribers` signatures. Risk is low: call sites are unchanged, indexes stay paired on Add/Remove/dead cleanup (including the live-replacement skip), and the two required unit tests plus existing client-integration coverage match the new semantics.

## Issues
