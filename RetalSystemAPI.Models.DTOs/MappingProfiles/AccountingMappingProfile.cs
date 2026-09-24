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
        CreateMap<Safe, SafeResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == SafeType.Bank ? "حساب بنكي" :
                src.Type == SafeType.CashBox ? "صندوق نقدي" :
                src.Type == SafeType.CashDrawer ? "درج نقود كاشير" : src.Type.ToString()));

        CreateMap<Safe, SafeSummaryDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == SafeType.Bank ? "حساب بنكي" :
                src.Type == SafeType.CashBox ? "صندوق نقدي" :
                src.Type == SafeType.CashDrawer ? "درج نقود كاشير" : src.Type.ToString()));

        CreateMap<CreateSafeDto, Safe>()
            .ForMember(dest => dest.CurrentBalance, opt => opt.MapFrom(src => src.OpeningBalance));

        CreateMap<UpdateSafeDto, Safe>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.OpeningBalance, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore());


        // ── 2. حركات الخزائن (Safe Transactions) ─────────────────────────────
        CreateMap<SafeTransaction, SafeTransactionResponseDto>()
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : string.Empty))
            .ForMember(dest => dest.DirectionName, opt => opt.MapFrom(src =>
                src.Direction == TransactionDirection.Inbound ? "وارد / إيداع" : "صادر / سحب"))
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

        CreateMap<CreateSafeTransactionDto, SafeTransaction>()
            .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => FinancialDocumentType.Adjustment));


        // ── 3. تصنيفات المصروفات (Expense Categories) ──────────────────────
        CreateMap<ExpenseCategory, ExpenseCategoryResponseDto>()
            .ForMember(dest => dest.ExpensesCount, opt => opt.MapFrom(src => src.Expenses != null ? src.Expenses.Count : 0));

        CreateMap<CreateExpenseCategoryDto, ExpenseCategory>();

        CreateMap<UpdateExpenseCategoryDto, ExpenseCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());


        // ── 4. سندات المصروفات (Expenses) ──────────────────────────────────
        CreateMap<Expense, ExpenseResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.ExpenseCategoryName, opt => opt.MapFrom(src => src.ExpenseCategory != null ? src.ExpenseCategory.Name : string.Empty))
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : null))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل بنكي" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()));

        CreateMap<Expense, ExpenseSummaryDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.ExpenseCategoryName, opt => opt.MapFrom(src => src.ExpenseCategory != null ? src.ExpenseCategory.Name : string.Empty))
            .ForMember(dest => dest.SafeName, opt => opt.MapFrom(src => src.Safe != null ? src.Safe.Name : null))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل بنكي" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()));

        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(dest => dest.ExpenseNumber, opt => opt.Ignore());

        CreateMap<UpdateExpenseDto, Expense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ExpenseNumber, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Amount, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            .ForMember(dest => dest.SafeId, opt => opt.Ignore());
    }
}
