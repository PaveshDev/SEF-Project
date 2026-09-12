using WasteToValue.Api.Modules.Partners.Interfaces;
using WasteToValue.Api.Modules.Partners.Services;

namespace WasteToValue.Api.Modules.Partners;

public static class ModuleRegistration
{
    public static IServiceCollection AddPartnersModule(this IServiceCollection services)
    {
        services.AddScoped<IPartnersService, PartnersService>();
        services.AddScoped<IAcceptanceRulesService, AcceptanceRulesService>();
        services.AddScoped<IRecipientNeedsService, RecipientNeedsService>();
        return services;
    }
}
