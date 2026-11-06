using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

/// <summary>
/// Abstraction over product lookup so the service can be tested
/// without a real database
/// </summary>
public interface IProductDataStore
{
    Product GetProduct(string productIdentifier);
}
