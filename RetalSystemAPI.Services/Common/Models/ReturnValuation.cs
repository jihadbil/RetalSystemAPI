namespace RetalSystemAPI.Services.Common.Models;

internal static class ReturnValuation
{
    // Cumulative rounding preserves the invoice total even when allocations cannot be divided into four decimals.
    public static decimal[] Allocate(IReadOnlyList<decimal> weights, decimal total)
    {
        var sum = weights.Sum();
        var amounts = new decimal[weights.Count];
        decimal weightSoFar = 0, amountSoFar = 0;
        for (var i = 0; i < weights.Count; i++)
        {
            weightSoFar += weights[i];
            var cumulative = sum > 0 ? decimal.Round(total * weightSoFar / sum, 4) : 0;
            amounts[i] = cumulative - amountSoFar;
            amountSoFar = cumulative;
        }
        return amounts;
    }

    public static decimal ForQuantity(decimal amount, decimal sold, decimal previous, decimal requested) =>
        decimal.Round(amount * (previous + requested) / sold, 4) - decimal.Round(amount * previous / sold, 4);
}
