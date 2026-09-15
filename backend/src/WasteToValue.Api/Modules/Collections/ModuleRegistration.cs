using WasteToValue.Api.Modules.Collections.Agents.Tools;
using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Services;

namespace WasteToValue.Api.Modules.Collections;

public static class ModuleRegistration
{
    public static IServiceCollection AddCollectionsModule(this IServiceCollection services)
    {
        // Collection slot management
        services.AddScoped<ICollectionSlotService, CollectionSlotService>();

        // Pickup request lifecycle
        services.AddScoped<IPickupRequestService, PickupRequestService>();

        // Handover verification
        services.AddScoped<IHandoverService, HandoverService>();

        // External routing/maps API
        services.AddScoped<ITravelEstimateService, TravelEstimateService>();

        // Collection Agent allow-listed tools
        services.AddScoped<ICollectionAgentTools, CollectionAgentTools>();

        // Collection Agent orchestrator
        services.AddScoped<ICollectionAgentService, CollectionAgentService>();

        return services;
    }
}
