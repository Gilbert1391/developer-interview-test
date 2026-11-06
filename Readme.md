# Smartwyre Developer Test Instructions

You have been selected to complete our candidate coding exercise. Please follow the directions in this readme.

Clone, **DO NOT FORK**, this repository to your account on the online Git resource of your choosing (GitHub, BitBucket, GitLab, etc.). Your solution should retain previous commit history and you should utilize best practices for committing your changes to the repository.

You are welcome to use whatever tools you normally would when coding — including documentation, libraries, frameworks, or AI tools (such as ChatGPT or Copilot).

However, it is important that you fully understand your solution. As part of the interview process, we will review your code with you in detail. You should be able to:

- Explain the design choices you made.
- Walk us through how your solution works.
- Make modifications or extensions to your code during the review.

Please note: if your submission appears to have been generated entirely by an AI agent or another third party, without your own understanding or contribution, it will not meet our evaluation criteria.

# The Exercise

In the 'RebateService.cs' file you will find a method for calculating a rebate. At a high level the steps for calculating a rebate are:

 1. Lookup the rebate that the request is being made against.
 2. Lookup the product that the request is being made against.
 2. Check that the rebate and request are valid to calculate the incentive type rebate.
 3. Store the rebate calculation.

What we'd like you to do is refactor the code with the following things in mind:

 - Adherence to SOLID principles
 - Testability
 - Readability
 - Currently there are 3 known incentive types. In the future the business will want to add many more incentive types. Your solution should make it easy for developers to add new incentive types in the future.

We’d also like you to 
 - Add some unit tests to the Smartwyre.DeveloperTest.Tests project to show how you would test the code that you’ve produced 
 - Run the RebateService from the Smartwyre.DeveloperTest.Runner console application accepting inputs (either via command line arguments or via prompts is fine)

The only specific "rules" are:

- The solution must build
- All tests must pass

You are free to use any frameworks/NuGet packages that you see fit. You should plan to spend around 1 hour completing the exercise.

Feel free to use code comments to describe your changes. You are also welcome to update this readme with any important details for us to consider.

Once you have completed the exercise either ensure your repository is available publicly or contact the hiring manager to set up a private share.

# Solution Notes

## Running it

Requires the .NET 10 SDK.

```bash
dotnet build
dotnet test
dotnet run --project Smartwyre.DeveloperTest.Runner -- UOM2 WIDGET 2.5   # arguments: rebate, product, volume
dotnet run --project Smartwyre.DeveloperTest.Runner                      # no arguments: prompts for each value
```

The runner uses in-memory sample data (rebates `CASH10`, `RATE5`, `UOM2`; products `WIDGET`, `GADGET`) and lists it when prompting. Exit codes: `0` calculated, `1` invalid input, `2` rebate could not be calculated.

## Design

- **`RebateService`** coordinates the calculation: it looks up the rebate and product, picks the calculator for the rebate's incentive type, and stores the result. It no longer contains any incentive-specific rules.
- **`IIncentiveCalculator`** has one implementation per incentive type (`FixedCashAmountCalculator`, `FixedRateRebateCalculator`, `AmountPerUomCalculator`). Each owns its validation (`CanCalculate`) and formula (`Calculate`). The rules are unchanged from the original switch statement.
- **`IRebateDataStore` / `IProductDataStore`** are injected into the service instead of being created inside it, so the service can be tested with fakes and the runner can use in-memory stores.
- Dependencies are wired with `Microsoft.Extensions.DependencyInjection` in the runner's `Program.cs`.

## Adding a new incentive type

1. Add a value to `IncentiveType` and a flag to `SupportedIncentiveType`.
2. Add a class implementing `IIncentiveCalculator`.
3. Register it in `Program.cs`: `.AddSingleton<IIncentiveCalculator, MyNewCalculator>()`.
4. Add a test class for the calculator.

`RebateService` and the existing calculators do not need to change.

## Other changes

- Fixed a `NullReferenceException` when the rebate was not found (the incentive type was read before the null check) and when a `FixedCashAmount` rebate had no product.
- `CalculateRebateResult` now includes the calculated `Amount`.
- `SupportedIncentiveType` is now marked `[Flags]`, since its values are combined as bit flags.
- Added the missing project reference from the test project, and replaced the placeholder `PaymentService` test.
- Tests use xUnit and NSubstitute: calculator tests cover each incentive's rules, and `RebateService` tests cover the orchestration with faked dependencies.

## Possible next steps

- Return a failure reason (rebate not found, product not supported, missing values) instead of only `Success = false`.
- Decide whether negative amounts, percentages or volumes should be rejected. The original behavior was kept.
