using Microsoft.Extensions.DependencyInjection.Extensions;
using WasteToValue.Api.Modules.Recovery.DTOs;
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
        services.TryAddSingleton(new ValueEstimationOptions(TimeSpan.FromDays(30)));
        services.TryAddScoped<IRecoveryPlanningService, RecoveryPlanningService>();
        services.TryAddScoped<IProposalDecisionService, ProposalDecisionService>();
        services.TryAddScoped<ValueReferenceService>();
        services.TryAddScoped<RecoveryAgentOutputValidator>();
        services.TryAddScoped<RecoveryExceptionFilter>();
        services.TryAddSingleton<RecoveryReasoningValidator>();
        services.AddHttpClient(GeminiRecoveryReasoningProvider.HttpClientName,
                client => client.Timeout = Timeout.InfiniteTimeSpan)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false })
            .RemoveAllLoggers();
        services.TryAddScoped<IRecoveryReasoningProvider>(provider =>
        {
            var configuration = provider.GetService<IConfiguration>();
            var settings = configuration is null ? null : GeminiRecoveryReasoningOptions.FromConfiguration(configuration);
            return settings is null
                ? new UnavailableRecoveryReasoningProvider()
                : new GeminiRecoveryReasoningProvider(provider.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(GeminiRecoveryReasoningProvider.HttpClientName), settings,
                    provider.GetRequiredService<RecoveryReasoningValidator>());
        });
        return services;
    }
}
