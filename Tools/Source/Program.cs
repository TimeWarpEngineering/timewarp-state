namespace Tools
{
  using System.CommandLine;
  using System.CommandLine.Builder;
  using System.CommandLine.Invocation;
  using System.Threading.Tasks;

  partial class Program
  {
    private static async Task Main(string[] args)
    {
      Parser parser = new TimeWarpCommandLineBuilder().Build();

      await parser.InvokeAsync(args);
    }
  }
}