using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.Safes;
using RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة الخزائن المالية والحسابات المصرفية وأدراج النقدية وتسجيل الحركات والتحويلات المالية بينها.
/// </summary>
public class SafeService : ISafeService
{
    // وحدة العمل للوصول إلى مستودعات البيانات والعمليات المحاسبية
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل كائنات النماذج إلى كائنات DTO والعكس
    private readonly IMapper _mapper;
    // خدمة نقاط البيع والورديات النقدية للتحقق من ارتباطات الأدراج بالورديات
    private readonly IPosCashService _posCashService;
    /// <summary>
    /// باني فئة خدمة الخزائن وحقن وحدة العمل والمحول وخدمة الصندوق.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل</param>
    /// <param name = "mapper">محول الكيانات</param>
    /// <param name = "posCashService">خدمة صندوق ونقاط البيع</param>
    public SafeService(IUnitOfWork unitOfWork, IMapper mapper, IPosCashService posCashService)
    {
        // تعيين وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين محول الكيانات
        _mapper = mapper;
        // تعيين خدمة الصندوق
        _posCashService = posCashService;
    }

    /// <inheritdoc/>
    /// <summary>
    /// جلب بيانات خزينة مالية بواسطة المعرف الفريد مع تفاصيل الفرع والحركات.
    /// </summary>
    public async Task<ServiceResult<SafeResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الخزينة مع تفاصيلها من المستودع
        var safe = await _unitOfWork.Accounting.GetSafeWithDetailsAsync(id, ct);
        // إذا لم توجد الخزينة
        if (safe == null)
            // إرجاع خطأ عدم العثور على الخزينة
            return ServiceResult<SafeResponseDto>.Failure("الخزينة المطلوبة غير موجودة", ErrorCodes.SafeNotFound);
        // تحويل الكيان إلى كائن استجابة DTO وإرجاعه بنجاح
        return ServiceResult<SafeResponseDto>.Success(_mapper.Map<SafeResponseDto>(safe));
    }

    /// <inheritdoc/>
    /// <summary>
    /// استرجاع قائمة بكافة الخزائن المسجلة لدى المستأجر.
    /// </summary>
    public async Task<ServiceResult<IReadOnlyList<SafeSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // جلب كافة الخزائن من قاعدة البيانات
        var safes = await _unitOfWork.Accounting.GetAllSafesAsync(ct);
        // تحويل القائمة لكائنات ملخصات DTO وإرجاعها
        return ServiceResult<IReadOnlyList<SafeSummaryDto>>.Success(_mapper.Map<IReadOnlyList<SafeSummaryDto>>(safes));
    }

    /// <inheritdoc/>
    /// <summary>
    /// استرجاع صفحة مجزأة ومفلترة من ملخصات الخزائن.
    /// </summary>
    public async Task<ServiceResult<PagedResult<SafeSummaryDto>>> GetPagedAsync(Guid? branchId, bool? isActive, string? searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        // تصحيح رقم الصفحة في حال كان أقل من 1
        if (pageNumber < 1)
            pageNumber = 1;
        // تصحيح حجم الصفحة في حال كان أقل من 1
        if (pageSize < 1)
            pageSize = 10;
        // تنفيذ استعلام جلب الخزائن المجزأة من المستودع
        var(items, totalCount) = await _unitOfWork.Accounting.SearchSafesAsync(new SafeQuery { BranchId = branchId, IsActive = isActive, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل الكيانات المقروءة إلى DTO
        var dtos = _mapper.Map<IReadOnlyList<SafeSummaryDto>>(items);
        // إنشاء كائن النتائج المجزأة متضمناً الإحصائيات
        var pagedResult = PagedResult<SafeSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SafeSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    /// <summary>
    /// إنشاء خزينة مالية جديدة وتسجيل رصيدها الافتتاحي كحركة مالية أولى.
    /// </summary>
    public async Task<ServiceResult<SafeResponseDto>> CreateAsync(CreateSafeDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SafeResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من صحة نوع الخزينة
        if (!Enum.IsDefined(dto.Type))
            // إرجاع خطأ عدم صلاحية نوع الخزينة
            return ServiceResult<SafeResponseDto>.Failure("نوع الخزينة غير صالح", ErrorCodes.ValidationError);
        // التحقق من وجود الفرع في النظام
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        // إذا لم يوجد الفرع
        if (!branchExists)
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<SafeResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        // التحقق من عدم تكرار اسم الخزينة في نفس الفرع
        bool nameExists = await _unitOfWork.Accounting.SafeNameExistsAsync(dto.Name.Trim(), dto.BranchId, null, ct);
        // في حال كان الاسم مكرراً
        if (nameExists)
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<SafeResponseDto>.Failure("اسم الخزينة أو الحساب المصرفي مستخدم بالفعل في هذا الفرع", ErrorCodes.SafeNameExists);
        // تحويل كائن DTO إلى كيان الخزينة
        var safe = _mapper.Map<Safe>(dto);
        // تعيين الرصيد الحالي بالرصيد الافتتاحي
        safe.CurrentBalance = dto.OpeningBalance;
        // في حال وجود رصيد افتتاحي أكبر من الصفر
        if (dto.OpeningBalance > 0)
        {
            // توليد رقم حركة تسلسلي فريد للخزينة
            var txNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
            // إضافة حركة القيد الافتتاحي في جدول حركات الخزينة
            safe.Transactions.Add(new SafeTransaction { // رقم الحركة
 TransactionNumber = txNumber, // معرف الخزينة
 SafeId = safe.Id, // اتجاه الحركة وارد
 Direction = TransactionDirection.Inbound, // المبلغ
 Amount = dto.OpeningBalance, // الرصيد بعد الحركة
 BalanceAfter = dto.OpeningBalance, // تاريخ الحركة
 TransactionDate = DateTime.UtcNow, // نوع المستند رصيد افتتاحي
 DocumentType = FinancialDocumentType.OpeningBalance, // وصف الحركة
 Description = "رصيد افتتاحي عند إنشاء الخزينة" });
        }

        // إضافة الخزينة لمستودع البيانات
        await _unitOfWork.Accounting.AddSafeAsync(safe, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب الخزينة المنشأة مع تفاصيلها
        var created = await _unitOfWork.Accounting.GetSafeWithDetailsAsync(safe.Id, ct) ?? safe;
        // إرجاع كائن الاستجابة بنجاح
        return ServiceResult<SafeResponseDto>.Success(_mapper.Map<SafeResponseDto>(created));
    }

    /// <inheritdoc/>
    /// <summary>
    /// تحديث بيانات الخزينة وتدقيق القيود المتعلقة بنقاط البيع والورديات النشطة.
    /// </summary>
    public async Task<ServiceResult<SafeResponseDto>> UpdateAsync(Guid id, UpdateSafeDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SafeResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من صحة نوع الخزينة المحدد
        if (!Enum.IsDefined(dto.Type))
            // إرجاع خطأ عدم صحة النوع
            return ServiceResult<SafeResponseDto>.Failure("نوع الخزينة غير صالح", ErrorCodes.ValidationError);
        // جلب الخزينة من المستودع بالمعرف
        var safe = await _unitOfWork.Accounting.GetSafeByIdAsync(id, ct);
        // في حال عدم العثور عليها
        if (safe == null)
            // إرجاع خطأ عدم وجود الخزينة
            return ServiceResult<SafeResponseDto>.Failure("الخزينة المطلوبة غير موجودة", ErrorCodes.SafeNotFound);
        // التحقق من قيود نقاط البيع والورديات المفتوحة المرتبطة بالدرج
        var safeUpdateValidation = await _posCashService.ValidateSafeUpdateAsync(id, dto.Type, dto.IsActive, ct);
        // إذا فشل التحقق من قيود الصندوق
        if (!safeUpdateValidation.IsSuccess)
            // إرجاع رسالة الخطأ المناسبة
            return ServiceResult<SafeResponseDto>.Failure(safeUpdateValidation.ErrorMessage!, safeUpdateValidation.ErrorCode);
        // التحقق من عدم تكرار الاسم في الفرع عند تعديله
        if (!string.Equals(safe.Name, dto.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            // فحص تكرار الاسم الجديد
            bool nameExists = await _unitOfWork.Accounting.SafeNameExistsAsync(dto.Name.Trim(), safe.BranchId, id, ct);
            // إذا كان مكرراً
            if (nameExists)
                // إرجاع خطأ تكرار الاسم
                return ServiceResult<SafeResponseDto>.Failure("اسم الخزينة أو الحساب المصرفي مستخدم بالفعل في هذا الفرع", ErrorCodes.SafeNameExists);
        }

        // تطبيق التعديلات من كائن DTO إلى الكيان
        _mapper.Map(dto, safe);
        // إشعار المستودع بتحديث الكيان
        _unitOfWork.Accounting.UpdateSafe(safe);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب الخزينة المحدثة بالتفاصيل
        var updated = await _unitOfWork.Accounting.GetSafeWithDetailsAsync(id, ct) ?? safe;
        // إرجاع النتيجة بنجاح
        return ServiceResult<SafeResponseDto>.Success(_mapper.Map<SafeResponseDto>(updated));
    }

    /// <inheritdoc/>
    /// <summary>
    /// تبديل حالة نشاط الخزينة والتحقق من عدم ارتباطها بورديات نقدية مفتوحة أو قيد الاعتماد.
    /// </summary>
    public async Task<ServiceResult<SafeResponseDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الخزينة بالمعرف
        var safe = await _unitOfWork.Accounting.GetSafeByIdAsync(id, ct);
        // إذا لم توجد الخزينة
        if (safe == null)
            // إرجاع خطأ عدم وجودها
            return ServiceResult<SafeResponseDto>.Failure("الخزينة المطلوبة غير موجودة", ErrorCodes.SafeNotFound);
        // في حال كانت الخزينة نشطة ونريد تعطيلها
        if (safe.IsActive)
        {
            // التحقق من عدم ارتباطها بورديات مفتوحة
            var sessionResult = await _posCashService.ResolveDrawerPostingSessionAsync(id, ct);
            // إذا كانت مرتبطة بوردية مفتوحة أو تنتظر الاعتماد
            if (!sessionResult.IsSuccess || sessionResult.Data.HasValue)
                // إرجاع خطأ منع التعطيل
                return ServiceResult<SafeResponseDto>.Failure("لا يمكن تعطيل درج نقود مرتبط بورديّة مفتوحة أو قيد الاعتماد", ErrorCodes.ValidationError);
        }

        // عكس حالة النشاط
        safe.IsActive = !safe.IsActive;
        // تحديث الكيان في المستودع
        _unitOfWork.Accounting.UpdateSafe(safe);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب الخزينة المحدثة
        var updated = await _unitOfWork.Accounting.GetSafeWithDetailsAsync(id, ct) ?? safe;
        // إرجاع النتيجة
        return ServiceResult<SafeResponseDto>.Success(_mapper.Map<SafeResponseDto>(updated));
    }

    /// <inheritdoc/>
    /// <summary>
    /// حذف ناعم للخزينة بشرط عدم وجود أي حركات مالية مسجلة عليها.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الخزينة من المستودع
        var safe = await _unitOfWork.Accounting.GetSafeByIdAsync(id, ct);
        // إذا لم توجد
        if (safe == null)
            // إرجاع خطأ عدم العثور عليها
            return ServiceResult<bool>.Failure("الخزينة المطلوبة غير موجودة", ErrorCodes.SafeNotFound);
        // التحقق من خلو الخزينة من الحركات المالية
        bool hasTransactions = await _unitOfWork.Accounting.SafeHasTransactionsAsync(id, ct);
        // إذا كانت تحتوي على حركات
        if (hasTransactions)
            // إرجاع خطأ منع الحذف لوجود حركات مالية
            return ServiceResult<bool>.Failure("لا يمكن حذف الخزينة لاحتوائها على حركات مالية مسجلة", ErrorCodes.SafeHasTransactions);
        // تطبيق الحذف الناعم على الخزينة
        var posValidation = await _posCashService.ValidateSafeDeleteAsync(id, ct);
        if (!posValidation.IsSuccess)
            return ServiceResult<bool>.Failure(posValidation.ErrorMessage!, posValidation.ErrorCode);

        _unitOfWork.Accounting.SoftDeleteSafe(safe);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نجاح الحذف
        return ServiceResult<bool>.Success(true);
    }

    /// <inheritdoc/>
    /// <summary>
    /// تسجيل حركة مالية يدوية (إيداع أو سحب) في الخزينة وتعديل رصيدها ذرّياً.
    /// </summary>
    public async Task<ServiceResult<SafeTransactionResponseDto>> AddTransactionAsync(CreateSafeTransactionDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SafeTransactionResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من اتجاه الحركة
        if (!Enum.IsDefined(dto.Direction))
            // إرجاع خطأ عدم صحة الاتجاه
            return ServiceResult<SafeTransactionResponseDto>.Failure("اتجاه الحركة غير صالح", ErrorCodes.ValidationError);
        // جلب الخزينة بالمعرف
        var safe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.SafeId, ct);
        // إذا لم توجد الخزينة
        if (safe == null)
            // إرجاع خطأ عدم العثور على الخزينة
            return ServiceResult<SafeTransactionResponseDto>.Failure("الخزينة المطلوبة غير موجودة", ErrorCodes.SafeNotFound);
        // التحقق من نشاط الخزينة
        if (!safe.IsActive)
            // إرجاع خطأ أن الخزينة معطلة
            return ServiceResult<SafeTransactionResponseDto>.Failure("الخزينة معطلة حالياً ولا يمكن إجراء حركات عليها", ErrorCodes.SafeInactive);
        // فحص ما إذا كانت الخزينة درج نقود يحتاج وردية مفتوحة
        var drawerSession = await _posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
        // إذا كان هناك مانع تجميد أو تعليق
        if (!drawerSession.IsSuccess)
            // إرجاع خطأ الوردية
            return ServiceResult<SafeTransactionResponseDto>.Failure(drawerSession.ErrorMessage!, drawerSession.ErrorCode);
        // التحقق من كفاية الرصيد في حالة السحب (صادر)
        if (dto.Direction == TransactionDirection.Outbound && safe.CurrentBalance < dto.Amount)
        {
            // إرجاع خطأ عدم كفاية الرصيد
            return ServiceResult<SafeTransactionResponseDto>.Failure($"رصيد الخزينة الحالي ({safe.CurrentBalance:N2}) غير كافٍ لإتمام عملية السحب بمبلغ ({dto.Amount:N2})", ErrorCodes.InsufficientSafeBalance);
        }

        // تحديث رصيد الخزينة بحسب اتجاه الحركة
        if (dto.Direction == TransactionDirection.Inbound)
            // إضافة المبلغ في حالة الإيداع
            safe.CurrentBalance += dto.Amount;
        else
            // خصم المبلغ في حالة السحب
            safe.CurrentBalance -= dto.Amount;
        // توليد رقم حركة تسلسلي
        var txNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
        // تحويل كائن DTO إلى كيان الحركة
        var transaction = _mapper.Map<SafeTransaction>(dto);
        // تعيين رقم الحركة
        transaction.TransactionNumber = txNumber;
        // تعيين الرصيد بعد الحركة
        transaction.BalanceAfter = safe.CurrentBalance;
        // تعيين تاريخ الحركة
        transaction.TransactionDate = DateTime.UtcNow;
        // تعيين نوع المستند كتسوية
        transaction.DocumentType = FinancialDocumentType.Adjustment;
        // تعيين معرف الوردية إن وجد
        transaction.CashSessionId = drawerSession.Data;
        // إضافة الحركة لقاعدة البيانات
        await _unitOfWork.Accounting.AddSafeTransactionAsync(transaction, ct);
        // تحديث كائن الخزينة
        _unitOfWork.Accounting.UpdateSafe(safe);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);
        // ربط الخزينة بالحركة
        transaction.Safe = safe;
        // إرجاع كائن الاستجابة بنجاح
        return ServiceResult<SafeTransactionResponseDto>.Success(_mapper.Map<SafeTransactionResponseDto>(transaction));
    }

    /// <inheritdoc/>
    /// <summary>
    /// تحويل أموال بين خزينتين متزامنتين داخل معاملة ذرية تضمن ثبات الأرصدة وتوازن الطرفين.
    /// </summary>
    public async Task<ServiceResult<bool>> TransferFundsAsync(TransferFundsDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<bool>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من اختلاف الخزينتين
        if (dto.FromSafeId == dto.ToSafeId)
            // إرجاع خطأ منع التحويل لنفس الخزينة
            return ServiceResult<bool>.Failure("لا يمكن التحويل المالي بين نفس الخزينة", ErrorCodes.SafeSameTransfer);
        // جلب الخزينة المصدر
        var fromSafe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.FromSafeId, ct);
        // إذا لم توجد الخزينة المصدر
        if (fromSafe == null)
            // إرجاع خطأ عدم وجود الخزينة المصدر
            return ServiceResult<bool>.Failure("الخزينة المصدر غير موجودة", ErrorCodes.SafeNotFound);
        // جلب الخزينة الوجهة
        var toSafe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.ToSafeId, ct);
        // إذا لم توجد الخزينة الوجهة
        if (toSafe == null)
            // إرجاع خطأ عدم وجود الخزينة الوجهة
            return ServiceResult<bool>.Failure("الخزينة المستلمة غير موجودة", ErrorCodes.SafeNotFound);
        // التحقق من نشاط الخزينتين
        if (!fromSafe.IsActive || !toSafe.IsActive)
            // إرجاع خطأ تعطيل إحدى الخزائن
            return ServiceResult<bool>.Failure("إحدى الخزائن معطلة حالياً ولا يمكن إجراء التحويل", ErrorCodes.SafeInactive);
        // فحص وردية الخزينة المصدر
        var fromSession = await _posCashService.ResolveDrawerPostingSessionAsync(fromSafe.Id, ct);
        // في حال وجود مشكلة في وردية المصدر
        if (!fromSession.IsSuccess)
            return ServiceResult<bool>.Failure(fromSession.ErrorMessage!, fromSession.ErrorCode);
        // فحص وردية الخزينة الوجهة
        var toSession = await _posCashService.ResolveDrawerPostingSessionAsync(toSafe.Id, ct);
        // في حال وجود مشكلة في وردية الوجهة
        if (!toSession.IsSuccess)
            return ServiceResult<bool>.Failure(toSession.ErrorMessage!, toSession.ErrorCode);
        // التحقق من كفاية رصيد الخزينة المصدر
        if (fromSafe.CurrentBalance < dto.Amount)
        {
            // إرجاع خطأ عدم كفاية الرصيد
            return ServiceResult<bool>.Failure($"رصيد الخزينة المصدر ({fromSafe.CurrentBalance:N2}) غير كافٍ لتحويل مبلغ ({dto.Amount:N2})", ErrorCodes.InsufficientSafeBalance);
        }

        // بدء معاملة مصرفية ذرية لضمان توازن القيدين
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // خصم المبلغ من الخزينة المصدر
            fromSafe.CurrentBalance -= dto.Amount;
            // إضافة المبلغ إلى الخزينة الوجهة
            toSafe.CurrentBalance += dto.Amount;
            // توليد رقم حركة للحركة الصادرة
            var outTxNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
            // تحديد بيان التحويل
            var transferDesc = !string.IsNullOrWhiteSpace(dto.Description) ? dto.Description : (!string.IsNullOrWhiteSpace(dto.Notes) ? dto.Notes : "تحويل بين خزائن");
            // إنشاء كائن الحركة الصادرة
            var outTx = new SafeTransaction
            {
                TransactionNumber = outTxNumber,
                SafeId = fromSafe.Id,
                Direction = TransactionDirection.Outbound,
                Amount = dto.Amount,
                BalanceAfter = fromSafe.CurrentBalance,
                TransactionDate = DateTime.UtcNow,
                DocumentType = FinancialDocumentType.SafeTransfer,
                DocumentId = toSafe.Id,
                Description = $"تحويل مالي صادر إلى [{toSafe.Name}]: {transferDesc}",
                Notes = dto.Notes
            };
            // ربط الوردية للحركة الصادرة
            outTx.CashSessionId = fromSession.Data;
            // إضافة الحركة الصادرة
            await _unitOfWork.Accounting.AddSafeTransactionAsync(outTx, ct);
            // توليد رقم حركة للحركة الواردة
            var inTxNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
            // إنشاء كائن الحركة الواردة
            var inTx = new SafeTransaction
            {
                TransactionNumber = inTxNumber,
                SafeId = toSafe.Id,
                Direction = TransactionDirection.Inbound,
                Amount = dto.Amount,
                BalanceAfter = toSafe.CurrentBalance,
                TransactionDate = DateTime.UtcNow,
                DocumentType = FinancialDocumentType.SafeTransfer,
                DocumentId = fromSafe.Id,
                Description = $"تحويل مالي وارد من [{fromSafe.Name}]: {transferDesc}",
                Notes = dto.Notes
            };
            // ربط الوردية للحركة الواردة
            inTx.CashSessionId = toSession.Data;
            // إضافة الحركة الواردة
            await _unitOfWork.Accounting.AddSafeTransactionAsync(inTx, ct);
            // تحديث الخزينة المصدر في وحدة العمل
            _unitOfWork.Accounting.UpdateSafe(fromSafe);
            // تحديث الخزينة المستلمة في وحدة العمل
            _unitOfWork.Accounting.UpdateSafe(toSafe);
            // حفظ التغييرات
            await _unitOfWork.SaveChangesAsync(ct);
            // تأكيد ونجاح المعاملة الذرية
            await _unitOfWork.CommitTransactionAsync(ct);
            // إرجاع نجاح التحويل
            return ServiceResult<bool>.Success(true);
        }
        catch
        {
            // التراجع عن المعاملة في حال حدوث أي خطأ
            await _unitOfWork.RollbackTransactionAsync(ct);
            // إعادة إلقاء الاستثناء للمعالجة العامة
            throw;
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// استرجاع سجل الحركات النقدية مع الفلترة حسب الخزينة والتواريخ ونوع الحركة والتقسيم لصفحات.
    /// </summary>
    public async Task<ServiceResult<PagedResult<SafeTransactionResponseDto>>> GetTransactionsAsync(SafeTransactionFilterDto filter, CancellationToken ct = default)
    {
        // التحقق من رقم الصفحة
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        // التحقق من حجم الصفحة
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;
        // تنفيذ استعلام جلب الحركات المجزأة
        var(items, totalCount) = await _unitOfWork.Accounting.SearchSafeTransactionsAsync(new SafeTransactionQuery { SafeId = filter.SafeId, FromDate = filter.FromDate, ToDate = filter.ToDate, Direction = filter.Direction, DocumentType = filter.DocumentType, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل الكيانات لكائنات استجابة DTO
        var dtos = _mapper.Map<IReadOnlyList<SafeTransactionResponseDto>>(items);
        // بناء نتيجة التقسيم لصفحات
        var pagedResult = PagedResult<SafeTransactionResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SafeTransactionResponseDto>>.Success(pagedResult);
    }
}
