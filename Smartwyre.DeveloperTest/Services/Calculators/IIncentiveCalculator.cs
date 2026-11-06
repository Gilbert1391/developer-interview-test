using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Calculators;

/// <summary>
/// Calculates the rebate amount for a single incentive type.
/// To support a new incentive type, add a new implementation of this
/// interface; RebateService does not need to change.
/// </summary>
public interface IIncentiveCalculator
{
    /// <summary>The incentive type this calculator handles.</summary>
    IncentiveType IncentiveType { get; }

    /// <summary>
    /// Returns true when the rebate, product and request contain everything
    /// this incentive type needs to produce a valid amount.
    /// </summary>
    bool CanCalculate(Rebate rebate, Product product, CalculateRebateRequest request);

    /// <summary>
    /// Returns the rebate amount. Only call after <see cref="CanCalculate"/> returns true.
    /// </summary>
    decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request);
}
