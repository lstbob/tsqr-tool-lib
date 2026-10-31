using TSQR.Common.Events;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_InvokesEveryRegisteredHandlerForTheRuntimeEventType()
    {
        var domainEvent = new TestEvent("payload");
        var first = new RecordingHandler();
        var second = new RecordingHandler();
        var dispatcher = new DomainEventDispatcher(
            new HandlerServiceProvider(first, second, new UnrelatedHandler()));

        await dispatcher.DispatchAsync([domainEvent], CancellationToken.None);

        Assert.Same(domainEvent, Assert.Single(first.Handled));
        Assert.Same(domainEvent, Assert.Single(second.Handled));
    }

    [Fact]
    public async Task DispatchAsync_NoRegisteredHandlers_CompletesWithoutThrowing()
    {
        var dispatcher = new DomainEventDispatcher(new HandlerServiceProvider());

        await dispatcher.DispatchAsync([new TestEvent("payload")], CancellationToken.None);
    }

    [Fact]
    public async Task DispatchAsync_HandlerExceptionPropagatesUnwrapped()
    {
        var dispatcher = new DomainEventDispatcher(
            new HandlerServiceProvider(new ThrowingHandler()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await dispatcher.DispatchAsync([new TestEvent("payload")], CancellationToken.None));

        Assert.Equal("handler boom", ex.Message);
    }

    [Fact]
    public async Task DispatchPostCommitAsync_InvokesOnlyPostCommitHandlers()
    {
        var domainEvent = new TestEvent("payload");
        var inTransaction = new RecordingHandler();
        var postCommit = new PostCommitHandler();
        var dispatcher = new DomainEventDispatcher(
            new HandlerServiceProvider(inTransaction, postCommit));

        await dispatcher.DispatchAsync([domainEvent], CancellationToken.None);

        Assert.Single(inTransaction.Handled);
        Assert.Empty(postCommit.Handled);

        await dispatcher.DispatchPostCommitAsync([domainEvent], CancellationToken.None);

        Assert.Single(inTransaction.Handled);
        Assert.Single(postCommit.Handled);
    }

    private sealed record TestEvent(string Payload) : DomainEvent;

    private sealed class RecordingHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> Handled { get; } = [];

        public ValueTask HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Handled.Add(domainEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class PostCommitHandler : IPostCommitDomainEventHandler<TestEvent>
    {
        public List<TestEvent> Handled { get; } = [];

        public ValueTask HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default)
        {
            Handled.Add(domainEvent);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class UnrelatedHandler : IDomainEventHandler<OtherEvent>
    {
        public ValueTask HandleAsync(OtherEvent domainEvent, CancellationToken cancellationToken = default) =>
            ValueTask.CompletedTask;
    }

    private sealed class ThrowingHandler : IDomainEventHandler<TestEvent>
    {
        public ValueTask HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("handler boom");
    }

    private sealed record OtherEvent : DomainEvent;

    private sealed class HandlerServiceProvider(params object[] handlers) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (!serviceType.IsGenericType ||
                serviceType.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                return null;

            var elementType = serviceType.GetGenericArguments()[0];
            var matching = handlers.Where(elementType.IsInstanceOfType).ToArray();
            var result = Array.CreateInstance(elementType, matching.Length);
            for (var i = 0; i < matching.Length; i++)
                result.SetValue(matching[i], i);
            return result;
        }
    }
}
