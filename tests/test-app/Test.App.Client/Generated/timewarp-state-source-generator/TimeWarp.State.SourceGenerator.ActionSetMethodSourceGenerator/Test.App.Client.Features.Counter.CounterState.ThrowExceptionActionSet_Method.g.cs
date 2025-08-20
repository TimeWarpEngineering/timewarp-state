#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.Counter;

using System.Threading;
using System.Threading.Tasks;

partial class CounterState
{
    public async Task ThrowException(string message, CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new ThrowExceptionActionSet.Action(message),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591