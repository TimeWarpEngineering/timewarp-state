# timewarp-state: Purpose/Design region audit (master 29990f46+, 2026-10-09)

Total .cs files: 387. Both regions: 67. Missing Design only: 38. Missing Purpose only: 0. Missing both: 282.

320 files missing at least one region, grouped by area (work order). Each entry is tagged with what is missing: `[both]` = neither region, `[Design]` = has Purpose, missing Design.

| Area | Files | Missing both | Missing Design only |
|---|---|---|---|
| `source/timewarp-state` | 48 | 48 | 0 |
| `source/timewarp-state-plus` | 29 | 29 | 0 |
| `source/timewarp-state-policies` | 9 | 9 | 0 |
| `source/timewarp-state-analyzer` | 6 | 6 | 0 |
| `source/timewarp-state-source-generator` | 4 | 2 | 2 |
| `source/timewarp-state-telemetry` | 4 | 2 | 2 |
| `tests/test-app` | 72 | 71 | 1 |
| `tests/client-integration-tests` | 19 | 18 | 1 |
| `tests/test-app-end-to-end-tests` | 18 | 18 | 0 |
| `tests/timewarp-state-tests` | 14 | 7 | 7 |
| `tests/timewarp-state-plus-tests` | 12 | 6 | 6 |
| `tests/timewarp-state-analyzer-tests` | 11 | 9 | 2 |
| `tests/timewarp-state-telemetry-tests` | 9 | 7 | 2 |
| `tests/timewarp-state-source-generator-tests` | 6 | 2 | 4 |
| `tests/test-app-architecture-tests` | 4 | 4 | 0 |
| `samples` | 42 | 32 | 10 |
| `scripts` | 7 | 7 | 0 |
| `.githooks` | 5 | 5 | 0 |
| `tools` | 1 | 0 | 1 |
| **Total** | **320** | **282** | **38** |

## `source/timewarp-state` (48)

- [both] `source/timewarp-state/assembly-marker.cs`
- [both] `source/timewarp-state/base/state-action-handler.cs`
- [both] `source/timewarp-state/components/i-timewarp-state-component.cs`
- [both] `source/timewarp-state/components/timewarp-state-component.cs`
- [both] `source/timewarp-state/components/timewarp-state-component.register-render-trigger.cs`
- [both] `source/timewarp-state/components/timewarp-state-component.render-mode.cs`
- [both] `source/timewarp-state/components/timewarp-state-component.render-reasons.cs`
- [both] `source/timewarp-state/components/timewarp-state-input-component.cs`
- [both] `source/timewarp-state/event-ids.cs`
- [both] `source/timewarp-state/extensions/method-info-extensions.cs`
- [both] `source/timewarp-state/extensions/service-collection-extensions.add-action-catalog.cs`
- [both] `source/timewarp-state/extensions/service-collection-extensions.add-javascript-dispatch.cs`
- [both] `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs`
- [both] `source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs`
- [both] `source/timewarp-state/extensions/service-collection-extensions.use-redux-dev-tools.cs`
- [both] `source/timewarp-state/extensions/timewarp-state-options.cs`
- [both] `source/timewarp-state/extensions/type-extensions.cs`
- [both] `source/timewarp-state/features/action-catalog/action-catalog-arguments.cs`
- [both] `source/timewarp-state/features/action-catalog/action-catalog-parameter.cs`
- [both] `source/timewarp-state/features/action-catalog/action-visibility.cs`
- [both] `source/timewarp-state/features/action-catalog/i-action-catalog.cs`
- [both] `source/timewarp-state/features/javascript-interop/invalid-request-type-exception.cs`
- [both] `source/timewarp-state/features/javascript-interop/javascript-dispatch-builder.cs`
- [both] `source/timewarp-state/features/javascript-interop/json-request.cs`
- [both] `source/timewarp-state/features/persistence/abstractions/i-persistence-service.cs`
- [both] `source/timewarp-state/features/persistence/attributes/persistent-state-attribute.cs`
- [both] `source/timewarp-state/features/persistence/persistent-state-method.cs`
- [both] `source/timewarp-state/features/pipeline/exception-notification.cs`
- [both] `source/timewarp-state/features/pipeline/invalid-clone-exception.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/components/timewarp-state-dev-component.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/dispatch-request.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/redux-action.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/redux-dev-tools-behavior.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/redux-dev-tools-interop.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/redux-dev-tools-options.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/requests/commit/commit-handler.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/requests/commit/commit-request.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/requests/i-redux-request.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/requests/start/start-handler.cs`
- [both] `source/timewarp-state/features/redux-dev-tools/requests/start/start-request.cs`
- [both] `source/timewarp-state/features/render-subscriptions/non-nested-class-exception.cs`
- [both] `source/timewarp-state/features/state-initialization/state-initialization-pre-processor.cs`
- [both] `source/timewarp-state/global-suppressions.cs`
- [both] `source/timewarp-state/global-usings.cs`
- [both] `source/timewarp-state/state/i-state.cs`
- [both] `source/timewarp-state/store/i-store.cs`
- [both] `source/timewarp-state/store/state-initialized-notification.cs`
- [both] `source/timewarp-state/store/store.redux-dev-tools.cs`

## `source/timewarp-state-plus` (29)

- [both] `source/timewarp-state-plus/assembly-marker.cs`
- [both] `source/timewarp-state-plus/event-ids.cs`
- [both] `source/timewarp-state-plus/extensions/assembly-extensions.cs`
- [both] `source/timewarp-state-plus/extensions/service-collection-extensions.cs`
- [both] `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.complete-processing.cs`
- [both] `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.cs`
- [both] `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.debug.cs`
- [both] `source/timewarp-state-plus/features/action-tracking/action-tracking-state/action-tracking-state.start-processing.cs`
- [both] `source/timewarp-state-plus/features/action-tracking/pipeline/track-action-attribute.cs`
- [both] `source/timewarp-state-plus/features/feature-flags/feature-flag-state/feature-flag-state.cs`
- [both] `source/timewarp-state-plus/features/persistence/load-persistent-state-request.cs`
- [both] `source/timewarp-state-plus/features/persistence/state-initialized-notification-handler.cs`
- [both] `source/timewarp-state-plus/features/routing/components/timewarp-page-render-notifier.razor.cs`
- [both] `source/timewarp-state-plus/features/routing/route-state/route-state.change-route.cs`
- [both] `source/timewarp-state-plus/features/routing/route-state/route-state.cs`
- [both] `source/timewarp-state-plus/features/routing/route-state/route-state.go-back.cs`
- [both] `source/timewarp-state-plus/features/theme/theme-state/theme-state.cs`
- [both] `source/timewarp-state-plus/features/theme/theme-state/theme-state.debug.cs`
- [both] `source/timewarp-state-plus/features/theme/theme-state/theme-state.update.cs`
- [both] `source/timewarp-state-plus/features/timers/multi-timer-options.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-config.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-elapsed-notification.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-state/timer-state.add-timer.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-state/timer-state.remove-timer.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-state/timer-state.reset-timers-on-activity.cs`
- [both] `source/timewarp-state-plus/features/timers/timer-state/timer-state.update-timer.cs`
- [both] `source/timewarp-state-plus/global-usings.cs`
- [both] `source/timewarp-state-plus/state/i-timewarp-cacheable-state.cs`
- [both] `source/timewarp-state-plus/state/timewarp-cacheable-state.cs`

## `source/timewarp-state-policies` (9)

- [both] `source/timewarp-state-policies/be-nested-in-state-custom-rule.cs`
- [both] `source/timewarp-state-policies/extensions/net-arch-extensions.cs`
- [both] `source/timewarp-state-policies/global-usings.cs`
- [both] `source/timewarp-state-policies/have-injectable-constructor.cs`
- [both] `source/timewarp-state-policies/have-json-constructor.cs`
- [both] `source/timewarp-state-policies/policies.action-handler-policy.cs`
- [both] `source/timewarp-state-policies/policies.action-policy.cs`
- [both] `source/timewarp-state-policies/policies.action-set-policy.cs`
- [both] `source/timewarp-state-policies/policies.state-policy.cs`

## `source/timewarp-state-analyzer` (6)

- [both] `source/timewarp-state-analyzer/global-usings.cs`
- [both] `source/timewarp-state-analyzer/state-implementation-analyzer.cs`
- [both] `source/timewarp-state-analyzer/state-inheritance-analyzer.cs`
- [both] `source/timewarp-state-analyzer/state-read-only-public-properties-analyzer.cs`
- [both] `source/timewarp-state-analyzer/state-symbol-helpers.cs`
- [both] `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs`

## `source/timewarp-state-source-generator` (4)

- [both] `source/timewarp-state-source-generator/action-set-method-generator.cs`
- [Design] `source/timewarp-state-source-generator/equatable-array.cs`
- [both] `source/timewarp-state-source-generator/global-usings.cs`
- [Design] `source/timewarp-state-source-generator/is-external-init.cs`

## `source/timewarp-state-telemetry` (4)

- [both] `source/timewarp-state-telemetry/event-ids.cs`
- [both] `source/timewarp-state-telemetry/global-usings.cs`
- [Design] `source/timewarp-state-telemetry/service-collection-extensions.cs`
- [Design] `source/timewarp-state-telemetry/timewarp-state-telemetry.cs`

## `tests/test-app` (72)

- [both] `tests/test-app/test-app-client/assembly-marker.cs`
- [both] `tests/test-app/test-app-client/components/custom-input.razor.cs`
- [both] `tests/test-app/test-app-client/extensions/collection-extensions.cs`
- [both] `tests/test-app/test-app-client/features/application/application-state/application-state.cs`
- [both] `tests/test-app/test-app-client/features/application/application-state/application-state.debug.cs`
- [both] `tests/test-app/test-app-client/features/application/application-state/application-state.five-second-task.cs`
- [both] `tests/test-app/test-app-client/features/application/application-state/application-state.reset-store.cs`
- [both] `tests/test-app/test-app-client/features/application/application-state/application-state.two-second-task.cs`
- [both] `tests/test-app/test-app-client/features/application/notification/application-state.exception-notification-handler.cs`
- [both] `tests/test-app/test-app-client/features/base/base-action-handler.cs`
- [both] `tests/test-app/test-app-client/features/base/components/base-component.cs`
- [both] `tests/test-app/test-app-client/features/base/components/base-input-component.cs`
- [both] `tests/test-app/test-app-client/features/blue/actions/blue-state.increment-count.cs`
- [both] `tests/test-app/test-app-client/features/blue/blue-state.cs`
- [both] `tests/test-app/test-app-client/features/cacheable-weather/actions/cacheable-weather-state.fetch-weather-forecasts.cs`
- [both] `tests/test-app/test-app-client/features/cacheable-weather/cacheable-weather-state.cs`
- [both] `tests/test-app/test-app-client/features/clone-test/actions/cloneable-state.clone-test.cs`
- [both] `tests/test-app/test-app-client/features/clone-test/clone-test-state.debug.cs`
- [both] `tests/test-app/test-app-client/features/clone-test/cloneable-state.cs`
- [both] `tests/test-app/test-app-client/features/color/actions/color-state.update.cs`
- [both] `tests/test-app/test-app-client/features/color/color-state.cs`
- [both] `tests/test-app/test-app-client/features/color/color-state.debug.cs`
- [Design] `tests/test-app/test-app-client/features/counter/actions/counter-state.add-to-count.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/counter-state.increment-counter.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/counter-state.throw-exception.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/counter-state.throw-server-side-exception.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/improper-nested-action/improper-nested-action.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/improper-nested-action/improper-nested-handler.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/non-nested-action/non-nested-action.cs`
- [both] `tests/test-app/test-app-client/features/counter/actions/non-nested-action/non-nested-handler.cs`
- [both] `tests/test-app/test-app-client/features/counter/components/counter.razor.cs`
- [both] `tests/test-app/test-app-client/features/counter/counter-state.cs`
- [both] `tests/test-app/test-app-client/features/counter/counter-state.debug.cs`
- [both] `tests/test-app/test-app-client/features/counter/notification/increment-count-notification-handler.cs`
- [both] `tests/test-app/test-app-client/features/counter/notification/pre-increment-count-notification-handler.cs`
- [both] `tests/test-app/test-app-client/features/event-stream/actions/add-event/event-stream-state.add-event-action.cs`
- [both] `tests/test-app/test-app-client/features/event-stream/event-stream-state.cs`
- [both] `tests/test-app/test-app-client/features/event-stream/event-stream-state.debug.cs`
- [both] `tests/test-app/test-app-client/features/event-stream/pipeline/event-stream-behavior.cs`
- [both] `tests/test-app/test-app-client/features/purple/actions/purple-state.increment-count.cs`
- [both] `tests/test-app/test-app-client/features/purple/purple-state.cs`
- [both] `tests/test-app/test-app-client/features/weather-forecast/actions/weather-forecasts-state.fetch-weather-forecasts.cs`
- [both] `tests/test-app/test-app-client/features/weather-forecast/weather-forecast-state.cs`
- [both] `tests/test-app/test-app-client/features/window-dimensions/window-dimensions-state.cs`
- [both] `tests/test-app/test-app-client/global-usings.cs`
- [both] `tests/test-app/test-app-client/mediator-behaviors.cs`
- [both] `tests/test-app/test-app-client/pipeline/my-behavior.cs`
- [both] `tests/test-app/test-app-client/pipeline/notification-post-processor/post-pipeline-notification-request-post-processor.cs`
- [both] `tests/test-app/test-app-client/pipeline/notification-post-processor/post-pipeline-notification.cs`
- [both] `tests/test-app/test-app-client/pipeline/notification-pre-processor/pre-pipeline-notification-request-pre-processor.cs`
- [both] `tests/test-app/test-app-client/pipeline/notification-pre-processor/pre-pipeline-notification.cs`
- [both] `tests/test-app/test-app-client/program.cs`
- [both] `tests/test-app/test-app-client/test-objects/array-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/basic-object-with-ignore.cs`
- [both] `tests/test-app/test-app-client/test-objects/basic-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/collection-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/complex-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/custom-collection-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/dictionary-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/equality-comparers.cs`
- [both] `tests/test-app/test-app-client/test-objects/i-test-interface.cs`
- [both] `tests/test-app/test-app-client/test-objects/interface-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/multi-dimensional2d-array-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/multi-dimensional3d-array-object.cs`
- [both] `tests/test-app/test-app-client/test-objects/test-enum.cs`
- [both] `tests/test-app/test-app-client/tests/clone-provider-tests.cs`
- [both] `tests/test-app/test-app-contracts/features/exception-handling/throw-server-side-exception/throw-server-side-exception-request.cs`
- [both] `tests/test-app/test-app-contracts/features/exception-handling/throw-server-side-exception/throw-server-side-exception-response.cs`
- [both] `tests/test-app/test-app-contracts/features/weather-forecast/queries/get-weather-forecasts.cs`
- [both] `tests/test-app/test-app-contracts/global-usings.cs`
- [both] `tests/test-app/test-app-server/global-usings.cs`
- [both] `tests/test-app/test-app-server/program.cs`

## `tests/client-integration-tests` (19)

- [both] `tests/client-integration-tests/caching/cacheable-state-tests.cs`
- [both] `tests/client-integration-tests/clone/test-state-clone-tests.cs`
- [both] `tests/client-integration-tests/clone/test-state.cs`
- [both] `tests/client-integration-tests/convention-tests.cs`
- [Design] `tests/client-integration-tests/features/action-catalog/action-catalog-tests.cs`
- [both] `tests/client-integration-tests/features/application/application-state-clone-tests.cs`
- [both] `tests/client-integration-tests/features/blue/blue-state-deseralization-tests.cs`
- [both] `tests/client-integration-tests/features/counter/counter-state-clone-tests.cs`
- [both] `tests/client-integration-tests/features/counter/counter-state-deseralization-tests.cs`
- [both] `tests/client-integration-tests/global-usings.cs`
- [both] `tests/client-integration-tests/infrastructure/base-test.cs`
- [both] `tests/client-integration-tests/infrastructure/client-host-builder.cs`
- [both] `tests/client-integration-tests/infrastructure/client-host.cs`
- [both] `tests/client-integration-tests/infrastructure/testing-convention.cs`
- [both] `tests/client-integration-tests/pipeline/action-tracking-tests.cs`
- [both] `tests/client-integration-tests/pipeline/render-subscription-context-tests.cs`
- [both] `tests/client-integration-tests/pipeline/state-transaction-tests.cs`
- [both] `tests/client-integration-tests/store/store-lifecycle-tests.cs`
- [both] `tests/client-integration-tests/subscriptions/subscriptions-tests.cs`

## `tests/test-app-end-to-end-tests` (18)

- [both] `tests/test-app-end-to-end-tests/AssemblyInfo.cs`
- [both] `tests/test-app-end-to-end-tests/cacheable-weather-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/change-route-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/configuration.cs`
- [both] `tests/test-app-end-to-end-tests/configured-render-modes.cs`
- [both] `tests/test-app-end-to-end-tests/counter-page-test.cs`
- [both] `tests/test-app-end-to-end-tests/event-stream-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/global-usings.cs`
- [both] `tests/test-app-end-to-end-tests/go-back-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/home-page-test.cs`
- [both] `tests/test-app-end-to-end-tests/javascript-interop-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/page-utilities.cs`
- [both] `tests/test-app-end-to-end-tests/persistence-test-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/render-modes.cs`
- [both] `tests/test-app-end-to-end-tests/reset-store-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/sample-test.cs`
- [both] `tests/test-app-end-to-end-tests/static-weather-forecasts-page-tests.cs`
- [both] `tests/test-app-end-to-end-tests/throw-exception-page-tests.cs`

## `tests/timewarp-state-tests` (14)

- [both] `tests/timewarp-state-tests/convention-tests.cs`
- [both] `tests/timewarp-state-tests/global-usings.cs`
- [Design] `tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs`
- [Design] `tests/timewarp-state-tests/javascript-interop/json-request-handler-tests.cs`
- [Design] `tests/timewarp-state-tests/pipeline/render-subscriptions-post-processor-tests.cs`
- [Design] `tests/timewarp-state-tests/pipeline/state-transaction-behavior-tests.cs`
- [Design] `tests/timewarp-state-tests/state/throw-if-not-test-assembly-tests.cs`
- [Design] `tests/timewarp-state-tests/store/store-get-or-add-tests.cs`
- [both] `tests/timewarp-state-tests/subscriptions/subscriptions-tests.cs`
- [both] `tests/timewarp-state-tests/testing-convention.cs`
- [both] `tests/timewarp-state-tests/timewarp-state-component/capture-render-caller-tests.cs`
- [Design] `tests/timewarp-state-tests/timewarp-state-component/parameter-change-tests.cs`
- [both] `tests/timewarp-state-tests/timewarp-state-component/register-render-trigger-tests.cs`
- [both] `tests/timewarp-state-tests/type-extensions-tests.cs`

## `tests/timewarp-state-plus-tests` (12)

- [both] `tests/timewarp-state-plus-tests/architecture-tests.cs`
- [both] `tests/timewarp-state-plus-tests/convention-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/action-tracking/active-action-behavior-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/persistence/persistence-round-trip-tests.cs`
- [both] `tests/timewarp-state-plus-tests/features/routing/go-back-repro-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/routing/push-route-info-tests.cs`
- [both] `tests/timewarp-state-plus-tests/features/routing/routes-state-clone-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/routing/tw-breadcrumb-style-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/timers/add-timer-tests.cs`
- [Design] `tests/timewarp-state-plus-tests/features/timers/multi-timer-post-processor-tests.cs`
- [both] `tests/timewarp-state-plus-tests/global-usings.cs`
- [both] `tests/timewarp-state-plus-tests/testing-convention.cs`

## `tests/timewarp-state-analyzer-tests` (11)

- [Design] `tests/timewarp-state-analyzer-tests/analyzer-test-factory.cs`
- [Design] `tests/timewarp-state-analyzer-tests/catalog-action-analyzer-tests.cs`
- [both] `tests/timewarp-state-analyzer-tests/fixie-verifier.cs`
- [both] `tests/timewarp-state-analyzer-tests/global-usings.cs`
- [both] `tests/timewarp-state-analyzer-tests/handler-must-not-send-action-analyzer-tests.cs`
- [both] `tests/timewarp-state-analyzer-tests/state-implementation-analyzer-tests.cs`
- [both] `tests/timewarp-state-analyzer-tests/state-inheritance-analyzer-tests.cs`
- [both] `tests/timewarp-state-analyzer-tests/state-read-only-public-properties-analyzer-tests-new.cs`
- [both] `tests/timewarp-state-analyzer-tests/state-read-only-public-properties-analyzer-tests.cs`
- [both] `tests/timewarp-state-analyzer-tests/testing-convention.cs`
- [both] `tests/timewarp-state-analyzer-tests/timewarp-state-action-analyser-tests.cs`

## `tests/timewarp-state-telemetry-tests` (9)

- [both] `tests/timewarp-state-telemetry-tests/activity-listener-harness.cs`
- [both] `tests/timewarp-state-telemetry-tests/global-usings.cs`
- [both] `tests/timewarp-state-telemetry-tests/recorded-activity.cs`
- [both] `tests/timewarp-state-telemetry-tests/recording-store.cs`
- [Design] `tests/timewarp-state-telemetry-tests/service-collection-extensions-tests.cs`
- [Design] `tests/timewarp-state-telemetry-tests/telemetry-behavior-tests.cs`
- [both] `tests/timewarp-state-telemetry-tests/telemetry-test-json-context.cs`
- [both] `tests/timewarp-state-telemetry-tests/telemetry-test-state.cs`
- [both] `tests/timewarp-state-telemetry-tests/testing-convention.cs`

## `tests/timewarp-state-source-generator-tests` (6)

- [Design] `tests/timewarp-state-source-generator-tests/action-catalog-generator-test-driver.cs`
- [Design] `tests/timewarp-state-source-generator-tests/action-catalog-source-generator-tests.cs`
- [both] `tests/timewarp-state-source-generator-tests/global-usings.cs`
- [Design] `tests/timewarp-state-source-generator-tests/persistence-generator-test-driver.cs`
- [Design] `tests/timewarp-state-source-generator-tests/persistence-state-source-generator-tests.cs`
- [both] `tests/timewarp-state-source-generator-tests/testing-convention.cs`

## `tests/test-app-architecture-tests` (4)

- [both] `tests/test-app-architecture-tests/architecture-tests.cs`
- [both] `tests/test-app-architecture-tests/convention-tests.cs`
- [both] `tests/test-app-architecture-tests/global-usings.cs`
- [both] `tests/test-app-architecture-tests/testing-convention.cs`

## `samples` (42)

- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client/features/counter/counter-state.cs`
- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client/features/counter/counter-state.increment-count.cs`
- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client/global-usings.cs`
- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client/program.cs`
- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto/global-usings.cs`
- [both] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto/program.cs`
- [both] `samples/00-state-action-handler/server/sample-00-server/features/counter/counter-state.cs`
- [both] `samples/00-state-action-handler/server/sample-00-server/features/counter/counter-state.increment-count.cs`
- [both] `samples/00-state-action-handler/server/sample-00-server/global-usings.cs`
- [both] `samples/00-state-action-handler/server/sample-00-server/program.cs`
- [both] `samples/00-state-action-handler/wasm/sample-00-wasm/features/counter/counter-state.cs`
- [both] `samples/00-state-action-handler/wasm/sample-00-wasm/features/counter/counter-state.increment-count.cs`
- [both] `samples/00-state-action-handler/wasm/sample-00-wasm/global-usings.cs`
- [both] `samples/00-state-action-handler/wasm/sample-00-wasm/program.cs`
- [both] `samples/01-redux-dev-tools/wasm/sample-01-wasm/features/counter/counter-state.cs`
- [both] `samples/01-redux-dev-tools/wasm/sample-01-wasm/features/counter/counter-state.increment-count.cs`
- [both] `samples/01-redux-dev-tools/wasm/sample-01-wasm/global-usings.cs`
- [both] `samples/01-redux-dev-tools/wasm/sample-01-wasm/program.cs`
- [both] `samples/02-action-tracking/wasm/sample-02-wasm/features/demo/demo-state.cs`
- [both] `samples/02-action-tracking/wasm/sample-02-wasm/features/demo/demo-state.five-second-action.cs`
- [both] `samples/02-action-tracking/wasm/sample-02-wasm/features/demo/demo-state.two-second-action.cs`
- [both] `samples/02-action-tracking/wasm/sample-02-wasm/global-usings.cs`
- [both] `samples/02-action-tracking/wasm/sample-02-wasm/program.cs`
- [both] `samples/03-routing/wasm/sample-03-wasm/features/counter/counter-state.cs`
- [both] `samples/03-routing/wasm/sample-03-wasm/features/counter/counter-state.increment-count.cs`
- [both] `samples/03-routing/wasm/sample-03-wasm/global-usings.cs`
- [both] `samples/03-routing/wasm/sample-03-wasm/program.cs`
- [both] `samples/04-telemetry/apphost/global-usings.cs`
- [Design] `samples/04-telemetry/apphost/program.cs`
- [both] `samples/04-telemetry/server/sample-04-server/features/counter/counter-state.cs`
- [both] `samples/04-telemetry/server/sample-04-server/features/counter/counter-state.increment-count.cs`
- [both] `samples/04-telemetry/server/sample-04-server/global-usings.cs`
- [both] `samples/04-telemetry/server/sample-04-server/program.cs`
- [Design] `samples/05-persistence/wasm/sample-05-wasm/features/display-preferences/display-preferences-state.set-accent.cs`
- [Design] `samples/05-persistence/wasm/sample-05-wasm/features/display-preferences/display-preferences-state.toggle-compact.cs`
- [Design] `samples/05-persistence/wasm/sample-05-wasm/features/draft-note/draft-note-state.clear.cs`
- [Design] `samples/05-persistence/wasm/sample-05-wasm/features/draft-note/draft-note-state.update-text.cs`
- [Design] `samples/05-persistence/wasm/sample-05-wasm/global-usings.cs`
- [Design] `samples/06-render-control/wasm/sample-06-wasm/components/filter-model.cs`
- [Design] `samples/06-render-control/wasm/sample-06-wasm/features/activity/activity-state.increment-count.cs`
- [Design] `samples/06-render-control/wasm/sample-06-wasm/features/activity/activity-state.tick.cs`
- [Design] `samples/06-render-control/wasm/sample-06-wasm/global-usings.cs`

## `scripts` (7)

- [both] `scripts/build.cs`
- [both] `scripts/clean.cs`
- [both] `scripts/e2e.cs`
- [both] `scripts/global-usings.cs`
- [both] `scripts/package.cs`
- [both] `scripts/run-test-app.cs`
- [both] `scripts/test.cs`

## `.githooks` (5)

- [both] `.githooks/post-checkout.cs`
- [both] `.githooks/post-commit.cs`
- [both] `.githooks/post-merge.cs`
- [both] `.githooks/pre-commit.cs`
- [both] `.githooks/pre-push.cs`

## `tools` (1)

- [Design] `tools/dev-cli/global-usings.cs`
