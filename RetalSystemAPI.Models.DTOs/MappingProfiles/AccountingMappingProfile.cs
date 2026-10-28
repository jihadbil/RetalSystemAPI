using System.Linq;
using AutoMapper;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;
using RetalSystemAPI.Models.DTOs.Accounting.Expenses;
using RetalSystemAPI.Models.DTOs.Accounting.Safes;
using RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات النظام المحاسبي والمالي (Accounting Mapping Profile).
/// يحدد قواعد التحويل بين كيانات الخزائن، الحركات النقدية، المصروفات، وتصنيفاتها ونواقل البيانات المقابلة.
/// </summary>
public class AccountingMappingProfile : Profile
{
    /// <summary>
    /// تهيئة قواعد التحويل لنماذج الخزائن، الحركات النقدية، تصنيفات المصروفات، وسندات المصروفات.
    /// </summary>
    public AccountingMappingProfile()
    {
        // ── 1. الخزائن والحسابات المصرفية (Safes) ───────────────────────────
        // تكوين تحويل كيان الخزينة إلى ناقل بيانات الاستجابة المفصل
        CreateMap<Safe, SafeResponseDto>()
            // استخراج اسم الفرع التابعة له الخزينة
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // ترجمة نوع الخزينة التعدادي إلى الاسم المعرب للعرض
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == SafeType.Bank ? "حساب بنكي" :
                src.Type == SafeType.CashBox ? "صندوق نقدي" :
                src.Type == SafeType.CashDrawer ? "درج نقود كاشير" : src.Type.ToString()));

        // تكوين تحويل كيان الخزينة إلى ناقل بيانات الملخص السريع للجداول
        CreateMap<Safe, SafeSummaryDto>()
            // استخراج اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // ترجمة نوع الخزينة
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == SafeType.Bank ? "حساب بنكي" :
                src.Type == SafeType.CashBox ? "صندوق نقدي" :
                src.Type == SafeType.CashDrawer ? "درج نقود كاشير" : src.Type.ToString()));

        // تكوين تحويل ناقل بيانات إنشاء الخزينة إلى كيان الخزينة
        CreateMap<CreateSafeDto, Safe>(MemberList.None)
            // تعيين الرصيد الحالي الأولي مساوياً للرصيد الافتتاحي
            .ForMember(dest => dest.CurrentBalance, opt => opt.MapFrom(src => src.OpeningBalance));

        // تكوين تحويل ناقل بيانات تعديل الخزينة إلى كيان الخزينة
        CreateMap<UpdateSafeDto, Safe>(MemberList.None)
            // استبعاد المعرف الأساسي لمنع التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // استبعاد الفرع لمنع نقل الخزينة بين الفروع
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            // استبعاد الرصيد الافتتاحي للحفاظ على سلامة القيود التاريخية
            .ForMember(dest => dest.OpeningBalance, opt => opt.Ignore())
            // استبعاد الرصيد الحالي حيث يعدل فقط عبر الحركات المحاسبية
            .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore());


        // ── 2. حركات الخزائن (Safe Transactions) ─────────────────────────────
        // تكوين تحويل كيان حركة الخزينة إلى ناقل بيانات الاستجابة
        CreateMap<SafeTransaction, SafeTransactionResponseDto>()
            // استخراج اسم الخزينة
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : string.Empty))
            // ترجمة اتجاه الحركة إلى النص المعرب (وارد / صادر)
            .ForMember(dest => dest.DirectionName, opt => opt.MapFrom(src =>
                src.Direction == TransactionDirection.Inbound ? "وارد / إيداع" : "صادر / سحب"))
            // ترجمة نوع المستند المالي المصدر للحركة
            .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src =>
                src.DocumentType == FinancialDocumentType.SalesInvoice ? "فاتورة مبيعات" :
                src.DocumentType == FinancialDocumentType.SalesReturn ? "مرتجع مبيعات" :
                src.DocumentType == FinancialDocumentType.PurchaseInvoice ? "فاتورة مشتريات" :
                src.DocumentType == FinancialDocumentType.PurchaseReturn ? "مرتجع مشتريات" :
                src.DocumentType == FinancialDocumentType.Expense ? "سند مصروف" :
                src.DocumentType == FinancialDocumentType.CustomerPayment ? "سداد عميل" :
                src.DocumentType == FinancialDocumentType.SupplierPayment ? "سداد مورد" :
                src.DocumentType == FinancialDocumentType.Adjustment ? "تسوية مالية" :
                src.DocumentType == FinancialDocumentType.OpeningBalance ? "رصيد افتتاحي" : src.DocumentType.ToString()));

        // تكوين تحويل ناقل بيانات تسجيل حركة الخزينة اليدوية إلى كيان الحركة
        CreateMap<CreateSafeTransactionDto, SafeTransaction>(MemberList.None)
            // تعيين نوع المستند افتراضياً كـ تسوية مالية يدوية
            .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => FinancialDocumentType.Adjustment));


        // ── 3. تصنيفات المصروفات (Expense Categories) ──────────────────────
        // تكوين تحويل كيان تصنيف المصروف إلى ناقل بيانات الاستجابة
        CreateMap<ExpenseCategory, ExpenseCategoryResponseDto>()
            // احتساب إجمالي عدد سندات المصروف المسجلة تحت هذا التصنيف
            .ForMember(dest => dest.ExpensesCount, opt => opt.MapFrom(src => src.Expenses != null ? src.Expenses.Count : 0));

        // تكوين تحويل ناقل بيانات إنشاء تصنيف المصروف إلى الكيان
        CreateMap<CreateExpenseCategoryDto, ExpenseCategory>(MemberList.None);

        // تكوين تحويل ناقل بيانات تعديل تصنيف المصروف إلى الكيان
        CreateMap<UpdateExpenseCategoryDto, ExpenseCategory>(MemberList.None)
            // استبعاد المعرف الأساسي لمنع التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore());


        // ── 4. سندات المصروفات (Expenses) ──────────────────────────────────
        // تكوين تحويل كيان سند المصروف إلى ناقل بيانات الاستجابة المفصل
        CreateMap<Expense, ExpenseResponseDto>()
            // استخراج اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // استخراج اسم تصنيف المصروف
            .ForMember(dest => dest.ExpenseCategoryName, opt => opt.MapFrom(src => src.ExpenseCategory != null ? src.ExpenseCategory.Name : string.Empty))
            // استخراج اسم الخزينة المصروف منها
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : null))
            // ترجمة طريقة السداد إلى النص العربي المعادل
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل بنكي" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()));

        // تكوين تحويل كيان سند المصروف إلى ناقل بيانات الملخص السريع للجداول
        CreateMap<Expense, ExpenseSummaryDto>()
            // استخراج اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // استخراج اسم تصنيف المصروف
            .ForMember(dest => dest.ExpenseCategoryName, opt => opt.MapFrom(src => src.ExpenseCategory != null ? src.ExpenseCategory.Name : string.Empty))
            // استخراج اسم الخزينة
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : null))
            // ترجمة طريقة السداد
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل بنكي" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()));

        // تكوين تحويل ناقل بيانات تسجيل المصروف إلى كيان المصروف
        CreateMap<CreateExpenseDto, Expense>(MemberList.None)
            // استبعاد رقم المصروف حيث يتم توليده تسلسلياً بواسطة خدمة الترقيم
            .ForMember(dest => dest.ExpenseNumber, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات تعديل المصروف إلى كيان المصروف
        CreateMap<UpdateExpenseDto, Expense>(MemberList.None)
            // استبعاد المعرف الأساسي لمنع التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // استبعاد رقم السند لمنع تغييره
            .ForMember(dest => dest.ExpenseNumber, opt => opt.Ignore())
            // استبعاد الفرع لمنع تحويل السند
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            // استبعاد المبلغ حيث لا يمكن تعديل قيمة المصروف بعد ترحيله دفترياً
            .ForMember(dest => dest.Amount, opt => opt.Ignore())
            // استبعاد طريقة السداد
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            // استبعاد الخزينة
            .ForMember(dest => dest.SafeId, opt => opt.Ignore());
    }
}
