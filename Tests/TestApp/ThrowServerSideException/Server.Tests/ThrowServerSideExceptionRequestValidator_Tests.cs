namespace ThrowServerSideExceptionRequestValidator
{
  using FluentAssertions;
  using FluentValidation.Results;
  using FluentValidation.TestHelper;
  using TestApp.Client.Features.ExceptionHandlings;

  public class Validate_Should
  {
    private ThrowServerSideExceptionRequestValidator ThrowServerSideExceptionRequestValidator;

    public Validate_Should()
    {
      ThrowServerSideExceptionRequestValidator = new ThrowServerSideExceptionRequestValidator();
    }

    public void Be_Valid()
    {
      var throwServerSideExceptionRequest = new ThrowServerSideExceptionRequest
      {
        // Set Valid values here
        // #TODO
        SampleProperty = "sample"
      };

      ValidationResult validationResult = ThrowServerSideExceptionRequestValidator.TestValidate(throwServerSideExceptionRequest);

      validationResult.IsValid.Should().BeTrue();
    }

    // #TODO Rename thie test and add tests for all validation rules
    public void Have_error_when_SampleProperty_is_empty() => ThrowServerSideExceptionRequestValidator
      .ShouldHaveValidationErrorFor(aThrowServerSideExceptionRequest => aThrowServerSideExceptionRequest.SampleProperty, string.Empty);

  }
}
