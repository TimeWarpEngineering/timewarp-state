namespace Console_CSharp.Tests.Commands
{
  using System;
  using System.IO;
  using System.Threading.Tasks;
  using Console_CSharp;
  using Shouldly;

  internal class SampleCommandTests
  {
    public SampleCommandTests()
    {

    }


    public async Task ShouldReturnFailure()
    {
      string[] argumentArray = new string[]
        {
                  "SampleCommand",
                  "--Parameter1",
                  "Ha",
                  "--Parameter2",
                  "-2",
                  "--Parameter3",
                  "11"
        };
      TextWriter textWriter = Console.Out;
      int exitCode = 0;
      string output = null;

      using (var stringWriter = new StringWriter())
      {
        Console.SetError(stringWriter);
        exitCode = await Program.Main(argumentArray);
        output = stringWriter.ToString();
      }
      Console.SetError(textWriter);

      exitCode.ShouldBe(1);
      output.ShouldContain("Parameter3 must be greater than Parameter2");

    }

    public async Task NoArgumentsFailsAsync()
    {
      int exitCode = await Program.Main(null);
      exitCode.ShouldBe(1);
    }

    public async Task ShouldReturnSuccess()
    {
      string[] argumentArray = new string[]
        {
                  "SampleCommand",
                  "--Parameter1",
                  "ABC",
                  "--Parameter2",
                  "2",
                  "--Parameter3",
                  "9"
        };
      TextWriter textWriter = Console.Out;
      int exitCode = 0;
      string error = null;

      using (var stringWriter = new StringWriter())
      {
        Console.SetError(stringWriter);
        exitCode = await Program.Main(argumentArray);
        error = stringWriter.ToString();
      }
      Console.SetError(textWriter);

      exitCode.ShouldBe(0);
      error.ShouldBeNullOrEmpty();

    }

  }
}
