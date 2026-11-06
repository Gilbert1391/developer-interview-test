using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

/// <summary>
/// Abstraction over rebate persistence so the service can be tested
/// without a real database
/// </summary>
public interface IRebateDataStore
{
    Rebate GetRebate(string rebateIdentifier);

    void StoreCalculationResult(Rebate account, decimal rebateAmount);
}
