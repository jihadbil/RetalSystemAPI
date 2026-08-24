namespace RetalSystemAPI.Models.Enums;

public enum StockAdjustmentReason
{
    InventoryCount = 1,  // جرد دوري
    Damaged = 2,         // بضاعة تالفة
    Expired = 3,         // بضاعة منتهية الصلاحية
    InitialSetup = 4,    // إعداد رصيد افتتاحي
    Other = 5            // أخرى
}
