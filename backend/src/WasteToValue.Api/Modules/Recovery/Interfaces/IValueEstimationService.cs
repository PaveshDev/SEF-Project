using WasteToValue.Api.Modules.Recovery.DTOs;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IValueEstimationService
{
    ValueEstimate Calculate(decimal valueLow, decimal valueHigh, decimal repairCost,
        decimal pickupCost, string currency, string pickupCurrency);
}
