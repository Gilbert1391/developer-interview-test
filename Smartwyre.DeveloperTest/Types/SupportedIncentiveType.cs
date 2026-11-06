using System;

namespace Smartwyre.DeveloperTest.Types;

// Values are bit flags and are combined with |, so mark the enum as [Flags].
[Flags]
public enum SupportedIncentiveType
{
    FixedRateRebate = 1 << 0,
    AmountPerUom = 1 << 1,
    FixedCashAmount = 1 << 2,
}
