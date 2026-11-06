using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests.Calculators;

public class FixedCashAmountCalculatorTests
{
    private readonly FixedCashAmountCalculator _calculator = new();

    [Fact]
    public void IncentiveType_IsFixedCashAmount()
    {
        Assert.Equal(IncentiveType.FixedCashAmount, _calculator.IncentiveType);
    }

    [Fact]
    public void CanCalculate_ReturnsTrue_WhenProductSupportsIncentiveAndAmountIsSet()
    {
        var rebate = new Rebate { Amount = 50m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        Assert.True(_calculator.CanCalculate(rebate, product, new CalculateRebateRequest()));
    }

    [Fact]
    public void CanCalculate_ReturnsFalse_WhenProductDoesNotSupportIncentive()
    {
        var rebate = new Rebate { Amount = 50m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };

        Assert.False(_calculator.CanCalculate(rebate, product, new CalculateRebateRequest()));
    }

    [Fact]
    public void CanCalculate_ReturnsFalse_WhenAmountIsZero()
    {
        var rebate = new Rebate { Amount = 0m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        Assert.False(_calculator.CanCalculate(rebate, product, new CalculateRebateRequest()));
    }

    [Fact]
    public void Calculate_ReturnsRebateAmount_RegardlessOfVolume()
    {
        var rebate = new Rebate { Amount = 50m };
        var product = new Product { Price = 10m };
        var request = new CalculateRebateRequest { Volume = 7m };

        Assert.Equal(50m, _calculator.Calculate(rebate, product, request));
    }
}
