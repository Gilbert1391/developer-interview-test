using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests.Calculators;

public class AmountPerUomCalculatorTests
{
    private readonly AmountPerUomCalculator _calculator = new();

    [Fact]
    public void IncentiveType_IsAmountPerUom()
    {
        Assert.Equal(IncentiveType.AmountPerUom, _calculator.IncentiveType);
    }

    [Fact]
    public void CanCalculate_ReturnsTrue_WhenProductSupportsIncentiveAndValuesAreSet()
    {
        var rebate = new Rebate { Amount = 2m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.True(_calculator.CanCalculate(rebate, product, request));
    }

    [Fact]
    public void CanCalculate_ReturnsTrue_WhenProductSupportsSeveralIncentives()
    {
        var rebate = new Rebate { Amount = 2m };
        var product = new Product
        {
            SupportedIncentives = SupportedIncentiveType.AmountPerUom | SupportedIncentiveType.FixedCashAmount
        };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.True(_calculator.CanCalculate(rebate, product, request));
    }

    [Fact]
    public void CanCalculate_ReturnsFalse_WhenProductDoesNotSupportIncentive()
    {
        var rebate = new Rebate { Amount = 2m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedRateRebate };
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.False(_calculator.CanCalculate(rebate, product, request));
    }

    [Theory]
    [InlineData(0, 5)] // no amount
    [InlineData(2, 0)] // no volume
    public void CanCalculate_ReturnsFalse_WhenARequiredValueIsZero(double amount, double volume)
    {
        var rebate = new Rebate { Amount = (decimal)amount };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };
        var request = new CalculateRebateRequest { Volume = (decimal)volume };

        Assert.False(_calculator.CanCalculate(rebate, product, request));
    }

    [Fact]
    public void Calculate_ReturnsAmountTimesVolume()
    {
        var rebate = new Rebate { Amount = 2m };
        var product = new Product();
        var request = new CalculateRebateRequest { Volume = 5m };

        Assert.Equal(10m, _calculator.Calculate(rebate, product, request));
    }
}
