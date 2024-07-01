namespace HomePage_;

using Test.App.EndToEnd.Tests;

[TestClass]
public class Should_ : PageTest
{
  [TestMethod]
  public async Task HaveTitle()
  {
    await Page.GotoAsync(Configuration.SutBaseHttpsUrl);
    
    await Expect(Page).ToHaveTitleAsync("TimeWarp.State Test App");
  }
}
