using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class ValueEstimationServiceTests
{
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