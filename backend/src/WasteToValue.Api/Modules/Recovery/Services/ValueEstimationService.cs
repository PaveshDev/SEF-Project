using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Services;

public sealed class ValueEstimationService : IValueEstimationService
{
    public static readonly TimeSpan MaximumReferenceAge = TimeSpan.FromDays(30);
    private static readonly ValueEstimationOptions DefaultOptions = new(MaximumReferenceAge);
    private readonly ValueEstimationOptions options;
    private readonly TimeProvider time;

    public ValueEstimationService() : this(DefaultOptions, TimeProvider.System) { }
    public ValueEstimationService(ValueEstimationOptions options, TimeProvider time)
    {
        this.options = options;
        this.time = time;
    }

    public ValueEstimationResult Evaluate(ValueEstimationInput input, DateTimeOffset? asOf = null)
    {
        ValidateRawMoney(input.EstimatedProceedsLow, "EstimatedProceedsLow");
        ValidateRawMoney(input.EstimatedProceedsHigh, "EstimatedProceedsHigh");
        ValidateRawMoney(input.EstimatedRepairCost, "EstimatedRepairCost");
        ValidateRawMoney(input.EstimatedPickupCost, "EstimatedPickupCost");
        if (input.EstimatedProceedsLow > input.EstimatedProceedsHigh)
            throw RecoveryException.Invalid("Minimum proceeds must not exceed maximum proceeds.");
        RecoveryRequestValidator.Currency(input.Currency);
        var costCurrency = input.CostCurrency ?? input.Currency;
        RecoveryRequestValidator.Currency(costCurrency);
        if (input.Currency != costCurrency) throw RecoveryException.Invalid("Currency conversion is not supported.");
        RecoveryRequestValidator.Defined(input.Route);
        RecoveryRequestValidator.Text(input.SourceName, "SourceName", 200);
        var now = asOf ?? time.GetUtcNow();
        if (input.SourceObservedAt == default || input.SourceObservedAt > now)
            throw RecoveryException.Invalid("SourceObservedAt must be a known past or current timestamp.");

        var proceeds = Round(input.Route == RecoveryRoute.Donate ? 0 : input.EstimatedProceedsLow);
        var repair = Round(input.EstimatedRepairCost);
        var pickup = Round(input.EstimatedPickupCost);
        RecoveryRequestValidator.Money(proceeds);
        RecoveryRequestValidator.Money(Round(input.EstimatedProceedsHigh));
        RecoveryRequestValidator.Money(repair + pickup);
        var estimate = Calculate(proceeds, input.Route == RecoveryRoute.Donate ? 0 : Round(input.EstimatedProceedsHigh), repair, pickup, input.Currency, costCurrency);
        var net = estimate.NetValue - estimate.Shortfall;
        var stale = now - input.SourceObservedAt > options.MaximumReferenceAge;
        var warnings = new List<string>();
        if (stale) warnings.Add("The value reference is stale and should be reviewed.");
        if (input.Route == RecoveryRoute.Donate) warnings.Add("Donation has no fabricated monetary profit; benefits remain non-financial.");
        return new(input, options.FormulaVersion, proceeds, repair, pickup, net, input.Currency,
            input.SourceName.Trim(), input.SourceObservedAt.ToUniversalTime(), stale, warnings,
            input.SocialBenefits.ToArray(), input.EnvironmentalBenefits.ToArray());
    }

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
            Round(Math.Max(valueLow - total, 0)), Round(Math.Max(total - valueLow, 0)), currency);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static void ValidateRawMoney(decimal value, string field)
    {
        if (value < 0 || value > 9999999999.99m)
            throw RecoveryException.Invalid($"{field} must be nonnegative and fit numeric(12,2).");
    }
}
