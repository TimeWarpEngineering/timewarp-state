# Migrate from FluentAssertions to Shouldly

**Completed: 2025-08-20**

## Description
FluentAssertions has gone commercial without changing their package name, creating licensing concerns. Migrate all test assertions from FluentAssertions to Shouldly, which remains open source and provides similar fluent assertion capabilities.

## Acceptance Criteria

### Phase 1: Add Shouldly Package ✅
- [x] Add Shouldly package to Directory.Packages.props (v4.3.0)
- [x] Remove FluentAssertions package reference

### Phase 2: Migrate Test Projects ✅
- [x] Migrate TimeWarp.State.Tests assertions
- [x] Migrate TimeWarp.State.Analyzer.Tests assertions
- [x] Migrate TimeWarp.State.Plus.Tests assertions
- [x] Migrate Client.Integration.Tests assertions
- [x] Migrate Test.App.Architecture.Tests assertions
- [x] Migrate Test.App.EndToEnd.Tests assertions
- [x] Migrate TimeWarp.State.Policies assertions

### Phase 3: Update Assertion Patterns ✅
Common migrations:
- [x] `result.Should().Be(expected)` → `result.ShouldBe(expected)`
- [x] `result.Should().NotBe(expected)` → `result.ShouldNotBe(expected)`
- [x] `result.Should().BeNull()` → `result.ShouldBeNull()`
- [x] `result.Should().NotBeNull()` → `result.ShouldNotBeNull()`
- [x] `collection.Should().Contain(item)` → `collection.ShouldContain(item)`
- [x] `collection.Should().HaveCount(n)` → `collection.Count().ShouldBe(n)`
- [x] `action.Should().Throw<T>()` → `Should.Throw<T>(action)`
- [x] `result.Should().BeTrue()` → `result.ShouldBeTrue()`
- [x] `result.Should().BeFalse()` → `result.ShouldBeFalse()`
- [x] `result.Should().BeEquivalentTo(expected)` → `result.ShouldBe(expected)` or custom comparison

### Phase 4: Validation ✅
- [x] Run all tests to ensure they pass (46 passed, 4 skipped)
- [x] Verify no FluentAssertions using statements remain
- [x] Verify no FluentAssertions package references remain

## Notes
- Shouldly is MIT licensed and actively maintained
- Shouldly provides similar fluent syntax with better error messages
- Some complex FluentAssertions features may need custom replacements
- Priority: Complete before other major refactoring to avoid licensing issues