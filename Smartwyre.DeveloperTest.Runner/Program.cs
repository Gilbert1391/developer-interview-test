using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Runner.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Calculators;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

// Usage:
//   dotnet run -- <rebateIdentifier> <productIdentifier> <volume>
//   dotnet run                      (prompts for each value)
class Program
{
    static int Main(string[] args)
    {
        using var serviceProvider = BuildServiceProvider();

        if (args.Length != 0 && args.Length != 3)
        {
            Console.Error.WriteLine("Usage: <rebateIdentifier> <productIdentifier> <volume>, or no arguments to be prompted.");
            return 1;
        }

        var request = args.Length == 3 ? ParseArguments(args) : PromptForRequest(serviceProvider);
        if (request == null)
        {
            return 1;
        }

        var rebateService = serviceProvider.GetRequiredService<IRebateService>();
        var result = rebateService.Calculate(request);

        if (result.Success)
        {
            Console.WriteLine($"Rebate calculated: {result.Amount:0.00}");
            return 0;
        }

        Console.WriteLine("Rebate could not be calculated for this rebate, product and volume.");
        return 2;
    }

    // All wiring lives here. To support a new incentive type, register its calculator below.
    private static ServiceProvider BuildServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton<InMemoryRebateDataStore>()
            .AddSingleton<InMemoryProductDataStore>()
            .AddSingleton<IRebateDataStore>(sp => sp.GetRequiredService<InMemoryRebateDataStore>())
            .AddSingleton<IProductDataStore>(sp => sp.GetRequiredService<InMemoryProductDataStore>())
            .AddSingleton<IIncentiveCalculator, FixedCashAmountCalculator>()
            .AddSingleton<IIncentiveCalculator, FixedRateRebateCalculator>()
            .AddSingleton<IIncentiveCalculator, AmountPerUomCalculator>()
            .AddSingleton<IRebateService, RebateService>()
            .BuildServiceProvider();
    }

    private static CalculateRebateRequest ParseArguments(string[] args)
    {
        if (!TryParseVolume(args[2], out var volume))
        {
            Console.Error.WriteLine($"'{args[2]}' is not a valid volume.");
            return null;
        }

        return new CalculateRebateRequest
        {
            RebateIdentifier = args[0],
            ProductIdentifier = args[1],
            Volume = volume
        };
    }

    private static CalculateRebateRequest PromptForRequest(IServiceProvider serviceProvider)
    {
        PrintSampleData(serviceProvider);

        var rebateIdentifier = Prompt("Rebate identifier");
        var productIdentifier = Prompt("Product identifier");

        decimal volume;
        while (!TryParseVolume(Prompt("Volume"), out volume))
        {
            Console.WriteLine("Please enter a number, for example 5 or 2.5.");
        }

        return new CalculateRebateRequest
        {
            RebateIdentifier = rebateIdentifier,
            ProductIdentifier = productIdentifier,
            Volume = volume
        };
    }

    private static void PrintSampleData(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Sample rebates:");
        foreach (var rebate in serviceProvider.GetRequiredService<InMemoryRebateDataStore>().All)
        {
            Console.WriteLine($"  {rebate.Identifier,-8} {rebate.Incentive,-16} amount {rebate.Amount}, percentage {rebate.Percentage}");
        }

        Console.WriteLine("Sample products:");
        foreach (var product in serviceProvider.GetRequiredService<InMemoryProductDataStore>().All)
        {
            Console.WriteLine($"  {product.Identifier,-8} price {product.Price} per {product.Uom}, supports {product.SupportedIncentives}");
        }

        Console.WriteLine();
    }

    private static string Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine()?.Trim();
    }

    // Invariant culture so "2.5" means the same thing on every machine.
    private static bool TryParseVolume(string input, out decimal volume) =>
        decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out volume);
}
