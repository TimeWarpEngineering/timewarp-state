# Migrate from FluentAssertions to Shouldly

## Description
FluentAssertions has gone commercial without changing their package name, creating licensing concerns. Migrate all test assertions from FluentAssertions to Shouldly, which remains open source and provides similar fluent assertion capabilities.

## Acceptance Criteria

### Phase 1: Add Shouldly Package
- [ ] Add Shouldly package to Directory.Packages.props
- [ ] Remove FluentAssertions package reference

### Phase 2: Migrate Test Projects
- [ ] Migrate TimeWarp.State.Tests assertions
- [ ] Migrate TimeWarp.State.Analyzer.Tests assertions
- [ ] Migrate TimeWarp.State.Plus.Tests assertions
- [ ] Migrate Client.Integration.Tests assertions
- [ ] Migrate Test.App.Architecture.Tests assertions
- [ ] Migrate Test.App.EndToEnd.Tests assertions
- [ ] Migrate TimeWarp.State.Policies assertions

### Phase 3: Update Assertion Patterns
Common migrations:
- [ ] `result.Should().Be(expected)` → `result.ShouldBe(expected)`
- [ ] `result.Should().NotBe(expected)` → `result.ShouldNotBe(expected)`
- [ ] `result.Should().BeNull()` → `result.ShouldBeNull()`
- [ ] `result.Should().NotBeNull()` → `result.ShouldNotBeNull()`
- [ ] `collection.Should().Contain(item)` → `collection.ShouldContain(item)`
- [ ] `collection.Should().HaveCount(n)` → `collection.Count().ShouldBe(n)`
- [ ] `action.Should().Throw<T>()` → `Should.Throw<T>(action)`
- [ ] `result.Should().BeTrue()` → `result.ShouldBeTrue()`
- [ ] `result.Should().BeFalse()` → `result.ShouldBeFalse()`
- [ ] `result.Should().BeEquivalentTo(expected)` → `result.ShouldBe(expected)` or custom comparison

### Phase 4: Validation
- [ ] Run all tests to ensure they pass
- [ ] Verify no FluentAssertions using statements remain
- [ ] Verify no FluentAssertions package references remain

## Notes
- Shouldly is MIT licensed and actively maintained
- Shouldly provides similar fluent syntax with better error messages
- Some complex FluentAssertions features may need custom replacements
- Priority: Complete before other major refactoring to avoid licensing issues