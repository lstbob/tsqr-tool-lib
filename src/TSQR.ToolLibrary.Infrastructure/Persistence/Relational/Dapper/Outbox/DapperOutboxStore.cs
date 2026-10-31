using TSQR.Common.Messaging;
using TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Abstractions;

namespace TSQR.ToolLibrary.Infrastructure.Persistence.Relational.Dapper.Outbox;

/// <summary>
/// Dapper outbox backed by the same <see cref="ISqlUnitOfWork"/> transaction as
/// the aggregate writes. Enqueue is therefore atomic with the command.
/// </summary>
public sealed class DapperOutboxStore(ISqlUnitOfWork uow) : IOutboxStore
{
    private ISqlConnection Database => uow.Connection;

    public Task EnqueueAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        return Database.ExecuteAsync(
            """
            INSERT INTO OutboxMessages (EventId, EventType, Payload, OccurredOn)
            VALUES (@EventId, @EventType, @Payload, @OccurredOn)
            ON CONFLICT (EventId) DO NOTHING
            """,
            new
            {
                message.EventId,
                message.EventType,
                message.Payload,
                message.OccurredOn,
            });
    }

    public async Task<IReadOnlyCollection<OutboxMessage>> DequeueAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var rows = await Database.QueryAsync<OutboxRow>(
            """
            SELECT EventId, EventType, Payload, OccurredOn, Attempts, LastError
            FROM OutboxMessages
            WHERE ProcessedOn IS NULL
              AND NextAttemptOn <= NOW()
            ORDER BY OccurredOn
            LIMIT @BatchSize
            """,
            new { BatchSize = batchSize });

        return rows
            .Select(row => new OutboxMessage(
                row.EventId,
                row.EventType,
                row.Payload,
                row.OccurredOn,
                row.Attempts,
                row.LastError))
            .ToList();
    }

    public Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken = default) =>
        Database.ExecuteAsync(
            """
            UPDATE OutboxMessages
            SET ProcessedOn = NOW(),
                LastError = NULL
            WHERE EventId = @EventId
            """,
            new { EventId = eventId });

    public Task MarkFailedAsync(
        Guid eventId,
        string error,
        CancellationToken cancellationToken = default) =>
        Database.ExecuteAsync(
            """
            UPDATE OutboxMessages
            SET Attempts = Attempts + 1,
                LastError = @Error,
                NextAttemptOn = NOW() + (LEAST(Attempts + 1, 10) * INTERVAL '30 seconds')
            WHERE EventId = @EventId
            """,
            new { EventId = eventId, Error = error });

    private sealed record OutboxRow(
        Guid EventId,
        string EventType,
        string Payload,
        DateTime OccurredOn,
        int Attempts,
        string? LastError);
}
