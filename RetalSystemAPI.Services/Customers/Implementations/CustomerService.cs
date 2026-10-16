using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Customers.Interfaces;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Services.Customers.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة العملاء وسجل هواتفهم وحدود الائتمان وتدقيق الكود وحالات النشاط.
/// </summary>
public class CustomerService : ICustomerService
{
    // وحدة العمل للتعامل مع مستودعات العملاء وهواتفهم وحفظ التغييرات
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;
    private readonly IPosCashService _posCashService;
    /// <summary>
    /// تهيئة خدمة العملاء مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IPosCashService posCashService)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
        _posCashService = posCashService;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن العميل بالمعرف وتضمين هواتفه عبر المواصفة المخصصة
        var customer = await _unitOfWork.Customers.GetDetailsAsync(id, ct);
        // التحقق من وجود العميل
        if (customer is null)
        {
            // إرجاع خطأ عدم العثور على العميل
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة DTO
        var dto = _mapper.Map<CustomerResponseDto>(customer);
        // إرجاع النتيجة بنجاح
        return ServiceResult<CustomerResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة الفلترة حسب النوع وحالة النشاط والبحث
        // استرجاع قائمة العملاء المطابقين للمواصفة
        var customers = await _unitOfWork.Customers.ListAsync(new CustomerQuery { Type = type, IsActive = isActive, Search = search }, ct);
        // تحويل الكيانات إلى قائمة ملخصات العملاء
        var dtos = _mapper.Map<IReadOnlyList<CustomerSummaryDto>>(customers);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<CustomerSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة الاستعلام المخصصة بالترقيم والفلترة
        // جلب عناصر الصفحة المحددة وإجمالي عدد السجلات
        var(items, totalCount) = await _unitOfWork.Customers.SearchAsync(new CustomerQuery { Type = type, IsActive = isActive, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل قائمة الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<CustomerSummaryDto>>(items);
        // بناء كائن الصفحة الموحد
        var pagedResult = PagedResult<CustomerSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<CustomerSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<CustomerResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من كود العميل إن وجد لضمان عدم تكراره
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            // فحص وجود عميل آخر مسجل بنفس الكود
            bool codeExists = await _unitOfWork.Customers.CodeExistsAsync(dto.Code, ct);
            // في حالة وجود تطابق للكود
            if (codeExists)
            {
                // إرجاع خطأ تكرار الكود
                return ServiceResult<CustomerResponseDto>.Failure("كود العميل مستخدم بالفعل", ErrorCodes.CustomerCodeExists);
            }
        }

        // تحويل كائن DTO إلى كيان العميل
        var customer = _mapper.Map<Customer>(dto);
        customer.CurrentBalance = dto.OpeningBalance;
        // التحقق من وجود أرقام هواتف مرفقة
        if (dto.Phones != null && dto.Phones.Any())
        {
            // بناء كيانات هواتف العميل وربطها به
            customer.CustomerPhones = dto.Phones.Select(p => new CustomerPhone { // تعيين رقم الهاتف
 PhoneNumber = p.PhoneNumber, // تعيين اسم جهة الاتصال
 ContactName = p.ContactName, // تعيين حالة الرقم الافتراضي
 IsDefault = p.IsDefault }).ToList();
        }

        // إضافة كيان العميل وهواتفه للمستودع
        await _unitOfWork.Customers.AddAsync(customer, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب العميل المحفوظ مع هواتفه للتأكد من اكتمال البيانات
        var created = await _unitOfWork.Customers.GetDetailsAsync(customer.Id, ct) ?? customer;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<CustomerResponseDto>(created);
        // إرجاع نتيجة النجاح
        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<CustomerResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // البحث عن العميل الحالي مع هواتفه
        var customer = await _unitOfWork.Customers.GetDetailsAsync(id, ct);
        // التحقق من وجود العميل
        if (customer is null)
        {
            // إرجاع خطأ عدم وجود العميل
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        // التحقق من عدم تكرار الكود مع عميل آخر
        if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != customer.Code)
        {
            // فحص تطابق الكود لعميل آخر
            bool codeExists = await _unitOfWork.Customers.CodeExistsAsync(dto.Code, id, ct);
            // في حال وجود تعارض
            if (codeExists)
            {
                // إرجاع خطأ استخدام الكود مسبقاً
                return ServiceResult<CustomerResponseDto>.Failure("كود العميل مستخدم بالفعل لعميل آخر", ErrorCodes.CustomerCodeExists);
            }
        }

        // تحديث اسم العميل
        customer.Name = dto.Name;
        // تحديث كود العميل
        customer.Code = dto.Code;
        // تحديث البريد الإلكتروني
        customer.Email = dto.Email;
        // تحديث العنوان
        customer.Address = dto.Address;
        // تحديث تصنيف ونوع العميل
        customer.Type = dto.Type;
        // تحديث الحد الائتماني
        customer.CreditLimit = dto.CreditLimit;
        // تحديث حالة النشاط
        customer.IsActive = dto.IsActive;
        // التحقق من تقديم قائمة أرقام هواتف جديدة
        if (dto.Phones != null && dto.Phones.Any())
        {
            // حذف أرقام الهواتف القديمة في حال وجودها
            if (customer.CustomerPhones != null && customer.CustomerPhones.Any())
            {
                // المرور على قائمة الهواتف الحالية
                foreach (var phone in customer.CustomerPhones.ToList())
                {
                    // حذف الهاتف فيزيائياً
                    _unitOfWork.Customers.HardDeletePhone(phone);
                }
            }

            // إنشاء قائمة أرقام الهواتف الجديدة
            customer.CustomerPhones = dto.Phones.Select(p => new CustomerPhone { // تعيين معرف العميل
 CustomerId = id, // تعيين رقم الهاتف
 PhoneNumber = p.PhoneNumber, // تعيين اسم جهة الاتصال
 ContactName = p.ContactName, // تعيين حالة الرقم الافتراضي
 IsDefault = p.IsDefault }).ToList();
        }

        // وسم كيان العميل للتحديث في المستودع
        _unitOfWork.Customers.Update(customer);
        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب العميل المحدث مع بيانات الهواتف
        var updated = await _unitOfWork.Customers.GetDetailsAsync(id, ct) ?? customer;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<CustomerResponseDto>(updated);
        // إرجاع نتيجة التعديل بنجاح
        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سجل العميل بالمعرف
        var customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(id, ct);
        // التحقق من وجود العميل
        if (customer is null)
        {
            // إرجاع خطأ عدم العثور
            return ServiceResult.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        // تطبيق الحذف المنطقي للعميل
        _unitOfWork.Customers.SoftDelete(customer);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerResponseDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام العميل مع هواتفه
        var customer = await _unitOfWork.Customers.GetDetailsAsync(id, ct);
        // التحقق من وجود العميل
        if (customer is null)
        {
            // إرجاع خطأ عدم وجود العميل
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        // تبديل حالة النشاط الحالية
        customer.IsActive = !customer.IsActive;
        // وسم الكيان للتحديث
        _unitOfWork.Customers.Update(customer);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // تحويل الكيان المحدث إلى DTO
        var responseDto = _mapper.Map<CustomerResponseDto>(customer);
        // إرجاع نتيجة النجاح
        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerResponseDto>> AddPhoneAsync(Guid customerId, CustomerPhoneDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<CustomerResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // استعلام العميل وهوائفه بالمعرف
        var customer = await _unitOfWork.Customers.GetDetailsAsync(customerId, ct);
        // التحقق من وجود العميل
        if (customer is null)
        {
            // إرجاع خطأ عدم وجود العميل
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        // إذا كان الرقم الجديد هو الافتراضي، يتم إلغاء تعيين الرقم الافتراضي السابق
        if (dto.IsDefault && customer.CustomerPhones.Any(p => p.IsDefault))
        {
            // المرور على كافة الأرقام الافتراضية السابقة
            foreach (var p in customer.CustomerPhones.Where(p => p.IsDefault))
            {
                // إزالة علامة الافتراضي
                p.IsDefault = false;
                // وسم السجل للتحديث
                _unitOfWork.Customers.UpdatePhone(p);
            }
        }

        // إنشاء كيان رقم الهاتف الجديد
        var phone = new CustomerPhone
        {
            // تعيين معرف العميل
            CustomerId = customerId,
            // تعيين رقم الهاتف
            PhoneNumber = dto.PhoneNumber,
            // تعيين اسم جهة الاتصال
            ContactName = dto.ContactName,
            // تعيين حالة الرقم الافتراضي
            IsDefault = dto.IsDefault
        };
        // إضافة الهاتف للمستودع
        await _unitOfWork.Customers.AddPhoneAsync(phone, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب العميل مع قائمة الهواتف المحدثة
        var updated = await _unitOfWork.Customers.GetDetailsAsync(customerId, ct) ?? customer;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<CustomerResponseDto>(updated);
        // إرجاع النتيجة بنجاح
        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default)
    {
        // جلب سجل الهاتف بالمعرف
        var phone = await _unitOfWork.Customers.GetPhoneByIdAsync(phoneId, ct);
        // التحقق من وجود الهاتف وارتباطه بنفس العميل المحدد
        if (phone is null || phone.CustomerId != customerId)
        {
            // إرجاع خطأ عدم وجود الهاتف أو عدم التبعية
            return ServiceResult.Failure("رقم الهاتف غير موجود أو لا ينتمي لهذا العميل", ErrorCodes.NotFound);
        }

        // تطبيق الحذف المنطقي لرقم الهاتف
        _unitOfWork.Customers.SoftDeletePhone(phone);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerStatementDto>> GetStatementAsync(CustomerStatementFilterDto filter, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(filter.CustomerId, ct);
        if (customer == null)
            return ServiceResult<CustomerStatementDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        // حساب الرصيد قبل بداية الفترة
        decimal balanceBeforePeriod = filter.FromDate.HasValue ? await _unitOfWork.Customers.GetBalanceBeforeDateAsync(customer.Id, filter.FromDate.Value, ct) : customer.OpeningBalance;
        // جلب الحركات خلال الفترة
        var transactions = await _unitOfWork.Customers.GetTransactionsByDateRangeAsync(customer.Id, filter.FromDate, filter.ToDate, filter.BranchId, ct);
        var transactionDtos = _mapper.Map<List<CustomerTransactionResponseDto>>(transactions);
        var totalDebit = transactionDtos.Sum(t => t.DebitAmount);
        var totalCredit = transactionDtos.Sum(t => t.CreditAmount);
        var statement = new CustomerStatementDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            CreditLimit = customer.CreditLimit,
            OpeningBalance = customer.OpeningBalance,
            BalanceBeforePeriod = balanceBeforePeriod,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CurrentBalance = customer.CurrentBalance,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            Transactions = transactionDtos
        };
        return ServiceResult<CustomerStatementDto>.Success(statement);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerTransactionResponseDto>> AddPaymentAsync(CreateCustomerPaymentDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<CustomerTransactionResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.PaymentMethod) || dto.PaymentMethod == PaymentMethod.Credit)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("طريقة السداد غير صالحة", ErrorCodes.ValidationError);
        if (!dto.SafeId.HasValue)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("يجب تحديد الخزينة المستلمة للمبلغ", ErrorCodes.ValidationError);
        var customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId, ct);
        if (customer == null)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        Safe? safe = null;
        Guid? cashSessionId = null;
        if (dto.SafeId.HasValue)
        {
            safe = await _unitOfWork.Accounting.GetSafeByIdAsync(dto.SafeId.Value, ct);
            if (safe == null)
                return ServiceResult<CustomerTransactionResponseDto>.Failure("الخزينة المحددة غير موجودة", ErrorCodes.SafeNotFound);
            if (!safe.IsActive)
                return ServiceResult<CustomerTransactionResponseDto>.Failure("الخزينة معطلة حالياً", ErrorCodes.SafeInactive);
            if (safe.BranchId != dto.BranchId)
                return ServiceResult<CustomerTransactionResponseDto>.Failure("الخزينة المحددة لا تتبع فرع عملية السداد", ErrorCodes.ValidationError);
            var sessionResult = await _posCashService.ResolveDrawerPostingSessionAsync(safe.Id, ct);
            if (!sessionResult.IsSuccess)
                return ServiceResult<CustomerTransactionResponseDto>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);
            cashSessionId = sessionResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            customer.CurrentBalance -= dto.Amount;
            var transaction = new CustomerTransaction
            {
                CustomerId = customer.Id,
                TransactionType = CustomerTransactionType.Payment,
                DebitAmount = 0,
                CreditAmount = dto.Amount,
                BalanceAfter = customer.CurrentBalance,
                TransactionDate = dto.PaymentDate,
                BranchId = dto.BranchId,
                DocumentType = FinancialDocumentType.CustomerPayment,
                PaymentMethod = dto.PaymentMethod,
                Description = dto.Description ?? "سند قبض / سداد من العميل",
                Notes = dto.Notes
            };
            await _unitOfWork.Customers.AddTransactionAsync(transaction, ct);
            _unitOfWork.Customers.Update(customer);
            if (safe != null)
            {
                safe.CurrentBalance += dto.Amount;
                var safeTxNumber = await _unitOfWork.Accounting.GenerateSafeTransactionNumberAsync(ct);
                var safeTx = new SafeTransaction
                {
                    TransactionNumber = safeTxNumber,
                    SafeId = safe.Id,
                    Direction = TransactionDirection.Inbound,
                    Amount = dto.Amount,
                    BalanceAfter = safe.CurrentBalance,
                    TransactionDate = dto.PaymentDate,
                    DocumentType = FinancialDocumentType.CustomerPayment,
                    DocumentId = transaction.Id,
                    Description = $"سند قبض عميل [{customer.Name}] - {(string.IsNullOrWhiteSpace(dto.Description) ? "سداد نقدي" : dto.Description)}",
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
            transaction.Customer = customer;
            return ServiceResult<CustomerTransactionResponseDto>.Success(_mapper.Map<CustomerTransactionResponseDto>(transaction));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<CustomerTransactionResponseDto>> AddAdjustmentAsync(CreateCustomerAdjustmentDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<CustomerTransactionResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.Direction))
            return ServiceResult<CustomerTransactionResponseDto>.Failure("اتجاه التسوية غير صالح", ErrorCodes.ValidationError);
        var customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId, ct);
        if (customer == null)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
            return ServiceResult<CustomerTransactionResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        decimal debit = 0;
        decimal credit = 0;
        if (dto.Direction == TransactionDirection.Inbound)
        {
            customer.CurrentBalance += dto.Amount;
            debit = dto.Amount;
        }
        else
        {
            customer.CurrentBalance -= dto.Amount;
            credit = dto.Amount;
        }

        var transaction = new CustomerTransaction
        {
            CustomerId = customer.Id,
            TransactionType = CustomerTransactionType.Adjustment,
            DebitAmount = debit,
            CreditAmount = credit,
            BalanceAfter = customer.CurrentBalance,
            TransactionDate = DateTime.UtcNow,
            BranchId = dto.BranchId,
            DocumentType = FinancialDocumentType.Adjustment,
            Description = dto.Reason,
            Notes = dto.Notes
        };
        await _unitOfWork.Customers.AddTransactionAsync(transaction, ct);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);
        var branch = await _unitOfWork.Branches.GetByIdForUpdateAsync(dto.BranchId, ct);
        transaction.Branch = branch!;
        transaction.Customer = customer;
        return ServiceResult<CustomerTransactionResponseDto>.Success(_mapper.Map<CustomerTransactionResponseDto>(transaction));
    }
}
