
namespace Tools
{
  using System;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Tools.Commands.SetVersion;

  class Program
  {

    /// <summary>
    /// Updates all the required files to this version.
    /// Using semantic versioning Major.Minor.Patch
    /// </summary>
    /// <param name="Major">The Major version number to be set</param>
    /// <param name="Minor">The Minor version number to be set</param>
    /// <param name="Patch">The Patch version number to be set</param>
    static async System.Threading.Tasks.Task Main(int Major, int Minor, int Patch)
    {
      ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddMediatR()
        .BuildServiceProvider();

      Console.WriteLine($"aMajor: {Major}");
      Console.WriteLine($"aMinor: {Minor}");
      Console.WriteLine($"aPatch: {Patch}");

      IMediator mediator = serviceProvider.GetService<IMediator>();

      await mediator.Send(new SetVersionRequest
      {
        Major = Major,
        Minor = Minor,
        Patch = Patch
      });
    }
  }
}
