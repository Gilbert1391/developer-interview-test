using System;
using System.Collections.Generic;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner.Data;

/// <summary>
/// Demo store with a few sample rebates, so the Runner can be used without
/// a database. Stored calculations are written to the console.
/// </summary>
public class InMemoryRebateDataStore : IRebateDataStore
{
    private readonly Dictionary<string, Rebate> _rebates = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CASH10"] = new Rebate { Identifier = "CASH10", Incentive = IncentiveType.FixedCashAmount, Amount = 10m },
        ["RATE5"] = new Rebate { Identifier = "RATE5", Incentive = IncentiveType.FixedRateRebate, Percentage = 0.05m },
        ["UOM2"] = new Rebate { Identifier = "UOM2", Incentive = IncentiveType.AmountPerUom, Amount = 2m },
    };

    public IEnumerable<Rebate> All => _rebates.Values;

    public Rebate GetRebate(string rebateIdentifier)
    {
        return rebateIdentifier != null && _rebates.TryGetValue(rebateIdentifier, out var rebate) ? rebate : null;
    }

    public void StoreCalculationResult(Rebate account, decimal rebateAmount)
    {
        Console.WriteLine($"[store] Saved calculation for rebate {account.Identifier}: {rebateAmount:0.00}");
    }
}
