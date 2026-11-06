using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests.Calculators;

public class FixedRateRebateCalculatorTests
{
    private readonly FixedRateRebateCalculator _calculator = new();

    [Fact]
    public void IncentiveType_IsFixedRateRebate()
    {
        Assert.Equal(IncentiveType.FixedRateRebate, _calculator.IncentiveType);
    }

    [Fact]
    public void CanCalculate_ReturnsTrue_WhenProductSupportsIncentiveAndValuesAreSet()
    {
        var rebate = new Rebate { Percentage = 0.1m };
        var product = new Product { Price = 20m, SupportedIncentives = SupportedIncentiveType.FixedRateRebate };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.True(_calculator.CanCalculate(rebate, product, request));
    }

    [Fact]
    public void CanCalculate_ReturnsFalse_WhenProductDoesNotSupportIncentive()
    {
        var rebate = new Rebate { Percentage = 0.1m };
        var product = new Product { Price = 20m, SupportedIncentives = SupportedIncentiveType.FixedCashAmount };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.False(_calculator.CanCalculate(rebate, product, request));
    }

    // decimal can't be used in attributes, so the values are passed as double and converted.
    [Theory]
    [InlineData(0, 20, 5)]   // no percentage
    [InlineData(0.1, 0, 5)]  // no price
    [InlineData(0.1, 20, 0)] // no volume
    public void CanCalculate_ReturnsFalse_WhenARequiredValueIsZero(double percentage, double price, double volume)
    {
        var rebate = new Rebate { Percentage = (decimal)percentage };
        var product = new Product { Price = (decimal)price, SupportedIncentives = SupportedIncentiveType.FixedRateRebate };
        var request = new CalculateRebateRequest { Volume = (decimal)volume };

        Assert.False(_calculator.CanCalculate(rebate, product, request));
    }

    [Fact]
    public void Calculate_ReturnsPriceTimesPercentageTimesVolume()
    {
        var rebate = new Rebate { Percentage = 0.1m };
        var product = new Product { Price = 20m };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.Equal(10m, _calculator.Calculate(rebate, product, request));
    }
}
