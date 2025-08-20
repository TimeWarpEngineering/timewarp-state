#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.EventStream;

using System.Threading;
using System.Threading.Tasks;

partial class EventStreamState
{
    public async Task AddEvent(string message, CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new AddEventActionSet.Action(message),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591