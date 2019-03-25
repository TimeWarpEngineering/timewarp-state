### Breaking changes

In general, breaking changes can be made only in a new **major** product version, e.g. moving from `1.x.x` to `2.0.0`. Even still, we generally try to avoid breaking changes because they can incur large costs for anyone using these products.

Breaking changes in major versions need to be approved by an engineering manager.

If there is a case where a breaking change needs to be made *not* in a major product update, the change must be approved by at least @Eilon and @DamianEdwards.

For the normal case of breaking changes in major versions, this is the ideal process:

1. Provide some new alternative API (if necessary)
2. Mark the old type/member as `[Obsolete]` to alert users (see below), and to point them at the new alternative API (if applicable)
 * If the old API really doesn't/can't work at all, please discuss with engineering team
3. Update the XML doc comments to indicate the type/member is obsolete, plus what the alternative is. This is typically exactly the same as the obsolete attribute message.
4. File a bug in the next major milestone (e.g. 2.0.0) to remove the type/member
 * Mark this bug with a red `[breaking-change]` label (use exact casing, hyphenation, etc.). Create the label in the repo if it's not there.

Example of obsoleted API:

```c#
    /// <summary>
    /// <para>
    ///     This method/property/type is obsolete and will be removed in a future version.
    ///     The recommended alternative is Microsoft.SomethingCore.SomeType.SomeNewMethod.
    /// </para>
    /// <para>
    ///     ... old docs...
    /// </para>
    /// </summary>
    [Obsolete("This method/property/type is obsolete and will be removed in a future version. The recommended alternative is Microsoft.SomethingCore.SomeType.SomeNewMethod.")]
    public void SomeOldMethod(...)
    {
        ...
    }
```
