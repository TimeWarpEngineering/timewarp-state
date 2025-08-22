#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Application;

using System.Threading;
using System.Threading.Tasks;

partial class ApplicationState
{
    public async Task ResetStore(CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new ResetStoreActionSet.Action(),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591