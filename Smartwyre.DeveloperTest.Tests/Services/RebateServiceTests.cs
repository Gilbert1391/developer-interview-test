using System;
using NSubstitute;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests.Services;

// These tests cover how RebateService coordinates its dependencies.
// The rules for each incentive type are tested in the calculator tests.
public class RebateServiceTests
{
    private const string RebateId = "rebate-1";
    private const string ProductId = "product-1";

    private readonly IRebateDataStore _rebateDataStore = Substitute.For<IRebateDataStore>();
    private readonly IProductDataStore _productDataStore = Substitute.For<IProductDataStore>();

    private readonly Rebate _rebate = new() { Identifier = RebateId, Incentive = IncentiveType.FixedCashAmount };
    private readonly Product _product = new() { Identifier = ProductId };
    private readonly CalculateRebateRequest _request = new()
    {
        RebateIdentifier = RebateId,
        ProductIdentifier = ProductId,
        Volume = 3m
    };

    public RebateServiceTests()
    {
        _rebateDataStore.GetRebate(RebateId).Returns(_rebate);
        _productDataStore.GetProduct(ProductId).Returns(_product);
    }

    private RebateService CreateService(params IIncentiveCalculator[] calculators) =>
        new(_rebateDataStore, _productDataStore, calculators);

    private static IIncentiveCalculator CreateCalculator(IncentiveType type, bool canCalculate = true, decimal amount = 0m)
    {
        var calculator = Substitute.For<IIncentiveCalculator>();
        calculator.IncentiveType.Returns(type);
        calculator.CanCalculate(default, default, default).ReturnsForAnyArgs(canCalculate);
        calculator.Calculate(default, default, default).ReturnsForAnyArgs(amount);
        return calculator;
    }

    [Fact]
    public void Calculate_ReturnsSuccessAndStoresAmount_WhenCalculatorCanCalculate()
    {
        var calculator = CreateCalculator(IncentiveType.FixedCashAmount, amount: 42m);
        var service = CreateService(calculator);

        var result = service.Calculate(_request);

        Assert.True(result.Success);
        Assert.Equal(42m, result.Amount);
        calculator.Received(1).Calculate(_rebate, _product, _request);
        _rebateDataStore.Received(1).StoreCalculationResult(_rebate, 42m);
    }

    [Fact]
    public void Calculate_UsesTheCalculatorMatchingTheRebateIncentive()
    {
        var fixedCash = CreateCalculator(IncentiveType.FixedCashAmount, amount: 10m);
        var perUom = CreateCalculator(IncentiveType.AmountPerUom, amount: 99m);
        var service = CreateService(fixedCash, perUom);

        service.Calculate(_request);

        fixedCash.Received(1).Calculate(_rebate, _product, _request);
        perUom.DidNotReceiveWithAnyArgs().CanCalculate(default, default, default);
        perUom.DidNotReceiveWithAnyArgs().Calculate(default, default, default);
    }

    [Fact]
    public void Calculate_ReturnsFailureAndDoesNotStore_WhenRebateIsNotFound()
    {
        _rebateDataStore.GetRebate(RebateId).Returns((Rebate)null);
        var service = CreateService(CreateCalculator(IncentiveType.FixedCashAmount));

        var result = service.Calculate(_request);

        Assert.False(result.Success);
        _rebateDataStore.DidNotReceiveWithAnyArgs().StoreCalculationResult(default, default);
    }

    [Fact]
    public void Calculate_ReturnsFailureAndDoesNotStore_WhenProductIsNotFound()
    {
        _productDataStore.GetProduct(ProductId).Returns((Product)null);
        var service = CreateService(CreateCalculator(IncentiveType.FixedCashAmount));

        var result = service.Calculate(_request);

        Assert.False(result.Success);
        _rebateDataStore.DidNotReceiveWithAnyArgs().StoreCalculationResult(default, default);
    }

    [Fact]
    public void Calculate_ReturnsFailureAndDoesNotStore_WhenNoCalculatorIsRegisteredForIncentive()
    {
        var service = CreateService(CreateCalculator(IncentiveType.AmountPerUom));

        var result = service.Calculate(_request);

        Assert.False(result.Success);
        _rebateDataStore.DidNotReceiveWithAnyArgs().StoreCalculationResult(default, default);
    }

    [Fact]
    public void Calculate_ReturnsFailureAndDoesNotCalculate_WhenCalculatorCannotCalculate()
    {
        var calculator = CreateCalculator(IncentiveType.FixedCashAmount, canCalculate: false);
        var service = CreateService(calculator);

        var result = service.Calculate(_request);

        Assert.False(result.Success);
        calculator.DidNotReceiveWithAnyArgs().Calculate(default, default, default);
        _rebateDataStore.DidNotReceiveWithAnyArgs().StoreCalculationResult(default, default);
    }

    [Fact]
    public void Calculate_Throws_WhenRequestIsNull()
    {
        var service = CreateService();

        Assert.Throws<ArgumentNullException>(() => service.Calculate(null));
    }

    [Fact]
    public void Constructor_Throws_WhenTwoCalculatorsHandleTheSameIncentive()
    {
        Assert.Throws<ArgumentException>(() => CreateService(
            CreateCalculator(IncentiveType.FixedCashAmount),
            CreateCalculator(IncentiveType.FixedCashAmount)));
    }

    [Fact]
    public void Constructor_Throws_WhenDependencyIsNull()
    {
        var calculators = Array.Empty<IIncentiveCalculator>();

        Assert.Throws<ArgumentNullException>(() => new RebateService(null, _productDataStore, calculators));
        Assert.Throws<ArgumentNullException>(() => new RebateService(_rebateDataStore, null, calculators));
        Assert.Throws<ArgumentNullException>(() => new RebateService(_rebateDataStore, _productDataStore, null));
    }

    // One end-to-end check with the real calculators, so the wiring between
    // the service and an actual calculator is covered too.
    [Fact]
    public void Calculate_WithRealCalculators_StoresAmountPerUomRebate()
    {
        _rebate.Incentive = IncentiveType.AmountPerUom;
        _rebate.Amount = 2m;
        _product.SupportedIncentives = SupportedIncentiveType.AmountPerUom;
        var service = CreateService(
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator());

        var result = service.Calculate(_request);

        Assert.True(result.Success);
        _rebateDataStore.Received(1).StoreCalculationResult(_rebate, 6m);
    }
}
