using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;

/// <summary>
/// تنفيذ خدمة الترحيل المالي لإنشاء القيود وحركات الخزائن وتحديث أرصدة العملاء والموردين داخل معاملة وحدة العمل الحالية.
/// </summary>
/// <param name="unitOfWork">وحدة العمل لإدارة العمليات ومستودعات البيانات داخل نفس المعاملة</param>
public sealed class FinancialPostingService(IUnitOfWork unitOfWork, IPosCashService posCashService) : IFinancialPostingService
{
    /// <inheritdoc />
    /// <summary>
    /// التحقق من صلاحية الخزينة للفرع وكفاية رصيدها للحركة المالية المطلوبة.
    /// </summary>
    public async Task<ServiceResult<Safe?>> ResolveSafeAsync(
        Guid branchId, Guid? safeId, decimal amount, bool outbound, CancellationToken ct = default)
    {
        // إذا كان المبلغ صفراً أو سالباً، فلا حاجة لمطابقة خزينة
        if (amount <= 0)
            // إرجاع نجاح دون تحديد خزينة
            return ServiceResult<Safe?>.Success(null);

        // متغير لتخزين كائن الخزينة المستهدفة
        Safe? safe;
        // التحقق مما إذا كان معرف الخزينة مرسلاً في الطلب
        if (safeId.HasValue)
        {
            // جلب الخزينة المحددة من قاعدة البيانات
            safe = await unitOfWork.Accounting.GetSafeByIdAsync(safeId.Value, ct);
        }
        else
        {
            // في حال عدم التحديد، جلب الخزائن النشطة في الفرع لاختيار الخزينة الافتراضية
            var candidates = (await unitOfWork.Accounting.GetAllSafesAsync(ct))
                // تصفية الخزائن التابعة للفرع والنشطة فقط
                .Where(s => s.BranchId == branchId && s.IsActive)
                // أخذ أول نتيجتين للتأكد من وجود خزينة واحدة فقط
                .Take(2)
                // تحويل النتيجة لقائمة
                .ToList();

            // إذا لم توجد خزينة واحدة فريدة فعالة في الفرع
            if (candidates.Count != 1)
            {
                // إرجاع خطأ بضرورة تحديد الخزينة لعدم وجود خزينة وحيدة
                return ServiceResult<Safe?>.Failure(
                    "يجب تحديد الخزينة للحركة المالية؛ لا توجد خزينة فعالة وحيدة في الفرع.",
                    ErrorCodes.ValidationError);
            }

            // تعيين الخزينة الوحيدة المتاحة في الفرع
            safe = candidates[0];
        }

        // التحقق من وجود الخزينة وتبعيتها للفرع المحدد
        if (safe is null || safe.BranchId != branchId)
            // إرجاع خطأ عدم وجود الخزينة في الفرع
            return ServiceResult<Safe?>.Failure("الخزينة المحددة غير موجودة في الفرع.", ErrorCodes.SafeNotFound);
        // التحقق من حالة نشاط الخزينة
        if (!safe.IsActive)
            // إرجاع خطأ أن الخزينة معطلة
            return ServiceResult<Safe?>.Failure("الخزينة المحددة معطلة حالياً.", ErrorCodes.SafeInactive);
        // التحقق من كفاية الرصيد في الحركات الصادرة
        if (outbound && safe.CurrentBalance < amount)
            // إرجاع خطأ عدم كفاية الرصيد للحركة الصادرة
            return ServiceResult<Safe?>.Failure(
                $"رصيد الخزينة الحالي ({safe.CurrentBalance:N2}) غير كافٍ للحركة المطلوبة ({amount:N2}).",
                ErrorCodes.InsufficientSafeBalance);

        if (safe.Type == SafeType.CashDrawer)
        {
            var session = await posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
            if (!session.IsSuccess)
                return ServiceResult<Safe?>.Failure(session.ErrorMessage!, session.ErrorCode);
        }

        // إرجاع الخزينة المطابقة بنجاح
        return ServiceResult<Safe?>.Success(safe);
    }

    /// <inheritdoc />
    /// <summary>
    /// ترحيل حركة مالية على حساب العميل وتحديث رصيده وسجل معاملاته.
    /// </summary>
    public async Task PostCustomerBalanceAsync(
        Customer customer, decimal balanceDelta, CustomerTransactionType transactionType,
        Guid branchId, FinancialDocumentType documentType, Guid documentId, DateTime date,
        PaymentMethod? paymentMethod, string description, CancellationToken ct = default)
    {
        // إذا لم يكن هناك تغير في الرصيد نتخطى الترحيل
        if (balanceDelta == 0) return;

        // تعديل رصيد العميل الحالي بالقيمة المحددة
        customer.CurrentBalance += balanceDelta;
        // إضافة سجل حركة جديد في دفتر أستاذ العميل
        await unitOfWork.Customers.AddTransactionAsync(new CustomerTransaction
        {
            // تعيين معرف العميل
            CustomerId = customer.Id,
            // تعيين نوع المعاملة
            TransactionType = transactionType,
            // تعيين مبلغ المدين (إذا كان التغير موجباً)
            DebitAmount = balanceDelta > 0 ? balanceDelta : 0,
            // تعيين مبلغ الدائن (إذا كان التغير سالباً)
            CreditAmount = balanceDelta < 0 ? -balanceDelta : 0,
            // حفظ الرصيد بعد الحركة
            BalanceAfter = customer.CurrentBalance,
            // تاريخ المعاملة
            TransactionDate = date,
            // معرف الفرع
            BranchId = branchId,
            // نوع المستند المالي المرتبط
            DocumentType = documentType,
            // معرف المستند المالي
            DocumentId = documentId,
            // طريقة الدفع
            PaymentMethod = paymentMethod,
            // وصف وبيان المعاملة
            Description = description
        }, ct);
        // تحديث كائن العميل في وحدة العمل
        unitOfWork.Customers.Update(customer);
    }

    /// <inheritdoc />
    /// <summary>
    /// ترحيل حركة مالية على حساب المورد وتحديث رصيده وسجل معاملاته.
    /// </summary>
    public async Task PostSupplierBalanceAsync(
        Supplier supplier, decimal balanceDelta, SupplierTransactionType transactionType,
        Guid branchId, FinancialDocumentType documentType, Guid documentId, DateTime date,
        PaymentMethod? paymentMethod, string description, CancellationToken ct = default)
    {
        // إذا لم يكن هناك تغير في الرصيد نتخطى الترحيل
        if (balanceDelta == 0) return;

        // تعديل رصيد المورد الحالي بالقيمة المحددة
        supplier.CurrentBalance += balanceDelta;
        // إضافة سجل حركة جديد في دفتر أستاذ المورد
        await unitOfWork.Suppliers.AddTransactionAsync(new SupplierTransaction
        {
            // تعيين معرف المورد
            SupplierId = supplier.Id,
            // تعيين نوع المعاملة
            TransactionType = transactionType,
            // تعيين مبلغ المدين (إذا كان التغير سالباً)
            DebitAmount = balanceDelta < 0 ? -balanceDelta : 0,
            // تعيين مبلغ الدائن (إذا كان التغير موجباً)
            CreditAmount = balanceDelta > 0 ? balanceDelta : 0,
            // حفظ الرصيد بعد الحركة
            BalanceAfter = supplier.CurrentBalance,
            // تاريخ المعاملة
            TransactionDate = date,
            // معرف الفرع
            BranchId = branchId,
            // نوع المستند المالي المرتبط
            DocumentType = documentType,
            // معرف المستند المالي
            DocumentId = documentId,
            // طريقة الدفع
            PaymentMethod = paymentMethod,
            // بيان ووصف المعاملة
            Description = description
        }, ct);
        // تحديث كائن المورد في وحدة العمل
        unitOfWork.Suppliers.Update(supplier);
    }

    /// <inheritdoc />
    /// <summary>
    /// ترحيل حركة نقدية على الخزينة وتعديل رصيدها وتسجيل قيد الحركة.
    /// </summary>
    public async Task PostSafeBalanceAsync(
        Safe safe, decimal balanceDelta, FinancialDocumentType documentType,
        Guid documentId, DateTime date, string description, CancellationToken ct = default,
        Guid? cashSessionId = null, string? idempotencyKey = null)
    {
        // إذا لم يكن هناك تغير في الرصيد نتخطى الترحيل
        if (balanceDelta == 0) return;
        // التحقق من عدم تحول الرصيد إلى سالب
        if (safe.CurrentBalance + balanceDelta < 0)
            // إلقاء استثناء يمنع جعل الرصيد سالباً
            throw new InvalidOperationException("رصيد الخزينة غير كافٍ لإتمام الحركة المالية.");

        // تعديل رصيد الخزينة الحالي
        if (safe.Type == SafeType.CashDrawer)
        {
            var session = await posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
            if (!session.IsSuccess)
                throw new InvalidOperationException(session.ErrorMessage);
            if (cashSessionId.HasValue && session.Data != cashSessionId)
                throw new InvalidOperationException("وردية درج النقود لم تعد متاحة للترحيل");
            cashSessionId = session.Data;
        }

        safe.CurrentBalance += balanceDelta;
        // إنشاء وإضافة حركة الخزينة في دفتر اليومية النقدية
        await unitOfWork.Accounting.AddSafeTransactionAsync(new SafeTransaction
        {
            // توليد رقم تسلسلي جديد للحركة المالية
            TransactionNumber = await unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct),
            // تعيين معرف الخزينة
            SafeId = safe.Id,
            // تحديد اتجاه الحركة (واردة أو صادرة)
            Direction = balanceDelta > 0 ? TransactionDirection.Inbound : TransactionDirection.Outbound,
            // القيمة المطلقة للمبلغ
            Amount = Math.Abs(balanceDelta),
            // تسجيل الرصيد بعد الحركة
            BalanceAfter = safe.CurrentBalance,
            // تاريخ الحركة
            TransactionDate = date,
            // نوع المستند المالي
            DocumentType = documentType,
            // معرف المستند
            DocumentId = documentId,
            // وصف وبيان الحركة
            Description = description,
            // معرف وردية الصندوق المرتبطة
            CashSessionId = cashSessionId,
            // مفتاح منع تكرار المعاملة
            IdempotencyKey = idempotencyKey
        }, ct);
        // تحديث كائن الخزينة في وحدة العمل
        unitOfWork.Accounting.UpdateSafe(safe);
    }
}
