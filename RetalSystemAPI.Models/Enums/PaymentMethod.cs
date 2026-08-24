namespace RetalSystemAPI.Models.Enums;

public enum PaymentMethod
{
    Cash = 1,          // نقدي
    BankTransfer = 2,  // تحويل مصرفي
    CreditCard = 3,    // بطاقة مصرفية
    Credit = 4,        // آجل على حساب العميل
    Cheque = 5         // صك
}
