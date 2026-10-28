using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RetalSystemAPI.Models.DTOs.Sales;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// فئة مساعدة داخلية لمعالجة وتأمين طلبات مبيعات نقاط البيع (POS)، وتوليد بصمة التكرار وحماية إعادة الإرسال.
/// </summary>
internal static class PosSaleRequest
{
    /// <summary>
    /// صياغة رقم فريد لفاتورة نقطة البيع بالاعتماد على مفتاح العميل لمنع توليد أرقام جديدة عند إعادة المحاولة.
    /// </summary>
    /// <param name="clientNumber">رقم الفاتورة أو المعرف القادم من جهاز العميل</param>
    /// <returns>رقم الفاتورة المهيأ ببادئة POS وبصمة حتمية ثابتة</returns>
    internal static string InvoiceNumber(string clientNumber) =>
        // التحقق مما إذا كان الرقم يبدأ بالفعل ببادئة نقاط البيع المعتمدة
        clientNumber.StartsWith("POS-", StringComparison.Ordinal)
            // إذا كان يبدأ بالبادئة نعتمد الرقم كما هو
            ? clientNumber
            // وإلا يتم اقتطاع أول 32 خانة من بصمة الهاش التابعة له لتوليد رقم ثابت لا يتغير عند إعادة الإرسال
            : $"POS-{Hash(clientNumber)[..32]}";

    /// <summary>
    /// توليد بصمة فريدة (Fingerprint) لطلب إنشاء الفاتورة لمطابقة الطلبات المكررة والتأكد من مطابقة البنود والمدفوعات.
    /// </summary>
    /// <param name="dto">بيانات فاتورة المبيعات المطلوب التحقق من بصمتها</param>
    /// <returns>رمز البصمة الرقمية المولد بتقنية SHA-256</returns>
    internal static string Fingerprint(CreateSalesInvoiceDto dto) =>
        // في حال عدم وجود مدفوعات متعددة يتم توليد البصمة بالصيغة القديمة، وإلا نستخدم صيغة الإصدار الثاني المتضمنة للمدفوعات
        dto.Payments is null
            // حساب البصمة القديمة بدون مدفوعات مجزأة
            ? LegacyFingerprint(dto)
            // توليد بصمة الإصدار الثاني المتضمنة تفاصيل الدفع المرتبة
            : Hash(JsonSerializer.Serialize(new
            {
                // رقم إصدار صيغة البصمة
                Version = 2,
                // تضمين بصمة الفاتورة والبنود
                Invoice = LegacyFingerprint(dto),
                // ترتيب وتنسيق مبالغ المدفوعات بدقة لضمان ثبات البصمة
                Payments = dto.Payments.OrderBy(p => p.ClientMovementId).Select(p => new
                {
                    // معرف حركة العميل
                    p.ClientMovementId,
                    // وسيلة السداد
                    p.PaymentMethod,
                    // المبلغ المنسق نصياً لتجنب مشاكل الفواصل
                    Amount = Money(p.Amount),
                    // المبلغ المقدم المنسق
                    Tendered = p.TenderedAmount.HasValue ? Money(p.TenderedAmount.Value) : null
                })
            }));

    /// <summary>
    /// توليد بصمة الفاتورة الأساسية بالاعتماد على الحقول المالية وبنود المنتجات والكميات.
    /// </summary>
    /// <param name="dto">بيانات الفاتورة</param>
    /// <returns>رمز هاش مشفر للبيانات الأساسية للفاتورة</returns>
    private static string LegacyFingerprint(CreateSalesInvoiceDto dto) =>
        // تحويل البيانات المالية والبنود إلى صيغة JSON ثابتة ثم تشفيرها بهاش SHA-256
        Hash(JsonSerializer.Serialize(new
        {
            // معرف الفرع
            dto.BranchId,
            // معرف المستودع أو الصالة
            dto.WarehouseId,
            // معرف العميل
            dto.CustomerId,
            // حالة الفاتورة
            dto.Status,
            // طريقة الدفع
            dto.PaymentMethod,
            // خزينة الدفع
            dto.PaymentSafeId,
            // جلسة الصندوق
            dto.CashSessionId,
            // الملاحظات
            dto.Notes,
            // مبلغ الخصم منسقاً
            Discount = Money(dto.DiscountAmount),
            // مبلغ الضريبة منسقاً
            Tax = Money(dto.TaxAmount),
            // المبلغ المدفوع منسقاً
            Paid = Money(dto.PaidAmount),
            // قائمة البنود مع كمياتها وأسعارها وخصوماتها
            Items = dto.Items.Select(i => new
            {
                // معرف المنتج
                i.ProductId,
                // معرف الباركود
                i.ProductBarCodeId,
                // الكمية
                i.Quantity,
                // سعر الوحدة المنسق
                Price = Money(i.UnitPrice),
                // قيمة الخصم المنسق
                Discount = Money(i.DiscountAmount)
            })
        }));

    /// <summary>
    /// تجهيز نسخة طبق الأصل من كائن إنشاء الفاتورة لعملية الترحيل مع تثبيت رقم الفاتورة ودون تغيير مدخلات العميل.
    /// </summary>
    /// <param name="dto">كائن الفاتورة الأصلي القادم في الطلب</param>
    /// <param name="invoiceNumber">رقم الفاتورة النهائي المعتمد</param>
    /// <returns>نسخة مخصصة لعملية الترحيل</returns>
    internal static CreateSalesInvoiceDto ForPosting(CreateSalesInvoiceDto dto, string invoiceNumber) => new()
    {
        // تعيين رقم الفاتورة النهائي
        InvoiceNumber = invoiceNumber,
        // تاريخ الفاتورة
        InvoiceDate = dto.InvoiceDate,
        // معرف الفرع
        BranchId = dto.BranchId,
        // معرف المستودع
        WarehouseId = dto.WarehouseId,
        // معرف العميل
        CustomerId = dto.CustomerId,
        // حالة الفاتورة
        Status = dto.Status,
        // طريقة الدفع
        PaymentMethod = dto.PaymentMethod,
        // المجموع الفرعي
        SubTotal = dto.SubTotal,
        // إجمالي الخصم
        DiscountAmount = dto.DiscountAmount,
        // إجمالي الضريبة
        TaxAmount = dto.TaxAmount,
        // الإجمالي النهائي
        TotalAmount = dto.TotalAmount,
        // المبلغ المدفوع
        PaidAmount = dto.PaidAmount,
        // خزينة الدفع
        PaymentSafeId = dto.PaymentSafeId,
        // جلسة الصندوق
        CashSessionId = dto.CashSessionId,
        // الملاحظات
        Notes = dto.Notes,
        // قائمة المدفوعات المجزأة
        Payments = dto.Payments,
        // نسخ قائمة البنود بالكامل
        Items = dto.Items.Select(i => new SalesInvoiceItemDto
        {
            ProductId = i.ProductId,
            ProductBarCodeId = i.ProductBarCodeId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            UnitCost = i.UnitCost,
            DiscountAmount = i.DiscountAmount
        }).ToList()
    };

    /// <summary>
    /// تنسيق القيم المالية بصيغة قياسية محايدة للثقافة لضمان تطابق النصوص المشفرة في الهاش.
    /// </summary>
    /// <param name="value">القيمة المالية</param>
    /// <returns>نص رقمي قياسي غير خاضع لاختلافات الفواصل المحلية</returns>
    private static string Money(decimal value) =>
        // تحويل القيمة العشرية باستخدام النمط العام G29 مع الثقافة المحايدة InvariantCulture
        value.ToString("G29", CultureInfo.InvariantCulture);

    /// <summary>
    /// حساب قيمة التشفير الهاش SHA-256 لنص معين وتحويله إلى صيغة سداسية عشرية Hex.
    /// </summary>
    /// <param name="value">النص المراد تشفيره</param>
    /// <returns>الرمز المشفر المكون من 64 حرفاً كبيراً</returns>
    private static string Hash(string value) =>
        // تشفير مصفوفة البايتات UTF-8 باستخدام خوارزمية SHA-256 وتحويلها لسلسلة Hex
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
