using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Suppliers.Interfaces;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Services.Suppliers.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة الموردين، حساباتهم، وجهات الاتصال وأرقام الهواتف التابعة لهم.
/// </summary>
public class SupplierService : ISupplierService
{
    // وحدة العمل للوصول إلى مستودعات الموردين وهواتفهم وحفظ التعديلات
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل التلقائي بين الكيانات وDTOs
    private readonly IMapper _mapper;
    private readonly IPosCashService _posCashService;
    /// <summary>
    /// تهيئة خدمة الموردين مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, IPosCashService posCashService)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
        _posCashService = posCashService;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن المورد بالمعرف مع تضمين أرقام هواتفه
        var supplier = await _unitOfWork.Suppliers.GetDetailsAsync(id, ct);
        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم العثور على المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // تحويل الكيان إلى كائن استجابة DTO
        var dto = _mapper.Map<SupplierResponseDto>(supplier);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SupplierResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // استرجاع كافة الموردين مع هواتفهم مرتبين بالاسم
        var suppliers = await _unitOfWork.Suppliers.ListAsync(new SupplierQuery { }, ct);
        // تحويل قائمة الكيانات إلى قائمة ملخصات DTOs
        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(suppliers);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SupplierSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default) =>
        GetPagedAsync(pageNumber, pageSize, search, null, false, ct);

    public async Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search, bool? isActive, bool hasBalance, CancellationToken ct = default)
    {
        // تجهيز مواصفة الاستعلام المخصصة بالبحث والترتيب
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.Suppliers.SearchAsync(new SupplierQuery { Search = search, IsActive = isActive, HasBalance = hasBalance, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل قائمة العناصر المسترجعة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(items);
        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SupplierSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SupplierSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SupplierResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من عدم تكرار اسم المورد للمستأجر الحالي
        bool exists = await _unitOfWork.Suppliers.NameExistsAsync(dto.Name, ct);
        // في حال وجود مورد آخر مسجل بنفس الاسم
        if (exists)
        {
            // إرجاع خطأ تكرار اسم المورد
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد موجود بالفعل", ErrorCodes.SupplierNameExists);
        }

        // تحويل كائن DTO إلى كيان المورد
        var supplier = _mapper.Map<Supplier>(dto);
        supplier.CurrentBalance = dto.OpeningBalance;
        // التحقق من وجود هواتف مرفقة مع المورد الجديد
        if (dto.Phones != null && dto.Phones.Any())
        {
            // بناء كيانات هواتف المورد
            supplier.SupplierPhones = dto.Phones.Select(p => new SupplierPhone { // تعيين رقم الهاتف
 PhoneNumber = p.PhoneNumber, // تعيين اسم جهة الاتصال
 Name = p.Name }).ToList();
        }

        // إضافة كيان المورد وهواتفه إلى المستودع
        await _unitOfWork.Suppliers.AddAsync(supplier, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب المورد المحفوظ مع هواتفه
        var created = await _unitOfWork.Suppliers.GetDetailsAsync(supplier.Id, ct) ?? supplier;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(created);
        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SupplierResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // البحث عن المورد الحالي مع هواتفه
        var supplier = await _unitOfWork.Suppliers.GetDetailsAsync(id, ct);
        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // التحقق من عدم تعارض الاسم الجديد مع مورد آخر
        bool nameExists = await _unitOfWork.Suppliers.NameExistsAsync(dto.Name, id, ct);
        // في حال وجود مورد آخر بنفس الاسم
        if (nameExists)
        {
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد مستخدم بالفعل لمورد آخر", ErrorCodes.SupplierNameExists);
        }

        // نقل البيانات المحدثة من DTO إلى الكيان
        _mapper.Map(dto, supplier);
        foreach (var phone in supplier.SupplierPhones.ToList())
            _unitOfWork.Suppliers.HardDeletePhone(phone);
        supplier.SupplierPhones = dto.Phones.Select(phone => new SupplierPhone
        {
            SupplierId = id, PhoneNumber = phone.PhoneNumber, Name = phone.Name
        }).ToList();
        // ضمان ثبات معرف المورد الأصلي
        supplier.Id = id;
        // وسم الكيان للتحديث في المستودع
        _unitOfWork.Suppliers.Update(supplier);
        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب المورد المحدث مع هواتفه
        var updated = await _unitOfWork.Suppliers.GetDetailsAsync(id, ct) ?? supplier;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);
        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سجل المورد بالمعرف
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(id, ct);
        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم العثور على المورد
            return ServiceResult.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // تطبيق الحذف المنطقي للمورد
        if (supplier.CurrentBalance != 0)
            return ServiceResult.Failure("لا يمكن حذف مورد لديه رصيد قائم؛ استخدم التعطيل حتى تسوية حسابه", ErrorCodes.ValidationError);

        if (await _unitOfWork.Suppliers.HasFinancialHistoryAsync(id, ct))
            return ServiceResult.Failure("لا يمكن حذف مورد له سجل مالي؛ استخدم التعطيل للحفاظ على مستنداته وكشف حسابه", ErrorCodes.ValidationError);

        _unitOfWork.Suppliers.SoftDelete(supplier);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SupplierResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // البحث عن المورد بالمعرف
        var supplier = await _unitOfWork.Suppliers.GetDetailsAsync(supplierId, ct);
        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // إنشاء كيان رقم الهاتف الجديد
        var phone = new SupplierPhone
        {
            // تعيين معرف المورد
            SupplierId = supplierId,
            // تعيين رقم الهاتف
            PhoneNumber = dto.PhoneNumber,
            // تعيين اسم جهة الاتصال
            Name = dto.Name
        };
        // إضافة الهاتف إلى المستودع
        await _unitOfWork.Suppliers.AddPhoneAsync(phone, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب المورد مع قائمة الهواتف المحدثة
        var updated = await _unitOfWork.Suppliers.GetDetailsAsync(supplierId, ct) ?? supplier;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);
        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default)
    {
        // جلب سجل الهاتف بالمعرف
        var phone = await _unitOfWork.Suppliers.GetPhoneByIdAsync(phoneId, ct);
        // التحقق من وجود الهاتف وارتباطه بنفس المورد المحدد
        if (phone is null || phone.SupplierId != supplierId)
        {
            // إرجاع خطأ عدم وجود الهاتف أو عدم التبعية
            return ServiceResult.Failure("رقم الهاتف غير موجود أو لا ينتمي لهذا المورد", ErrorCodes.NotFound);
        }

        // تطبيق الحذف المنطقي للهاتف
        _unitOfWork.Suppliers.SoftDeletePhone(phone);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierStatementDto>> GetStatementAsync(SupplierStatementFilterDto filter, CancellationToken ct = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(filter.SupplierId, ct);
        if (supplier == null)
            return ServiceResult<SupplierStatementDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        // حساب الرصيد قبل بداية الفترة
        decimal balanceBeforePeriod = filter.FromDate.HasValue ? await _unitOfWork.Suppliers.GetBalanceBeforeDateAsync(supplier.Id, filter.FromDate.Value, ct) : supplier.OpeningBalance;
        // جلب الحركات خلال الفترة
        var transactions = await _unitOfWork.Suppliers.GetTransactionsByDateRangeAsync(supplier.Id, filter.FromDate, filter.ToDate, filter.BranchId, ct);
        var transactionDtos = _mapper.Map<List<SupplierTransactionResponseDto>>(transactions);
        var totalDebit = transactionDtos.Sum(t => t.DebitAmount);
        var totalCredit = transactionDtos.Sum(t => t.CreditAmount);
        var statement = new SupplierStatementDto
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            OpeningBalance = supplier.OpeningBalance,
            BalanceBeforePeriod = balanceBeforePeriod,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CurrentBalance = supplier.CurrentBalance,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Transactions = transactionDtos
        };
        return ServiceResult<SupplierStatementDto>.Success(statement);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierTransactionResponseDto>> AddPaymentAsync(CreateSupplierPaymentDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SupplierTransactionResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.PaymentMethod) || dto.PaymentMethod == PaymentMethod.Credit)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("طريقة السداد غير صالحة", ErrorCodes.ValidationError);
        if (!dto.SafeId.HasValue)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("يجب تحديد الخزينة التي سُحب منها المبلغ", ErrorCodes.ValidationError);
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(dto.SupplierId, ct);
        if (supplier == null)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        Safe? safe = null;
        Guid? cashSessionId = null;
        if (dto.SafeId.HasValue)
        {
            safe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.SafeId.Value, ct);
            if (safe == null)
                return ServiceResult<SupplierTransactionResponseDto>.Failure("الخزينة المحددة غير موجودة", ErrorCodes.SafeNotFound);
            if (!safe.IsActive)
                return ServiceResult<SupplierTransactionResponseDto>.Failure("الخزينة معطلة حالياً", ErrorCodes.SafeInactive);
            if (safe.BranchId != dto.BranchId)
                return ServiceResult<SupplierTransactionResponseDto>.Failure("الخزينة المحددة لا تتبع فرع عملية السداد", ErrorCodes.ValidationError);
            var sessionResult = await _posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
            if (!sessionResult.IsSuccess)
                return ServiceResult<SupplierTransactionResponseDto>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);
            cashSessionId = sessionResult.Data;
            if (safe.CurrentBalance < dto.Amount)
            {
                return ServiceResult<SupplierTransactionResponseDto>.Failure($"رصيد الخزينة الحالي ({safe.CurrentBalance:N2}) غير كافٍ لسداد دفعة المورد بمبلغ ({dto.Amount:N2})", ErrorCodes.InsufficientSafeBalance);
            }
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            supplier.CurrentBalance -= dto.Amount;
            var transaction = new SupplierTransaction
            {
                SupplierId = supplier.Id,
                TransactionType = SupplierTransactionType.Payment,
                DebitAmount = dto.Amount,
                CreditAmount = 0,
                BalanceAfter = supplier.CurrentBalance,
                TransactionDate = dto.PaymentDate,
                BranchId = dto.BranchId,
                DocumentType = FinancialDocumentType.SupplierPayment,
                PaymentMethod = dto.PaymentMethod,
                Description = dto.Description ?? "سند صرف / سداد دفعة للمورد",
                Notes = dto.Notes
            };
            await _unitOfWork.Suppliers.AddTransactionAsync(transaction, ct);
            _unitOfWork.Suppliers.Update(supplier);
            if (safe != null)
            {
                safe.CurrentBalance -= dto.Amount;
                var safeTxNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
                var safeTx = new SafeTransaction
                {
                    TransactionNumber = safeTxNumber,
                    SafeId = safe.Id,
                    Direction = TransactionDirection.Outbound,
                    Amount = dto.Amount,
                    BalanceAfter = safe.CurrentBalance,
                    TransactionDate = dto.PaymentDate,
                    DocumentType = FinancialDocumentType.SupplierPayment,
                    DocumentId = transaction.Id,
                    Description = $"سند صرف للمورد [{supplier.Name}] - {(string.IsNullOrWhiteSpace(dto.Description) ? "سداد نقدي" : dto.Description)}",
                    Notes = dto.Notes
                };
                safeTx.CashSessionId = cashSessionId;
                await _unitOfWork.Accounting.AddSafeTransactionAsync(safeTx, ct);
                _unitOfWork.Accounting.UpdateSafe(safe);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
            var branch = await _unitOfWork.Branches.GetByIdForUpdateAsync(dto.BranchId, ct);
            transaction.Branch = branch!;
            transaction.Supplier = supplier;
            return ServiceResult<SupplierTransactionResponseDto>.Success(_mapper.Map<SupplierTransactionResponseDto>(transaction));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SupplierTransactionResponseDto>> AddAdjustmentAsync(CreateSupplierAdjustmentDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SupplierTransactionResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.Direction))
            return ServiceResult<SupplierTransactionResponseDto>.Failure("اتجاه التسوية غير صالح", ErrorCodes.ValidationError);
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(dto.SupplierId, ct);
        if (supplier == null)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
            return ServiceResult<SupplierTransactionResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        // Inbound: زيادة التزام على المحل للمورد -> دائن
        // Outbound: تخفيض التزام على المحل لصالح المورد -> مدين
        decimal debit = 0;
        decimal credit = 0;
        if (dto.Direction == TransactionDirection.Inbound)
        {
            supplier.CurrentBalance += dto.Amount;
            credit = dto.Amount;
        }
        else
        {
            supplier.CurrentBalance -= dto.Amount;
            debit = dto.Amount;
        }

        var transaction = new SupplierTransaction
        {
            SupplierId = supplier.Id,
            TransactionType = SupplierTransactionType.Adjustment,
            DebitAmount = debit,
            CreditAmount = credit,
            BalanceAfter = supplier.CurrentBalance,
            TransactionDate = DateTime.UtcNow,
            BranchId = dto.BranchId,
            DocumentType = FinancialDocumentType.Adjustment,
            Description = dto.Reason,
            Notes = dto.Notes
        };
        await _unitOfWork.Suppliers.AddTransactionAsync(transaction, ct);
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync(ct);
        var branch = await _unitOfWork.Branches.GetByIdForUpdateAsync(dto.BranchId, ct);
        transaction.Branch = branch!;
        transaction.Supplier = supplier;
        return ServiceResult<SupplierTransactionResponseDto>.Success(_mapper.Map<SupplierTransactionResponseDto>(transaction));
    }
}
