using Microsoft.Extensions.DependencyInjection.Extensions;
using WasteToValue.Api.Modules.Recovery.Controllers;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery;

public static class ModuleRegistration
{
    public static IServiceCollection AddRecoveryModule(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IRecoveryActorAccessor, UnavailableRecoveryIntegrations>();
        services.TryAddScoped<IRecoveryCommandExecutor, UnavailableRecoveryIntegrations>();
        services.TryAddScoped<IAssessmentGateway, UnavailableRecoveryIntegrations>();
        services.TryAddScoped<IMatchingGateway, UnavailableRecoveryIntegrations>();
        services.TryAddScoped<IPickupPlanningGateway, UnavailableRecoveryIntegrations>();
        services.TryAddScoped<IRecoveryRepository, EfRecoveryRepository>();
        services.TryAddScoped<RecoveryAccess>();
        services.TryAddScoped<IValueEstimationService, ValueEstimationService>();
        services.TryAddScoped<IRecoveryPlanningService, RecoveryPlanningService>();
        services.TryAddScoped<IProposalDecisionService, ProposalDecisionService>();
        services.TryAddScoped<ValueReferenceService>();
        services.TryAddScoped<RecoveryAgentOutputValidator>();
        services.TryAddScoped<RecoveryExceptionFilter>();
        return services;
    }
}
