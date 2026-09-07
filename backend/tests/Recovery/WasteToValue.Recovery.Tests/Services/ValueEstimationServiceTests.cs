using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class ValueEstimationServiceTests
{
    [Fact]
    public void Deterministic_engine_returns_formula_inputs_and_result()
    {
        var service = new ValueEstimationService(new ValueEstimationOptions(TimeSpan.FromDays(30)), TimeProvider.System);
        var observed = DateTimeOffset.UtcNow.AddDays(-1);
        var result = service.Evaluate(new(100, 150, 20, 30, "LKR", RecoveryRoute.Reuse,
            "Verified source", observed, new[] { "Community reuse" }, new[] { "Avoided waste" }), DateTimeOffset.UtcNow);

        Assert.Equal("recovery-valuation-v1", result.FormulaVersion);
        Assert.Equal(50m, result.EstimatedNetValue);
        Assert.Equal(100m, result.EstimatedProceeds);
        Assert.Equal("Verified source", result.SourceName);
        Assert.Equal(observed, result.SourceObservedAt);
        Assert.False(result.IsReferenceStale);
        Assert.Equal(new[] { "Community reuse" }, result.SocialBenefits);
        Assert.Equal(new[] { "Avoided waste" }, result.EnvironmentalBenefits);
    }

    [Fact]
    public void Deterministic_engine_supports_zero_values_and_explicit_rounding()
    {
        var service = new ValueEstimationService();
        var zero = service.Evaluate(new(0, 0, 0, 0, "LKR", RecoveryRoute.Reuse,
            "Source", DateTimeOffset.UtcNow, Array.Empty<string>(), Array.Empty<string>()));
        var rounded = service.Evaluate(new(10.005m, 10.005m, 1.005m, 2.005m, "LKR", RecoveryRoute.Reuse,
            "Source", DateTimeOffset.UtcNow, Array.Empty<string>(), Array.Empty<string>()));

        Assert.Equal(0m, zero.EstimatedNetValue);
        Assert.Equal(6.99m, rounded.EstimatedNetValue);
    }

    [Fact]
    public void Deterministic_engine_rejects_negative_values_invalid_ranges_and_currency_mismatch()
    {
        var service = new ValueEstimationService();
        var source = DateTimeOffset.UtcNow;
        var negative = Assert.Throws<RecoveryException>(() => service.Evaluate(new(-1, 1, 0, 0, "LKR", RecoveryRoute.Reuse, "Source", source, Array.Empty<string>(), Array.Empty<string>())));
        var range = Assert.Throws<RecoveryException>(() => service.Evaluate(new(2, 1, 0, 0, "LKR", RecoveryRoute.Reuse, "Source", source, Array.Empty<string>(), Array.Empty<string>())));
        var currency = Assert.Throws<RecoveryException>(() => service.Evaluate(new(1, 2, 0, 0, "LKR", RecoveryRoute.Reuse, "Source", source, Array.Empty<string>(), Array.Empty<string>(), "USD")));

        Assert.Equal("invalid_input", negative.Code);
        Assert.Equal("invalid_input", range.Code);
        Assert.Equal("invalid_input", currency.Code);
    }

    [Fact]
    public void Deterministic_engine_marks_old_references_stale()
    {
        var service = new ValueEstimationService(new ValueEstimationOptions(TimeSpan.FromDays(7)), TimeProvider.System);
        var now = DateTimeOffset.UtcNow;
        var result = service.Evaluate(new(100, 120, 0, 0, "LKR", RecoveryRoute.Reuse,
            "Old source", now.AddDays(-8), Array.Empty<string>(), Array.Empty<string>()), now);

        Assert.True(result.IsReferenceStale);
        Assert.Contains(result.Warnings, warning => warning.Contains("stale", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Donation_never_fabricates_monetary_profit()
    {
        var service = new ValueEstimationService();
        var result = service.Evaluate(new(500, 800, 0, 25, "LKR", RecoveryRoute.Donate,
            "Donation reference", DateTimeOffset.UtcNow, new[] { "Community benefit" }, new[] { "Diversion" }));

        Assert.Equal(0m, result.EstimatedProceeds);
        Assert.Equal(-25m, result.EstimatedNetValue);
        Assert.Contains(result.Warnings, warning => warning.Contains("non-financial", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(new[] { "Community benefit" }, result.SocialBenefits);
    }

    [Fact]
    public void ValueEstimationService_Calculates_Conservative_Surplus()
    {
        var service = new ValueEstimationService();
        var result = service.Calculate(100, 150, 20, 30, "LKR", "LKR");

        Assert.Equal(50, result.NetValue);
        Assert.Equal(0, result.Shortfall);
    }

    [Fact]
    public void ValueEstimationService_Shows_Donation_Costs_As_Shortfall()
    {
        var service = new ValueEstimationService();
        var loss = service.Calculate(0, 0, 0, 500, "LKR", "LKR");

        Assert.Equal(0, loss.NetValue);
        Assert.Equal(500, loss.Shortfall);
    }

    [Fact]
    public void ValueEstimationService_Rejects_Negative_Values()
    {
        var service = new ValueEstimationService();

        var exception = Assert.Throws<RecoveryException>(() =>
            service.Calculate(-1, 1, 0, 0, "LKR", "LKR"));

        Assert.Equal("invalid_input", exception.Code);

        exception = Assert.Throws<RecoveryException>(() =>
            service.Calculate(2, 1, 0, 0, "LKR", "LKR"));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void ValueEstimationService_Rejects_Currency_Mismatch()
    {
        var service = new ValueEstimationService();

        var exception = Assert.Throws<RecoveryException>(() =>
            service.Calculate(1, 2, 0, 0, "LKR", "USD"));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void ValueEstimationService_Rejects_Invalid_Net_Value_Range()
    {
        var service = new ValueEstimationService();

        var exception = Assert.Throws<RecoveryException>(() =>
            service.Calculate(0.001m, 1, 0, 0, "LKR", "LKR"));

        Assert.Equal("invalid_input", exception.Code);

        exception = Assert.Throws<RecoveryException>(() =>
            service.Calculate(1, 2, 9999999999.99m, 1, "LKR", "LKR"));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void ValueEstimationService_Rejects_Invalid_Currency_Code()
    {
        var exception = Assert.Throws<RecoveryException>(() =>
            RecoveryRequestValidator.Currency("lkr"));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void ValueEstimationService_Ensures_No_Economic_Loss_Disappears()
    {
        var service = new ValueEstimationService();
        for (var gross = 0; gross <= 100; gross += 5)
        for (var cost = 0; cost <= 100; cost += 5)
        {
            var estimate = service.Calculate(gross, gross + 1, cost, 0, "LKR", "LKR");
            Assert.Equal(gross - cost, estimate.NetValue - estimate.Shortfall);
            Assert.True(estimate.NetValue == 0 || estimate.Shortfall == 0);
        }
    }
}