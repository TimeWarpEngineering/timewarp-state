namespace ThrowServerSideExceptionHandler
{
  using System.Threading.Tasks;
  using System.Text.Json;
  using Microsoft.AspNetCore.Mvc.Testing;
  using TestApp.Client.Server.Integration.Tests.Infrastructure;
  using TestApp.Client.Features.ExceptionHandlings;
  using TestApp.Client.Server;
  using FluentAssertions;

  public class Handle_Returns : BaseTest
  {
    private readonly ThrowServerSideExceptionRequest ThrowServerSideExceptionRequest;

    public Handle_Returns
    (
      WebApplicationFactory<Startup> aWebApplicationFactory,
      JsonSerializerOptions aJsonSerializerOptions
    ) : base(aWebApplicationFactory, aJsonSerializerOptions)
    {
      ThrowServerSideExceptionRequest = new ThrowServerSideExceptionRequest { SampleProperty = "sample" };
    }

    public async Task ThrowServerSideExceptionResponse()
    {
      ThrowServerSideExceptionResponse ThrowServerSideExceptionResponse = await Send(ThrowServerSideExceptionRequest);

      ValidateThrowServerSideExceptionResponse(ThrowServerSideExceptionResponse);
    }

    private void ValidateThrowServerSideExceptionResponse(ThrowServerSideExceptionResponse aThrowServerSideExceptionResponse)
    {
      aThrowServerSideExceptionResponse.CorrelationId.Should().Be(ThrowServerSideExceptionRequest.CorrelationId);
      ;// #TODO: check Other properties here
    }

  }
}