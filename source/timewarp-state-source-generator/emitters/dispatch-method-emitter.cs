#region Purpose
// Emits the dispatch method a state gets for each nested *ActionSet, for example CounterState.IncrementCount(amount),
// which builds the Action and sends it.
#endregion

#region Design
// A pure function of one ActionSetMethodModel (ActionFactExtractor builds it). The text is the pre-142 ActionSet
// method generator's, byte for byte, which the golden tests pin. The method takes an optional CancellationToken
// linked with the state's own token. Required members are set with an object initializer. The containing-type chain
// and namespace come from the model, so nested, generic and global-namespace states get a partial that merges.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal static class DispatchMethodEmitter
{
  public static void Emit(ActionSetMethodModel model, SourceProductionContext context) =>
    context.AddSource(model.HintName, GenerateMethodCode(model));

  internal static string GenerateMethodCode(ActionSetMethodModel model)
  {
    string parameterList = string.Join
    (
      separator: ", ",
      model.Parameters.Select(parameter =>
        $"{parameter.Type} {parameter.Name}{(parameter.DefaultExpression is null ? "" : $" = {parameter.DefaultExpression}")}")
    );

    string argumentList = string.Join
    (
      separator: ", ",
      model.Parameters.Where(parameter => parameter.InitializerMember is null).Select(parameter => parameter.Name)
    );

    string initializer = string.Join
    (
      separator: ", ",
      model.Parameters.Where(parameter => parameter.InitializerMember is not null)
        .Select(parameter => $"{parameter.InitializerMember} = {parameter.Name}")
    );

    string construction = initializer.Length == 0
      ? $"new {model.ActionSetName}.Action({argumentList})"
      : $"new {model.ActionSetName}.Action({argumentList}) {{ {initializer} }}";

    string separator = model.Parameters.Count > 0 ? ", " : "";

    var builder = new StringBuilder();
    builder.AppendLine("#nullable enable");
    builder.AppendLine();
    builder.AppendLine("#pragma warning disable CS1591");
    if (model.NamespaceName is not null)
    {
      builder.AppendLine($"namespace {model.NamespaceName};");
      builder.AppendLine();
    }

    builder.AppendLine("using System.Threading;");
    builder.AppendLine("using System.Threading.Tasks;");
    builder.AppendLine();

    string indent = string.Empty;
    foreach (string header in model.TypeHeaders)
    {
      builder.AppendLine($"{indent}{header}");
      builder.AppendLine($"{indent}{{");
      indent += "    ";
    }

    builder.AppendLine($"{indent}{model.Accessibility} async Task {model.MethodName}({parameterList}{separator}CancellationToken? externalCancellationToken = null)");
    builder.AppendLine($"{indent}{{");
    builder.AppendLine($"{indent}    using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue");
    builder.AppendLine($"{indent}        ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)");
    builder.AppendLine($"{indent}        : null;");
    builder.AppendLine();
    builder.AppendLine($"{indent}    await Sender.Send");
    builder.AppendLine($"{indent}    (");
    builder.AppendLine($"{indent}        {construction},");
    builder.AppendLine($"{indent}        linkedCts?.Token ?? CancellationToken");
    builder.AppendLine($"{indent}    );");
    builder.AppendLine($"{indent}}}");

    for (int depth = model.TypeHeaders.Count - 1; depth >= 0; depth--)
    {
      indent = indent.Substring(0, depth * 4);
      builder.AppendLine($"{indent}}}");
    }

    builder.AppendLine("#pragma warning restore CS1591");
    return builder.ToString();
  }
}

internal sealed record ActionSetMethodModel
(
  string? NamespaceName,
  EquatableArray<string> TypeHeaders,
  string ActionSetName,
  string MethodName,
  string Accessibility,
  EquatableArray<ActionParameterModel> Parameters,
  string HintName
);
