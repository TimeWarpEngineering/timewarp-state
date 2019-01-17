namespace Console_CSharp
{
  using System.CommandLine.Builder;
  using System.CommandLine.Invocation;
  using FluentValidation;
  using MediatR.Pipeline;
  using Microsoft.Extensions.DependencyInjection;
  using Console_CSharp.Behaviors;
  using Console_CSharp.Services;
  using Console_CSharp.Commands.SampleCommand;

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
      aServiceCollection.AddScoped(typeof(IValidator<SampleCommandRequest>), typeof(SampleCommandValidator));
      aServiceCollection.AddLogging();
      aServiceCollection.AddSingleton<GitService>();
    }
  }
}