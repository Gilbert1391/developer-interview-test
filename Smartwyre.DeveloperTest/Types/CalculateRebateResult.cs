namespace Smartwyre.DeveloperTest.Types;

public class CalculateRebateResult
{
    public bool Success { get; set; }

    /// <summary>The calculated rebate amount. Zero when <see cref="Success"/> is false.</summary>
    public decimal Amount { get; set; }
}
