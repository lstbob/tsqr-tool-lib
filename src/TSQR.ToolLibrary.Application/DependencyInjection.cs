using Microsoft.Extensions.DependencyInjection;
using TSQR.ToolLibrary.Application.Events;
using TSQR.ToolLibrary.Application.Inventory.Commands;
using TSQR.ToolLibrary.Application.Loan.Commands;
using TSQR.ToolLibrary.Application.Member.Commands;
using TSQR.ToolLibrary.Application.Reservation.Commands;
using TSQR.ToolLibrary.Application.Tool.Commands;

namespace TSQR.ToolLibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddToolFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<RegisterToolCommand, Result<ToolId>>, RegisterToolCommandHandler>();
        services.AddScoped<IInteractor<UpdateToolDetailsCommand, Result>, UpdateToolDetailsCommandHandler>();
        services.AddScoped<IInteractor<SetScarcityLevelCommand, Result>, SetScarcityLevelCommandHandler>();
        services.AddScoped<IInteractor<RemoveScarcityLevelCommand, Result>, RemoveScarcityLevelCommandHandler>();
        return services;
    }

    public static IServiceCollection AddMemberFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<RegisterMemberCommand, Result<MemberId>>, RegisterMemberCommandHandler>();
        services.AddScoped<IInteractor<VerifyMemberCommand, Result>, VerifyMemberCommandHandler>();
        services.AddScoped<IInteractor<RequestMemberAccessCommand, Result>, RequestMemberAccessCommandHandler>();
        services.AddScoped<IInteractor<ApproveMemberAccessCommand, Result>, ApproveMemberAccessCommandHandler>();
        services.AddScoped<IInteractor<DenyMemberAccessCommand, Result>, DenyMemberAccessCommandHandler>();
        services.AddScoped<IInteractor<SuspendMemberCommand, Result>, SuspendMemberCommandHandler>();
        services.AddScoped<IInteractor<BanMemberCommand, Result>, BanMemberCommandHandler>();
        services.AddScoped<IInteractor<ReinstateMemberCommand, Result>, ReinstateMemberCommandHandler>();
        return services;
    }

    public static IServiceCollection AddReservationFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<ReserveToolCommand, Result<ReservationId>>, ReserveToolCommandHandler>();
        services.AddScoped<IInteractor<ActivateReservationCommand, Result>, ActivateReservationCommandHandler>();
        services.AddScoped<IInteractor<CancelReservationCommand, Result>, CancelReservationCommandHandler>();
        services.AddScoped<IInteractor<ConfirmPickupCommand, Result>, ConfirmPickupCommandHandler>();
        services.AddScoped<IInteractor<CompleteReservationCommand, Result>, CompleteReservationCommandHandler>();
        return services;
    }

    public static IServiceCollection AddLoanFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<LoanToolCommand, Result>, LoanToolCommandHandler>();
        services.AddScoped<IInteractor<MarkLoanAsNotReturnedCommand, Result>, MarkLoanAsNotReturnedCommandHandler>();
        return services;
    }

    public static IServiceCollection AddInventoryFeatures(this IServiceCollection services)
    {
        services.AddScoped<IInteractor<ReturnToolCommand, Result>, ReturnToolCommandHandler>();
        services.AddScoped<IInteractor<MarkToolForRepairCommand, Result>, MarkToolForRepairCommandHandler>();
        services.AddScoped<IInteractor<CompleteRepairCommand, Result>, CompleteRepairCommandHandler>();
        services.AddScoped<IInteractor<MarkToolAsLostCommand, Result>, MarkToolAsLostCommandHandler>();
        return services;
    }

    public static IServiceCollection AddDomainEventFeatures(this IServiceCollection services)
    {
        services.AddScoped<DomainEventOrchestrator>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<ReservationCancelledEvent>, ReservationCancelledEventHandler>();
        services.AddScoped<IDomainEventHandler<ToolReturnedEvent>, ToolReturnedEventHandler>();
        services.AddScoped<IDomainEventHandler<InventoryItemRequiredEvent>, InventoryItemRequiredEventHandler>();
        services.AddScoped<IDomainEventHandler<LoanCreatedDomainEvent>, LoanCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ReservationCreatedDomainEvent>, ReservationCreatedDomainEventHandler>();
        services.AddScoped<IDomainEventHandler<ToolMarkedForRepairEvent>, ToolMarkedForRepairNotificationHandler>();
        services.AddScoped<IDomainEventHandler<NextInLineNotificationEvent>, NextInLineNotificationHandler>();
        services.AddScoped<IDomainEventHandler<PickupReminderEvent>, PickupReminderHandler>();
        services.AddScoped<IDomainEventHandler<ReturnReminderEvent>, ReturnReminderHandler>();
        return services;
    }

    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddToolFeatures();
        services.AddMemberFeatures();
        services.AddReservationFeatures();
        services.AddLoanFeatures();
        services.AddInventoryFeatures();
        services.AddDomainEventFeatures();
        return services;
    }
}