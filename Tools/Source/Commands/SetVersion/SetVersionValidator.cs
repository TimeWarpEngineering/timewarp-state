namespace Tools.Commands.SetVersion
{
  using System.Linq;
  using FluentValidation;

  internal class SetVersionValidator : AbstractValidator<SetVersionRequest>
  {
    public SetVersionValidator()
    {
      RuleFor(a => SetVersionRequest.ProjectList.Contains(a.Project));
      RuleFor(aSetVersionRequest => aSetVersionRequest)
        .Must(ValidateIsNotAllZeros)
        .WithMessage("Not all versions can be zero. `0.0.0` is not a valid version");
      RuleFor(aSetVersionRequest => aSetVersionRequest.Major).GreaterThanOrEqualTo(0);
      RuleFor(aSetVersionRequest => aSetVersionRequest.Minor).GreaterThanOrEqualTo(0);
      RuleFor(aSetVersionRequest => aSetVersionRequest.Patch).GreaterThanOrEqualTo(0);
    }

    private bool ValidateIsNotAllZeros(SetVersionRequest aSetVersionRequest)
    {
      return
        !(aSetVersionRequest.Major == 0 &&
        aSetVersionRequest.Minor == 0 &&
        aSetVersionRequest.Patch == 0);
    }
  }
}