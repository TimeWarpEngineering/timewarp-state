#region Purpose
// Registers ISender/IPublisher for ServerPipeline without the ambiguous DI extension.
#endregion

#region Design
// Test.App.Client and Test.App.Server both emit
// Microsoft.Extensions.DependencyInjection.GeneratedMediatorServiceCollectionExtensions
// (TimeWarpMediatorNamespace only renames Mediator/Sender/Publisher types, not the DI
// extensions). Calling AddGeneratedMediator<ServerPipeline>() here is CS0121. Register via
// the unique Test.App.Server.Generated Sender/Publisher types instead; keep Generators so
// those types and TWM004 still emit. Mirror AddGeneratedMediator_ServerPipeline.
// Aliasing the client ProjectReference would disambiguate the extension but breaks Razor
// @using Test.App.Client on the server host. Track the generator namespace gap for 080-003.
#endregion

namespace Test.App.Server;

using Contracts.Features.WeatherForecast;
using Features.WeatherForecast;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Mediator;
using TimeWarp.State;

internal static class ServerPipelineRegistration
{
  public static IServiceCollection AddServerPipelineMediator(this IServiceCollection services)
  {
    services.AddTransient<Generated.Sender_ServerPipeline>();
    services.AddTransient<ISender<ServerPipeline>>(static sp => sp.GetRequiredService<Generated.Sender_ServerPipeline>());
    services.AddTransient<Generated.Publisher_ServerPipeline>();
    services.AddTransient<IPublisher<ServerPipeline>>(static sp => sp.GetRequiredService<Generated.Publisher_ServerPipeline>());
    services.AddScoped<GetWeatherForecastsHandler>();
    services.AddScoped<IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecasts.Response>>(
      static sp => sp.GetRequiredService<GetWeatherForecastsHandler>());
    return services;
  }
}
