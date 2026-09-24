using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.Expenses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة سندات المصروفات التشغيلية والخصم المباشر من الخزائن وأدراج النقدية وعكس الحركات عند الإلغاء.
/// </summary>
public class ExpenseService : IExpenseService
{
    // وحدة العمل للوصول للمستودعات وإدارة المعاملات
    private readonly IUnitOfWork _unitOfWork;

    // محول الكيانات لتحويل النماذج إلى كائنات نقل البيانات DTO
    private readonly IMapper _mapper;

    // خدمة الصندوق ونقاط البيع لمطابقة جلسات الأدراج النقدية
    private readonly IPosCashService _posCashService;

    /// <summary>
    /// تهيئة خدمة سندات المصروفات وحقن وحدة العمل والمحول وخدمة الصندوق.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل</param>
    /// <param name="mapper">محول الكيانات</param>
    /// <param name="posCashService">خدمة صندوق ونقاط البيع</param>
    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper, IPosCashService posCashService)
    {
        // تعيين وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين محول الكيانات
        _mapper = mapper;
        // تعيين خدمة الصندوق
        _posCashService = posCashService;
    }

    /// <inheritdoc />
    /// <summary>
    /// جلب بيانات سند مصروف محدد بواسطة المعرف مع تفاصيل التصنيف والخزينة المرتبطة.
    /// </summary>
    public async Task<ServiceResult<ExpenseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سند المصروف مع بياناته المفصلة
        var expense = await _unitOfWork.Accounting.GetExpenseWithDetailsAsync(id, ct);
        // إذا لم يتم العثور على السند
        if (expense == null)
            // إرجاع خطأ عدم وجود السند
            return ServiceResult<ExpenseResponseDto>.Failure("سند المصروف غير موجود", ErrorCodes.ExpenseNotFound);

        // تحويل الكيان إلى DTO وإرجاعه بنجاح
        return ServiceResult<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(expense));
    }

    /// <inheritdoc />
    /// <summary>
    /// جلب صفحة سندات مصروفات مجزأة مع الفلترة حسب معايير الفرع، التصنيف، الخزينة، التواريخ وطريقة الدفع.
    /// </summary>
    public async Task<ServiceResult<PagedResult<ExpenseSummaryDto>>> GetPagedAsync(
        ExpenseFilterDto filter, CancellationToken ct = default)
    {
        // التحقق من رقم الصفحة
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        // التحقق من حجم الصفحة
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        // تنفيذ استعلام جلب المصروفات المجزأة من المستودع
        var (items, totalCount) = await _unitOfWork.Accounting.GetPagedExpensesAsync(
            filter.BranchId,
            filter.ExpenseCategoryId,
            filter.SafeId,
            filter.PaymentMethod,
            filter.FromDate,
            filter.ToDate,
            filter.SearchTerm,
            pageNumber,
            pageSize,
            ct);

        // تحويل الكيانات لملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<ExpenseSummaryDto>>(items);
        // بناء كائن النتائج المجزأة
        var pagedResult = PagedResult<ExpenseSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<ExpenseSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    /// <summary>
    /// إنشاء سند مصروف تشغيلي جديد وخصم قيمته من رصيد الخزينة أو الدرج وتسجيل حركة الصرف ذرّياً.
    /// </summary>
    public async Task<ServiceResult<ExpenseResponseDto>> CreateAsync(CreateExpenseDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة طريقة الدفع وأنها ليست آجلة (المصروف لا يكون آجلاً)
        if (!Enum.IsDefined(dto.PaymentMethod) || dto.PaymentMethod == PaymentMethod.Credit)
            // إرجاع خطأ طريقة الدفع
            return ServiceResult<ExpenseResponseDto>.Failure("طريقة دفع المصروف غير صالحة", ErrorCodes.ValidationError);

        // التحقق من تحديد الخزينة التي تم الصرف منها
        if (!dto.SafeId.HasValue)
            // إرجاع خطأ وجوب تحديد الخزينة
            return ServiceResult<ExpenseResponseDto>.Failure("يجب تحديد الخزينة أو الحساب الذي صُرف منه المبلغ", ErrorCodes.ValidationError);

        // 1. التحقق من وجود الفرع المحدد ونشاطه
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        // إذا لم يوجد الفرع
        if (!branchExists)
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<ExpenseResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);

        // 2. التحقق من وجود تصنيف المصروف ونشاطه
        var category = await _unitOfWork.Accounting.GetExpenseCategoryByIdAsync(dto.ExpenseCategoryId, ct);
        // إذا لم يوجد التصنيف
        if (category == null)
            // إرجاع خطأ عدم وجود التصنيف
            return ServiceResult<ExpenseResponseDto>.Failure("تصنيف المصروف غير موجود", ErrorCodes.ExpenseCategoryNotFound);

        // إذا كان التصنيف معطلاً
        if (!category.IsActive)
            // إرجاع خطأ تعطيل التصنيف
            return ServiceResult<ExpenseResponseDto>.Failure("تصنيف المصروف معطل حالياً", ErrorCodes.ValidationError);

        // 3. التحقق من الخزينة وكفاية رصيدها
        Safe? safe = null;
        // معرف الوردية النقدية في حال كان الدرج نقطة بيع
        Guid? cashSessionId = null;
        // إذا تم تحديد معرف الخزينة
        if (dto.SafeId.HasValue)
        {
            // جلب الخزينة من المستودع
            safe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.SafeId.Value, ct);
            // إذا لم توجد الخزينة
            if (safe == null)
                // إرجاع خطأ عدم وجود الخزينة
                return ServiceResult<ExpenseResponseDto>.Failure("الخزينة المحددة غير موجودة", ErrorCodes.SafeNotFound);

            // التحقق من نشاط الخزينة
            if (!safe.IsActive)
                // إرجاع خطأ أن الخزينة معطلة
                return ServiceResult<ExpenseResponseDto>.Failure("الخزينة معطلة حالياً ولا يمكن الصرف منها", ErrorCodes.SafeInactive);

            // التحقق من تبعية الخزينة لنفس فرع المصروف
            if (safe.BranchId != dto.BranchId)
                // إرجاع خطأ عدم تطابق الفرع
                return ServiceResult<ExpenseResponseDto>.Failure("الخزينة المحددة لا تتبع فرع سند المصروف", ErrorCodes.ValidationError);

            // التحقق من كفاية رصيد الخزينة لصرف المبلغ
            if (safe.CurrentBalance < dto.Amount)
            {
                // إرجاع خطأ عدم كفاية الرصيد
                return ServiceResult<ExpenseResponseDto>.Failure(
                    $"رصيد الخزينة الحالي ({safe.CurrentBalance:N2}) غير كافٍ لصرف مبلغ ({dto.Amount:N2})",
                    ErrorCodes.InsufficientSafeBalance);
            }

            // فحص وردية الصندوق في حال كانت الخزينة درج نقود
            var sessionResult = await _posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
            // في حال وجود مشكلة بالوردية
            if (!sessionResult.IsSuccess)
                // إرجاع خطأ الوردية
                return ServiceResult<ExpenseResponseDto>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);
            // تعيين معرف الوردية
            cashSessionId = sessionResult.Data;
        }

        // 4. توليد رقم سند مصروف تسلسلي جديد
        var expenseNumber = await _unitOfWork.Accounting.GenerateExpenseNumberAsync(ct);

        // تحويل كائن DTO إلى كيان سند المصروف
        var expense = _mapper.Map<Expense>(dto);
        // تعيين رقم السند المتسلسل
        expense.ExpenseNumber = expenseNumber;

        // 5. الحفظ ضمن معاملة ذرية Transaction لضمان تزامن السند وخصم الخزينة
        if (safe != null)
        {
            // بدء المعاملة الذرية
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                // خصم مبلغ المصروف من رصيد الخزينة
                safe.CurrentBalance -= dto.Amount;

                // إضافة سند المصروف لقاعدة البيانات
                await _unitOfWork.Accounting.AddExpenseAsync(expense, ct);

                // توليد رقم حركة جديد للخزينة
                var txNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
                // إنشاء سجل حركة صرف نقدي في الخزينة
                var safeTx = new SafeTransaction
                {
                    TransactionNumber = txNumber,
                    SafeId = safe.Id,
                    Direction = TransactionDirection.Outbound,
                    Amount = dto.Amount,
                    BalanceAfter = safe.CurrentBalance,
                    TransactionDate = dto.ExpenseDate,
                    DocumentType = FinancialDocumentType.Expense,
                    DocumentId = expense.Id,
                    Description = $"سند مصروف [{expense.ExpenseNumber}] - {dto.Description}",
                    Notes = dto.Notes
                };
                // ربط حركة الصرف بوردية الصندوق
                safeTx.CashSessionId = cashSessionId;

                // إضافة حركة الخزينة
                await _unitOfWork.Accounting.AddSafeTransactionAsync(safeTx, ct);
                // تحديث كائن الخزينة
                _unitOfWork.Accounting.UpdateSafe(safe);

                // حفظ التغييرات
                await _unitOfWork.SaveChangesAsync(ct);
                // تأكيد المعاملة الذرية
                await _unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                // التراجع عن المعاملة في حال حدوث خطأ
                await _unitOfWork.RollbackTransactionAsync(ct);
                // إعادة إلقاء الاستثناء
                throw;
            }
        }
        else
        {
            // في حال عدم وجود خزينة إضافة السند مباشرة
            await _unitOfWork.Accounting.AddExpenseAsync(expense, ct);
            // حفظ التغييرات
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // جلب سند المصروف المنشأ مع كافة تفاصيله
        var created = await _unitOfWork.Accounting.GetExpenseWithDetailsAsync(expense.Id, ct) ?? expense;
        // إرجاع النتيجة بنجاح
        return ServiceResult<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(created));
    }

    /// <inheritdoc />
    /// <summary>
    /// تحديث البيانات الوصفية لسند المصروف (الوصف، الملاحظات، التصنيف) دون المساس بالمبلغ المالي المصروف.
    /// </summary>
    public async Task<ServiceResult<ExpenseResponseDto>> UpdateAsync(Guid id, UpdateExpenseDto dto, CancellationToken ct = default)
    {
        // جلب سند المصروف من المستودع
        var expense = await _unitOfWork.Accounting.GetExpenseByIdAsync(id, ct);
        // إذا لم يوجد السند
        if (expense == null)
            // إرجاع خطأ عدم وجود السند
            return ServiceResult<ExpenseResponseDto>.Failure("سند المصروف غير موجود", ErrorCodes.ExpenseNotFound);

        // التحقق من تصنيف المصروف الجديد إن تم تغييره
        if (expense.ExpenseCategoryId != dto.ExpenseCategoryId)
        {
            // جلب التصنيف الجديد
            var category = await _unitOfWork.Accounting.GetExpenseCategoryByIdAsync(dto.ExpenseCategoryId, ct);
            // إذا لم يوجد التصنيف
            if (category == null)
                // إرجاع خطأ عدم وجود التصنيف
                return ServiceResult<ExpenseResponseDto>.Failure("تصنيف المصروف غير موجود", ErrorCodes.ExpenseCategoryNotFound);

            // إذا كان التصنيف الجديد معطلاً
            if (!category.IsActive)
                // إرجاع خطأ تعطيل التصنيف
                return ServiceResult<ExpenseResponseDto>.Failure("تصنيف المصروف معطل حالياً", ErrorCodes.ValidationError);
        }

        // تطبيق التعديلات الوصفية على الكيان
        _mapper.Map(dto, expense);
        // تحديث الكيان في المستودع
        _unitOfWork.Accounting.UpdateExpense(expense);
        // حفظ التعديلات
        await _unitOfWork.SaveChangesAsync(ct);

        // جلب السند المحدث بالتفاصيل
        var updated = await _unitOfWork.Accounting.GetExpenseWithDetailsAsync(id, ct) ?? expense;
        // إرجاع النتيجة بنجاح
        return ServiceResult<ExpenseResponseDto>.Success(_mapper.Map<ExpenseResponseDto>(updated));
    }

    /// <inheritdoc />
    /// <summary>
    /// حذف ناعم لسند المصروف وعكس حركته المالية بإعادة المبلغ للخزينة المسحوب منها ضمن معاملة ذرية.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سند المصروف بالمعرف
        var expense = await _unitOfWork.Accounting.GetExpenseByIdAsync(id, ct);
        // إذا لم يوجد السند
        if (expense == null)
            // إرجاع خطأ عدم وجود السند
            return ServiceResult<bool>.Failure("سند المصروف غير موجود", ErrorCodes.ExpenseNotFound);

        // إذا كان المصروف مسحوباً من خزينة، نقوم باسترداد المبلغ وإيداعه مجدداً
        if (expense.SafeId.HasValue)
        {
            // جلب الخزينة التي صرف منها المبلغ
            var safe = await _unitOfWork.Accounting.GetSafeByIdAsync(expense.SafeId.Value, ct);
            // إذا وجدت الخزينة
            if (safe != null)
            {
                // فحص وردية الصندوق
                var sessionResult = await _posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
                // إذا وجد مانع في الوردية
                if (!sessionResult.IsSuccess)
                    // إرجاع خطأ الوردية
                    return ServiceResult<bool>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);

                // بدء المعاملة الذرية للاسترداد
                await _unitOfWork.BeginTransactionAsync(ct);
                try
                {
                    // إعادة المبلغ المخصوم إلى رصيد الخزينة
                    safe.CurrentBalance += expense.Amount;

                    // توليد رقم حركة استرداد تسلسلي
                    var txNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
                    // إنشاء حركة عكسية واردة للخزينة
                    var reverseTx = new SafeTransaction
                    {
                        TransactionNumber = txNumber,
                        SafeId = safe.Id,
                        Direction = TransactionDirection.Inbound,
                        Amount = expense.Amount,
                        BalanceAfter = safe.CurrentBalance,
                        TransactionDate = DateTime.UtcNow,
                        DocumentType = FinancialDocumentType.Adjustment,
                        DocumentId = expense.Id,
                        Description = $"استرداد إلغاء سند المصروف [{expense.ExpenseNumber}]"
                    };
                    // ربط الحركة بالوردية
                    reverseTx.CashSessionId = sessionResult.Data;

                    // إضافة الحركة العكسية
                    await _unitOfWork.Accounting.AddSafeTransactionAsync(reverseTx, ct);
                    // تحديث رصيد الخزينة
                    _unitOfWork.Accounting.UpdateSafe(safe);
                    // تطبيق الحذف الناعم على سند المصروف
                    _unitOfWork.Accounting.SoftDeleteExpense(expense);

                    // حفظ التغييرات
                    await _unitOfWork.SaveChangesAsync(ct);
                    // تأكيد المعاملة
                    await _unitOfWork.CommitTransactionAsync(ct);

                    // إرجاع نجاح العملية
                    return ServiceResult<bool>.Success(true);
                }
                catch
                {
                    // التراجع عن المعاملة في حال حدوث خطأ
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    // إعادة إلقاء الاستثناء
                    throw;
                }
            }
        }

        // في حال عدم وجود خزينة مرتبطة تطبيق الحذف الناعم مباشرة
        _unitOfWork.Accounting.SoftDeleteExpense(expense);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نجاح الحذف
        return ServiceResult<bool>.Success(true);
    }
}
