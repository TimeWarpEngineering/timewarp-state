#region Purpose
// Proves the test app server maps ThrowServerSideExceptionRequest's route and answers it with a 500.
#endregion

#region Design
// Resolves the HttpClient the TestingConvention points at the in-process Test.App.Server and GETs the contract's
// GetRoute. Before the endpoint existed this returned 404. In the Development test host the developer exception
// page renders the exception, so the body is checked for the message.
#endregion

namespace ThrowServerSideExceptionEndpointTests;

public class Endpoint_Should
{
  private readonly HttpClient HttpClient;

  public Endpoint_Should(ClientHost clientHost)
  {
    HttpClient = clientHost.ServiceProvider.GetRequiredService<HttpClient>();
  }

  public async Task Return_InternalServerError_With_Message()
  {
    const string message = "endpoint test message";
    ThrowServerSideExceptionRequest request = new() { SampleProperty = message };

    using HttpResponseMessage response = await HttpClient.GetAsync(request.GetRoute());

    response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    string body = await response.Content.ReadAsStringAsync();
    body.ShouldContain(message);
  }
}
