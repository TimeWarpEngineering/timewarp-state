namespace Microsoft.JSInterop;

public static class CamelCase
{
  public static string MemberNameToCamelCase(string memberName)
  {
    if (string.IsNullOrEmpty(memberName))
    {
      throw new ArgumentException(
          $"The value '{memberName ?? "null"}' is not a valid member name.",
          nameof(memberName));
    }

    // If we don't need to modify the value, bail out without creating a char array
    if (!char.IsUpper(memberName[0]))
    {
      return memberName;
    }

    // We have to modify at least one character
    char[] chars = memberName.ToCharArray();

    int length = chars.Length;
    if (length < 2 || !char.IsUpper(chars[1]))
    {
      // Only the first character needs to be modified
      // Note that this branch is functionally necessary, because the 'else' branch below
      // never looks at char[1]. It's always looking at the n+2 character.
      chars[0] = char.ToLowerInvariant(chars[0]);
    }
    else
    {
      // If chars[0] and chars[1] are both upper, then we'll lowercase the first char plus
      // any consecutive uppercase ones, stopping if we find any char that is followed by a
      // non-uppercase one
      int i = 0;
      while (i < length)
      {
        chars[i] = char.ToLowerInvariant(chars[i]);

        i++;

        // If the next-plus-one char isn't also uppercase, then we're now on the last uppercase, so stop
        if (i < length - 1 && !char.IsUpper(chars[i + 1]))
        {
          break;
        }
      }
    }

    return new string(chars);
  }
}
