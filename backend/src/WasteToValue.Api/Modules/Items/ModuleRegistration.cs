using WasteToValue.Api.Modules.Items.Interfaces;
using WasteToValue.Api.Modules.Items.Services;

namespace WasteToValue.Api.Modules.Items;

public static class ModuleRegistration
{
    public static IServiceCollection AddItemsModule(this IServiceCollection services)
    {
        services.AddSingleton<IItemRepository, InMemoryItemRepository>();
        services.AddScoped<IItemService, ItemService>();
        return services;
    }
}
