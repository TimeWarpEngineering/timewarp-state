namespace Tools.Commands.GenerateToc
{
  using MediatR;

  /// <summary>
  /// Generate toc.yml files for all folders found under the documentation folder
  /// if a toc.yml already exist and does not contain `auto-generated` in the first line this folder will be skipped.
  /// Generated toc.yml files will have a comment on the first line as in: `# auto-generated`
  /// </summary>
  public class GenerateTocRequest : IRequest { }
}
