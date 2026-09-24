namespace RetalSystemAPI.Models.Enums;

/// <summary>
/// يحدد اتجاه التدفق النقدي لحركة الخزينة أو الحساب البنكي (وارد أو صادر).
/// </summary>
// تعريف تعداد اتجاه حركة الأموال في الخزائن
public enum TransactionDirection
{
    /// <summary>
    /// تدفق نقدي وارد (قبض / إيداع / تحصيل يزيد رصيد الخزينة).
    /// </summary>
    // حركة واردة إلى الخزينة
    Inbound = 1,

    /// <summary>
    /// تدفق نقدي صادر (صرف / سحب / سداد ينقص رصيد الخزينة).
    /// </summary>
    // حركة صادرة من الخزينة
    Outbound = 2
}
