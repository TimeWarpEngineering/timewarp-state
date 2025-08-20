#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Application;

using System.Threading;
using System.Threading.Tasks;

partial class ApplicationState
{
    public async Task TwoSecondTask(CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new TwoSecondTaskActionSet.Action(),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591