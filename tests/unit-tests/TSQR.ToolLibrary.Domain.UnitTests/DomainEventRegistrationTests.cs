using Microsoft.Extensions.DependencyInjection;
using TSQR.Common.Events;
using TSQR.ToolLibrary.Application;

namespace TSQR.ToolLibrary.Domain.UnitTests;

public class DomainEventRegistrationTests
{
    [Fact]
    public void EveryDomainEvent_HasRegisteredHandler()
    {
        var services = new DescriptorCollection();
        services.AddDomainEventFeatures();

        var eventTypes = typeof(TSQR.ToolLibrary.Domain.Events.ToolRegisteredEvent).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true }
                && typeof(DomainEvent).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(eventTypes);

        foreach (var eventType in eventTypes)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var postCommitHandlerType = typeof(IPostCommitDomainEventHandler<>).MakeGenericType(eventType);

            var hasHandler = services.Any(descriptor =>
                descriptor.ServiceType == handlerType
                || descriptor.ServiceType == postCommitHandlerType);

            Assert.True(hasHandler, $"No domain event handler is registered for {eventType.Name}.");
        }
    }

    private sealed class DescriptorCollection : List<ServiceDescriptor>, IServiceCollection;
}
