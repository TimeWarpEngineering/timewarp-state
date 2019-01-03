namespace Tools.Behaviors
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using FluentValidation;
  using FluentValidation.Results;
  using MediatR.Pipeline;

  internal class ValidationBehavior<TRequest> : IRequestPreProcessor<TRequest>
  {
    private IEnumerable<IValidator<TRequest>> Validators { get; }

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> aValidators)
    {
      Validators = aValidators;
    }

    public Task Process(TRequest aRequest, CancellationToken aCancellationToken)
    {
      var validationContext = new ValidationContext(aRequest);
      List<ValidationFailure> validationFailures = Validators
        .Select(aValidationResult => aValidationResult.Validate(validationContext))
        .SelectMany(aValidationResult => aValidationResult.Errors)
        .Where(aValidationFailure => aValidationFailure != null)
        .ToList();

      if (validationFailures.Count != 0)
      {
        validationFailures.ForEach(aValidationFailure => Console.Error.WriteLine(aValidationFailure.ErrorMessage));

        throw new ValidationException(validationFailures);
      }

      return Task.CompletedTask;
    }
  }
}