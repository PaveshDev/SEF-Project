using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class ValueEstimationService : IValueEstimationService
{
    public ValueEstimate Calculate(decimal valueLow, decimal valueHigh, decimal repairCost,
        decimal pickupCost, string currency, string pickupCurrency)
    {
        foreach (var amount in new[] { valueLow, valueHigh, repairCost, pickupCost }) RecoveryRequestValidator.Money(amount);
        RecoveryRequestValidator.Currency(currency);
        RecoveryRequestValidator.Currency(pickupCurrency);
        if (currency != pickupCurrency) throw RecoveryException.Invalid("Currency conversion is not supported.");
        if (valueLow > valueHigh) throw RecoveryException.Invalid("Minimum value exceeds maximum value.");
        var total = repairCost + pickupCost;
        RecoveryRequestValidator.Money(total);
        return new(valueLow, valueHigh, repairCost, pickupCost,
            Math.Max(valueLow - total, 0), Math.Max(total - valueLow, 0), currency);
    }
}
