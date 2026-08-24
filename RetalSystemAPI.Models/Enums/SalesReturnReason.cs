namespace RetalSystemAPI.Models.Enums;

public enum SalesReturnReason
{
    Defective = 1,      // بضاعة معيبة أو تالفة
    WrongItem = 2,      // صنف خاطئ
    CustomerChange = 3, // تغيير رأي العميل
    Expired = 4,        // منتهي الصلاحية
    Other = 5           // سبب آخر
}
