namespace RetalSystemAPI.Models.Common;

/// <summary>Stock quantities are whole counts of the product's smallest unit (grams, pieces, etc.).</summary>
public static class StockQuantity
{
    public static bool IsValid(decimal quantity) =>
        quantity > 0 && quantity <= int.MaxValue && quantity == decimal.Truncate(quantity);

    public static int ToInt32(decimal quantity)
    {
        if (!IsValid(quantity))
            throw new ArgumentOutOfRangeException(nameof(quantity),
                "كمية المخزون يجب أن تكون عدداً صحيحاً موجباً من أصغر وحدة، ضمن الحد المسموح.");

        return decimal.ToInt32(quantity);
    }
}
