#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.CloneTest;

using System.Threading;
using System.Threading.Tasks;

partial class CloneableState
{
    public async Task CloneTest(CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new CloneTestActionSet.Action(),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591