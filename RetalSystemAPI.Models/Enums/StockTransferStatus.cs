namespace RetalSystemAPI.Models.Enums;

public enum StockTransferStatus
{
    Draft = 1,      // مسودة
    Confirmed = 2,  // مؤكدة وجاهزة للتنفيذ
    Completed = 3,  // منفذة ومرحّلة
    Cancelled = 4   // ملغاة
}
