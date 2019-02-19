namespace Console_CSharp.Commands.SampleCommand
{
  using FluentValidation;

  internal class SampleCommandValidator : AbstractValidator<SampleCommandRequest>
  {
    public SampleCommandValidator()
    {
      RuleFor(aSampleCommandRequest => aSampleCommandRequest)
        .Must(ValidateParameter3GreaterthanParameter2)
        .WithMessage("Parameter3 must be greater than Parameter2");
      RuleFor(aSetVersionRequest => aSetVersionRequest.Parameter1).MinimumLength(3);
      RuleFor(aSetVersionRequest => aSetVersionRequest.Parameter2).GreaterThanOrEqualTo(0);
      RuleFor(aSetVersionRequest => aSetVersionRequest.Parameter3).LessThanOrEqualTo(10);
    }

    private bool ValidateParameter3GreaterthanParameter2(SampleCommandRequest aSampleCommandRequest) =>
      (aSampleCommandRequest.Parameter3 > aSampleCommandRequest.Parameter2);
  }
}