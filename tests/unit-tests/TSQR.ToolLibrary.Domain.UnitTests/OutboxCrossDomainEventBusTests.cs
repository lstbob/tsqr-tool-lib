using System.Text.Json;
using TSQR.Common.Messaging;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class OutboxCrossDomainEventBusTests
{
    [Fact]
    public async Task PublishAsync_SerializesAndEnqueuesTheEvent()
    {
        var store = new RecordingOutboxStore();
        var bus = new OutboxCrossDomainEventBus(store);
        var integrationEvent = new TestIntegrationEvent("payload");

        await bus.PublishAsync(integrationEvent, CancellationToken.None);

        var message = Assert.Single(store.Messages);
        Assert.Equal(integrationEvent.EventId, message.EventId);
        Assert.Equal(typeof(TestIntegrationEvent).FullName, message.EventType);
        Assert.Equal(integrationEvent.OccurredOn, message.OccurredOn);

        var roundTrip = JsonSerializer.Deserialize<TestIntegrationEvent>(
            message.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(roundTrip);
        Assert.Equal("payload", roundTrip.Value);
        Assert.Equal(integrationEvent.EventId, roundTrip.EventId);
        Assert.Equal(integrationEvent.OccurredOn, roundTrip.OccurredOn);
    }

    private sealed record TestIntegrationEvent(string Value) : IntegrationEvent;

    private sealed class RecordingOutboxStore : IOutboxStore
    {
        public List<OutboxMessage> Messages { get; } = [];

        public Task EnqueueAsync(
            OutboxMessage message,
            CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<OutboxMessage>> DequeueAsync(
            int batchSize,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<OutboxMessage>>([]);

        public Task MarkProcessedAsync(
            Guid eventId,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task MarkFailedAsync(
            Guid eventId,
            string error,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
