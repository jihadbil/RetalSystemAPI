namespace RetalSystemAPI.Models.Enums;

/// <summary>
/// يمثل أسباب إرجاع البضاعة المشتراة إلى المورد.
/// </summary>
public enum PurchaseReturnReason
{
    /// <summary>بضاعة معيبة أو تالفة</summary>
    Defective = 1,

    /// <summary>غير مطابق للمواصفات أو الطلب</summary>
    WrongSpecification = 2,

    /// <summary>قريب أو منتهي الصلاحية</summary>
    NearExpiryOrExpired = 3,

    /// <summary>فائض عن حاجة المخزون أو متفق على إرجاعه</summary>
    ExcessStock = 4,

    /// <summary>سبب آخر</summary>
    Other = 5
}
