using WasteToValue.Api.Modules.Collections.Interfaces;
using WasteToValue.Api.Modules.Collections.Services;

namespace WasteToValue.Api.Modules.Collections;

public static class ModuleRegistration
{
    public static IServiceCollection AddCollectionsModule(this IServiceCollection services)
    {
        // Collection slot management
        services.AddSingleton<ICollectionSlotService, CollectionSlotService>();

        // Pickup request lifecycle
        services.AddSingleton<IPickupRequestService, PickupRequestService>();

        // Handover verification
        services.AddSingleton<IHandoverService, HandoverService>();

        // External routing/maps API (placeholder — returns unavailable)
        services.AddSingleton<ITravelEstimateService, TravelEstimateService>();

        // Collection Agent orchestrator (placeholder — deterministic demo proposals)
        services.AddSingleton<ICollectionAgentService, CollectionAgentService>();

        return services;
    }
}
