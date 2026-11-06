using System;
using System.Collections.Generic;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner.Data;

/// <summary>
/// Demo store with a few sample products, so the Runner can be used without a database.
/// </summary>
public class InMemoryProductDataStore : IProductDataStore
{
    private readonly Dictionary<string, Product> _products = new(StringComparer.OrdinalIgnoreCase)
    {
        ["WIDGET"] = new Product
        {
            Id = 1,
            Identifier = "WIDGET",
            Price = 100m,
            Uom = "each",
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
                | SupportedIncentiveType.FixedRateRebate
                | SupportedIncentiveType.AmountPerUom
        },
        ["GADGET"] = new Product
        {
            Id = 2,
            Identifier = "GADGET",
            Price = 50m,
            Uom = "box",
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate
        },
    };

    public IEnumerable<Product> All => _products.Values;

    public Product GetProduct(string productIdentifier)
    {
        return productIdentifier != null && _products.TryGetValue(productIdentifier, out var product) ? product : null;
    }
}
