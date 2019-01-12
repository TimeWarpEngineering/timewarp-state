namespace Tools
{
  using System.CommandLine.Builder;
  using System.CommandLine.Invocation;
  using FluentValidation;
  using MediatR.Pipeline;
  using Microsoft.Extensions.DependencyInjection;
  using Tools.Behaviors;
  using Tools.Commands.SetVersion;
  using Tools.Services;

  internal class Startup
  {
    public void Configure(TimeWarpCommandLineBuilder aTimeWarpCommandLineBuilder)
    {
      aTimeWarpCommandLineBuilder.AddVersionOption()
        // middleware
        .UseHelp()
        .UseParseDirective()
        .UseDebugDirective()
        .UseSuggestDirective()
        .RegisterWithDotnetSuggest()
        .UseParseErrorReporting()
        .UseExceptionHandler();
    }

    public void ConfigureServices(IServiceCollection aServiceCollection)
    {
      aServiceCollection.AddScoped(typeof(IRequestPreProcessor<>), typeof(ValidationBehavior<>));
      aServiceCollection.AddScoped(typeof(IValidator<SetVersionRequest>), typeof(SetVersionValidator));
      aServiceCollection.AddLogging();
      aServiceCollection.AddSingleton<GitService>();
      aServiceCollection.AddSingleton<YmlTools>();
    }
  }
}