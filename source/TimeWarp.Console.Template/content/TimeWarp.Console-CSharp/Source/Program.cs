namespace Console_CSharp
{
  using System.CommandLine;
  using System.CommandLine.Builder;
  using System.CommandLine.Invocation;
  using System.Threading.Tasks;

  internal class Program
  {
    internal static async Task<int> Main(string[] aArgumentArray)
    {
      Parser parser = new TimeWarpCommandLineBuilder().Build();

      return await parser.InvokeAsync(aArgumentArray);
    }
  }
}