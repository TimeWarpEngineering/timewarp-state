namespace ThrowServerSideExceptionEndpoint
{
  using FluentAssertions;
  using Microsoft.AspNetCore.Mvc.Testing;
  using System.Net;
  using System.Net.Http;
  using System.Text.Json;
  using System.Threading.Tasks;
  using TestApp.Client.Features.ExceptionHandlings;
  using TestApp.Client.Server.Integration.Tests.Infrastructure;
  using TestApp.Client.Server;

  public class Returns : BaseTest
  {
    private readonly ThrowServerSideExceptionRequest ThrowServerSideExceptionRequest;

    public Returns
    (
      WebApplicationFactory<Startup> aWebApplicationFactory,
      JsonSerializerOptions aJsonSerializerOptions
    ) : base(aWebApplicationFactory, aJsonSerializerOptions)
    {
      ThrowServerSideExceptionRequest = new ThrowServerSideExceptionRequest { SampleProperty = "sample" };
    }

    public async Task ThrowServerSideExceptionResponse()
    {
      ThrowServerSideExceptionResponse ThrowServerSideExceptionResponse =
        await GetJsonAsync<ThrowServerSideExceptionResponse>(ThrowServerSideExceptionRequest.GetRoute());

      ValidateThrowServerSideExceptionResponse(ThrowServerSideExceptionResponse);
    }

    public async Task ValidationError()
    {
      // Set invalid value
      ThrowServerSideExceptionRequest.SampleProperty = string.Empty;

      HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(ThrowServerSideExceptionRequest.GetRoute());

      string json = await httpResponseMessage.Content.ReadAsStringAsync();

      httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      json.Should().Contain("errors");
      json.Should().Contain(nameof(ThrowServerSideExceptionRequest.SampleProperty));
    }

    private void ValidateThrowServerSideExceptionResponse(ThrowServerSideExceptionResponse aThrowServerSideExceptionResponse)
    {
      aThrowServerSideExceptionResponse.CorrelationId.Should().Be(ThrowServerSideExceptionRequest.CorrelationId);
      // check Other properties here
    }
  }
}