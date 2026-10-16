using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب تفاصيل فاتورة مشتريات محددة مع المورد، الفرع، المستودع، البنود المجمعة، وتفصيلات النكهات (Breakdowns).
/// </summary>
internal sealed class PurchaseInvoiceWithDetailsSpec : BaseSpecification<PurchaseInvoice>
{
    /// <summary>
    /// تهيئة مواصفة فاتورة المشتريات بالمعرف وتضمين كافة العلاقات التفصيلية وتفاصيل النكهات.
    /// </summary>
    /// <param name = "id">معرف فاتورة المشتريات</param>
    public PurchaseInvoiceWithDetailsSpec(Guid id) : base(p => p.Id == id)
    {
        // تضمين بيانات المورد المصدر للفاتورة
        AddInclude(p => p.Supplier);
        // تضمين بيانات الفرع التابع له الفاتورة
        AddInclude(p => p.Branch);
        // تضمين بيانات المستودع المستلم للبضاعة
        AddInclude(p => p.Warehouse);
        // تضمين أمر الشراء المرتبط في حال وجوده
        AddInclude(p => p.PurchaseOrder!);
        // تضمين بنود الفاتورة
        AddInclude(p => p.Items);
        // تضمين بيانات الصنف لكل بند
        AddInclude("Items.Product");
        // تضمين بيانات الباركود الرئيسي لكل بند
        AddInclude("Items.ProductBarCode");
        // تضمين تفريعات ونكهات البنود
        AddInclude("Items.Breakdowns");
        // تضمين باركود كل نكهة أو تفريعة للبند
        AddInclude("Items.Breakdowns.ProductBarCode");
    }
}
