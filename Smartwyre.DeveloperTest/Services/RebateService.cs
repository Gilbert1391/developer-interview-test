using System;
using System.Collections.Generic;
using System.Linq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;
    private readonly IReadOnlyDictionary<IncentiveType, IIncentiveCalculator> _calculators;

    // Data stores are injected rather than created inside Calculate, so callers
    // (and tests) decide which implementation is used.
    // Calculators are injected as a collection and indexed by incentive type, so
    // registering a new calculator is enough to support a new incentive type.
    public RebateService(
        IRebateDataStore rebateDataStore,
        IProductDataStore productDataStore,
        IEnumerable<IIncentiveCalculator> calculators)
    {
        _rebateDataStore = rebateDataStore ?? throw new ArgumentNullException(nameof(rebateDataStore));
        _productDataStore = productDataStore ?? throw new ArgumentNullException(nameof(productDataStore));
        ArgumentNullException.ThrowIfNull(calculators);

        // ToDictionary throws if two calculators claim the same incentive type,
        // which surfaces a misconfiguration at startup rather than at runtime.
        _calculators = calculators.ToDictionary(c => c.IncentiveType);
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Rebate rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        Product product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (rebate == null || product == null)
        {
            return Failed();
        }

        if (!_calculators.TryGetValue(rebate.Incentive, out var calculator))
        {
            return Failed();
        }

        if (!calculator.CanCalculate(rebate, product, request))
        {
            return Failed();
        }

        var rebateAmount = calculator.Calculate(rebate, product, request);
        _rebateDataStore.StoreCalculationResult(rebate, rebateAmount);

        return new CalculateRebateResult { Success = true, Amount = rebateAmount };
    }

    private static CalculateRebateResult Failed() => new() { Success = false };
}
