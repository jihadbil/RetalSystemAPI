using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Common.Interfaces;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة فواتير المبيعات ونقاط البيع وخصم المخزون والتحقق من كفاية الأرصدة وترحيل الحركات المالية.
/// </summary>
public class SalesInvoiceService : ISalesInvoiceService
{
    // وحدة العمل للتعامل مع مستودعات الفواتير والعملاء والفروع والمخزون
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;
    // خدمة الترحيل المالي وتحديث أرصدة العملاء والخزائن
    private readonly IFinancialPostingService _financialPosting;
    // خدمة إدارة نقدية وجلسات نقاط البيع
    private readonly IPosCashService _posCashService;
    // خدمة التحقق من صلاحيات المستخدم الحالي
    private readonly ICurrentPermissionService? _permissions;
    // خدمة مدفوعات نقاط البيع المتعددة
    private readonly IPosPaymentService? _posPayments;

    /// <summary>
    /// تهيئة خدمة فواتير المبيعات مع حقن التبعيات اللازمة.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    /// <param name="financialPosting">خدمة الترحيل المحاسبي</param>
    /// <param name="posCashService">خدمة جلسات الكاشير</param>
    /// <param name="permissions">خدمة فحص الصلاحيات</param>
    /// <param name="posPayments">خدمة المدفوعات المجزأة لنقاط البيع</param>
    public SalesInvoiceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFinancialPostingService financialPosting,
        IPosCashService posCashService,
        ICurrentPermissionService? permissions = null,
        IPosPaymentService? posPayments = null)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول النماذج
        _mapper = mapper;
        // تعيين خدمة الترحيل المالي
        _financialPosting = financialPosting;
        // تعيين خدمة نقدية نقاط البيع
        _posCashService = posCashService;
        // تعيين خدمة الصلاحيات
        _permissions = permissions;
        // تعيين خدمة مدفوعات نقاط البيع
        _posPayments = posPayments;
    }

    /// <summary>
    /// تحويل كيان الفاتورة إلى DTO مع إثراء البيانات ببيانات جهة الإصدار وحركات المدفوعات المجزأة.
    /// </summary>
    /// <param name="invoice">كيان فاتورة المبيعات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>كائن DTO متكامل للفاتورة متضمناً جهة الإصدار والمدفوعات</returns>
    private async Task<SalesInvoiceResponseDto> MapWithIssuerAsync(SalesInvoice invoice, CancellationToken ct)
    {
        // تحويل كيان الفاتورة الأساسي إلى كائن استجابة DTO
        var dto = _mapper.Map<SalesInvoiceResponseDto>(invoice);

        // جلب حركات المدفوعات المسجلة للفاتورة في حال كانت خدمة المدفوعات مفعلة
        if (_posPayments is not null)
        {
            // إسناد تفاصيل المدفوعات إلى كائن الاستجابة
            dto.Payments = await _posPayments.GetInvoicePaymentsAsync(invoice.Id, ct);
        }

        // استعلام بيانات المستأجر لإدراج معلومات جهة الإصدار والفاتورة الضريبية
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(invoice.TenantId, ct);

        // في حال العثور على المستأجر نقوم بتعبئة ترويسة جهة الإصدار
        if (tenant is not null)
        {
            // بناء كائن جهة الإصدار بالاسم التجاري والرقم الضريبي والعنوان والهاتف
            dto.Issuer = new InvoiceIssuerDto
            {
                Name = string.IsNullOrWhiteSpace(tenant.CommercialName) ? tenant.Name : tenant.CommercialName,
                TaxNumber = tenant.TaxNumber,
                Address = tenant.Address,
                Phone = tenant.PhoneNumber
            };
        }

        // إرجاع كائن الاستجابة بعد الإثراء
        return dto;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام فاتورة المبيعات بالمعرف مع تفاصيل البنود والعميل والفرع
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct);

        // التحقق من وجود الفاتورة في قاعدة البيانات
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // تحويل الكيان إلى كائن استجابة DTO مع بيانات جهة الإصدار
        var dto = await MapWithIssuerAsync(invoice, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        // استعلام الفاتورة بواسطة رقم الفاتورة المميز
        var invoice = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);

        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // تحويل الكيان إلى DTO مع جهة الإصدار
        var dto = await MapWithIssuerAsync(invoice, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        // استرجاع قائمة الفواتير المطابقة لمعايير الفلترة المحددة
        var invoices = await _unitOfWork.SalesInvoices.ListAsync(new SalesInvoiceQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            CustomerId = customerId,
            Status = status,
            PaymentMethod = paymentMethod,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search
        }, ct);

        // تحويل قائمة الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(invoices);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        // تنفيذ استعلام الصفحة المجزأة مع جلب إجمالي العدد
        var (items, totalCount) = await _unitOfWork.SalesInvoices.SearchAsync(new SalesInvoiceQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            CustomerId = customerId,
            Status = status,
            PaymentMethod = paymentMethod,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SalesInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SalesInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> CreatePosAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        // التحقق من امتلاك المستخدم لصلاحية إنشاء حركة بيع من نقطة البيع
        if (_permissions?.HasPermission(Permissions.Pos.CreateSale) != true)
        {
            // إرجاع خطأ عدم امتلاك الصلاحية
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا تملك صلاحية إتمام بيع نقطة البيع", ErrorCodes.Forbidden);
        }

        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            // إرجاع خطأ التحقق
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // في حال وجود خصم على الفاتورة أو البنود يتم التحقق من امتلاك المستخدم لصلاحية تطبيق الخصم
        if ((dto.DiscountAmount > 0 || dto.Items.Any(i => i.DiscountAmount > 0)) &&
            !_permissions.HasPermission(Permissions.Pos.ApplyDiscount))
        {
            // إرجاع خطأ منع الخصم بدون صلاحية
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا تملك صلاحية منح خصم في نقطة البيع", ErrorCodes.Forbidden);
        }

        // معالجة وسائل الدفع المتعددة في حال إرسالها
        if (dto.Payments is not null)
        {
            // التحقق من تفعيل خدمة المدفوعات وعدم تكرار وسائل الدفع أو معرفات حركات العميل
            if (_posPayments is null ||
                dto.Payments.Select(p => p.PaymentMethod).Distinct().Count() != dto.Payments.Count ||
                dto.Payments.Select(p => p.ClientMovementId).Distinct().Count() != dto.Payments.Count)
            {
                // إرجاع خطأ عدم صحة توزيع وسائل الدفع
                return ServiceResult<SalesInvoiceResponseDto>.Failure("توزيع وسائل الدفع غير صالح أو يحتوي وسيلة مكررة", ErrorCodes.ValidationError);
            }

            // احتساب إجمالي المبلغ المدفوع من مجموع وسائل الدفع
            dto.PaidAmount = dto.Payments.Sum(p => p.Amount);

            // تحديد طريقة الدفع كـ متعددة في حال تعدد الوسائل، أو مطابقة للوسيلة الوحيدة
            dto.PaymentMethod = dto.Payments.Count > 1 ? PaymentMethod.Multiple : dto.Payments[0].PaymentMethod;

            // تصفير خزينة الدفع في حالة الدفع المتعدد لأن كل وسيلة تسجل بحركتها المستقلة
            dto.PaymentSafeId = null;
        }

        // تهيئة وتوحيد رقم فاتورة نقطة البيع ببصمة فريدة
        var invoiceNumber = PosSaleRequest.InvoiceNumber(dto.InvoiceNumber);

        // توليد بصمة الطلب الفريدة للتحقق من التكرار
        var requestHash = PosSaleRequest.Fingerprint(dto);

        // فحص مسبق لوجود الفاتورة برقمها المولد
        var existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);

        // في حال وجود الفاتورة مسبقاً يتم استدعاء معالج إعادة الإرسال (Replay Handler)
        if (existing is not null)
        {
            // إرجاع النتيجة المطابقة أو رسالة تعارض في حال اختلاف محتوى الطلب
            return await PosReplayAsync(existing, requestHash, ct);
        }

        // جلب المنتجات للتحقق من تطابق الأسعار اللحظية مع أسعار البيع المعتمدة في النظام
        var products = (await _unitOfWork.Products.GetByIdsAsync(dto.Items.Select(i => i.ProductId).Distinct().ToList(), ct)).ToDictionary(p => p.Id);

        // فحص أسعار البنود للتأكد من عدم التلاعب بها وتطابقها مع سعر بيع الصنف في الكتالوج
        if (dto.Items.Any(i => !products.TryGetValue(i.ProductId, out var product) || i.UnitPrice != product.SalePrice))
        {
            // إرجاع خطأ تغير أسعار الأصناف
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيرت أسعار الأصناف أو حالتها؛ أعد تحميلها قبل إتمام البيع", ErrorCodes.ValidationError);
        }

        // التحقق من حساب التحصيل المصرفي في حال كان الدفع غير نقدي
        if (dto.Payments is null && dto.PaymentMethod != PaymentMethod.Cash && dto.PaidAmount > 0)
        {
            // جلب الخزينة أو الحساب المصرفي المختار
            var account = dto.PaymentSafeId.HasValue ? await _unitOfWork.Accounting.GetSafeByIdAsync(dto.PaymentSafeId.Value, ct) : null;

            // التأكد من أن الحساب عبارة عن حساب بنكي نشط تابع لنفس الفرع
            if (account is null || account.Type != SafeType.Bank || !account.IsActive || account.BranchId != dto.BranchId)
            {
                // إرجاع خطأ تحديد الحساب المصرفي
                return ServiceResult<SalesInvoiceResponseDto>.Failure("اختر حساب تحصيل مصرفي نشطاً تابعاً لفرع نقطة البيع", ErrorCodes.ValidationError);
            }
        }

        try
        {
            // استدعاء المنطق الجوهري لإنشاء الفاتورة وترحيل المخزون مع تمرير بصمة الطلب
            var result = await CreateCoreAsync(PosSaleRequest.ForPosting(dto, invoiceNumber), requestHash, ct);

            // في حال عدم نجاح العملية يتم التأكد من عدم إنشاء الفاتورة بواسطة طلب متزامن فاز في السباق
            if (!result.IsSuccess)
            {
                // محاولة إعادة جلب الفاتورة برقمها
                existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);

                // إذا وجدت الفاتورة نطبق منطق إعادة الطلب
                if (existing is not null) return await PosReplayAsync(existing, requestHash, ct);
            }

            // إرجاع نتيجة الإنشاء
            return result;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // في حال حدوث تعارض فريد في الفهرس نتيجة طلب متزامن فاز في السباق
            existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);

            // إذا وجدت الفاتورة نطبق فحص البصمة المطابقة
            if (existing is not null) return await PosReplayAsync(existing, requestHash, ct);

            // وإلا نرمي الاستثناء للأعلى
            throw;
        }
    }

    /// <summary>
    /// معالجة طلب نقطة البيع المكرر (Replay): إرجاع الفاتورة القائمة إن كانت البصمة متطابقة، أو رفض الطلب إن تغيرت البيانات.
    /// </summary>
    /// <param name="existing">كيان الفاتورة المسجلة مسبقاً</param>
    /// <param name="requestHash">بصمة الطلب الحالي المطلوب التحقق منها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة الفاتورة السابقة أو رسالة خطأ بالتعارض</returns>
    private async Task<ServiceResult<SalesInvoiceResponseDto>> PosReplayAsync(SalesInvoice existing, string requestHash, CancellationToken ct) =>
        // التحقق من تطابق بصمة الطلب مع البصمة المسجلة في الفاتورة
        existing.PosRequestHash == requestHash
            // في حال التطابق الكامل نرجع الفاتورة بنجاح دون تكرار أي أثر محاسبي أو مخزني
            ? ServiceResult<SalesInvoiceResponseDto>.Success(await MapWithIssuerAsync(existing, ct))
            // في حال اختلاف البيانات نرفض الطلب لحماية البيانات من التعديل برقم مكرر
            : ServiceResult<SalesInvoiceResponseDto>.Failure("رقم عملية نقطة البيع مستخدم لطلب مختلف؛ لا تعِد استخدامه بعد تغيير البيانات", ErrorCodes.SalesInvoiceNumberExists);

    /// <inheritdoc/>
    public Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default) =>
        // استدعاء المنطق الجوهري لإنشاء الفاتورة دون بصمة نقطة بيع
        CreateCoreAsync(dto, null, ct);

    /// <summary>
    /// المنطق الجوهري لإنشاء فواتير المبيعات: التحقق من الكيانات، فحص كفاية المخزون، خصم الأرصدة، وترحيل القيود المالية.
    /// </summary>
    /// <param name="dto">نموذج إنشاء الفاتورة</param>
    /// <param name="posRequestHash">بصمة طلب نقطة البيع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة الفاتورة المنشأة</returns>
    private async Task<ServiceResult<SalesInvoiceResponseDto>> CreateCoreAsync(CreateSalesInvoiceDto dto, string? posRequestHash, CancellationToken ct)
    {
        // التحقق من عدم محاولة استخدام الدفع المتعدد خارج مسار نقاط البيع
        if (dto.Payments is not null && (posRequestHash is null || _posPayments is null) ||
            dto.Payments is null && dto.PaymentMethod == PaymentMethod.Multiple)
        {
            // إرجاع خطأ اقتصار الدفع المتعدد على نقاط البيع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الدفع المتعدد متاح من مسار نقطة البيع مع توزيع الوسائل فقط", ErrorCodes.ValidationError);
        }

        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            // إرجاع خطأ التحقق
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // منع إرسال الفاتورة كمسودة لأن هذا المسار يعتمد الترحيل الفوري للمخزون والحسابات
        if (dto.Status == InvoiceStatus.Draft)
        {
            // إرجاع خطأ إرسال المسودة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إنشاء الفاتورة هنا يرحّل المخزون والحسابات؛ لا يمكن إرسالها كمسودة", ErrorCodes.ValidationError);
        }

        // التحقق من وجود الفرع المحدد في قاعدة البيانات
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من وجود المستودع أو صالة العرض المحددة ككيان متتبع
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // التأكد من نشاط المستودع وتبعيته لفرع الفاتورة وصحة نوعه
        if (!warehouse.IsActive || warehouse.BranchId != dto.BranchId || !Enum.IsDefined(warehouse.Type))
        {
            // إرجاع خطأ عدم توافق المستودع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب اختيار مستودع نشط تابع لفرع الفاتورة", ErrorCodes.ValidationError);
        }

        // متغير لتخزين كائن العميل إن وجد
        Customer? customer = null;

        // التحقق من وجود العميل ونشاطه في حال تم تحديده
        if (dto.CustomerId.HasValue)
        {
            // جلب العميل ككيان متتبع
            customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId.Value, ct);

            // التحقق من وجود العميل
            if (customer is null)
            {
                // إرجاع خطأ عدم وجود العميل
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }

            // التحقق من أن حساب العميل نشط
            if (!customer.IsActive)
            {
                // إرجاع خطأ تعطيل العميل
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد معطل حالياً", ErrorCodes.ValidationError);
            }
        }

        // التحقق المسبق من عدم تكرار رقم فاتورة المبيعات
        bool numExists = await _unitOfWork.SalesInvoices.NumberExistsAsync(dto.InvoiceNumber, ct);
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("رقم الفاتورة مستخدم بالفعل", ErrorCodes.SalesInvoiceNumberExists);
        }

        // التحقق من احتواء الفاتورة على بند واحد على الأقل
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ خلو الفاتورة من البنود
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل للفاتورة", ErrorCodes.ValidationError);
        }

        // استخراج قائمة معرفات المنتجات الفريدة
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

        // جلب بيانات المنتجات للتحقق من وجودها واحتساب التكاليف
        var productsById = (await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).ToDictionary(p => p.Id);

        // التحقق من العثور على كافة المنتجات المطلوبة
        if (productsById.Count != productIds.Count)
        {
            // إرجاع خطأ عدم وجود أحد المنتجات
            return ServiceResult<SalesInvoiceResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من صحة بيانات كل بند (الكميات والأسعار والخصومات)
        foreach (var item in dto.Items)
        {
            // حساب إجمالي السطر قبل الخصم
            var gross = item.Quantity * item.UnitPrice;

            // التحقق من أن الكمية موجبة والأسعار والخصومات غير سالبة ولا يتجاوز الخصم قيمة البند
            if (item.Quantity <= 0 || item.UnitPrice < 0 || item.DiscountAmount < 0 || item.DiscountAmount > gross)
            {
                // إرجاع خطأ عدم صحة بيانات البند
                return ServiceResult<SalesInvoiceResponseDto>.Failure("بيانات كمية أو سعر أو خصم أحد البنود غير صحيحة", ErrorCodes.ValidationError);
            }
        }

        // حساب إجمالي قيمة البنود الإجمالية
        var grossSubTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

        // حساب إجمالي خصومات السطور
        var lineDiscounts = dto.Items.Sum(i => i.DiscountAmount);

        // صافي القيمة قبل خصم الفاتورة العام
        var netBeforeInvoiceDiscount = grossSubTotal - lineDiscounts;

        // التحقق من منطقية خصم الفاتورة العام ومبلغ الضريبة
        if (dto.DiscountAmount < 0 || dto.DiscountAmount > netBeforeInvoiceDiscount || dto.TaxAmount < 0)
        {
            // إرجاع خطأ تجاوز خصم الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("خصم الفاتورة يتجاوز قيمة البنود", ErrorCodes.ValidationError);
        }

        // احتساب الإجمالي النهائي للفاتورة خادماً للضريبة والخصم
        var calculatedTotal = netBeforeInvoiceDiscount - dto.DiscountAmount + dto.TaxAmount;

        // التحقق من أن المبلغ المدفوع لا يتجاوز الإجمالي المحسوب
        if (dto.PaidAmount < 0 || dto.PaidAmount > calculatedTotal)
        {
            // إرجاع خطأ تجاوز المبلغ المدفوع للإجمالي
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", ErrorCodes.ValidationError);
        }

        // حساب المبلغ المتبقي الآجل
        var remainingAmount = calculatedTotal - dto.PaidAmount;

        // إلزامية تحديد العميل في حال وجود متبقٍ آجل على الفاتورة
        if (remainingAmount > 0 && customer is null)
        {
            // إرجاع خطأ وجوب تحديد العميل في البيع الآجل
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب تحديد العميل عند وجود مبلغ آجل", ErrorCodes.ValidationError);
        }

        // التحقق من سقف ائتمان العميل قبل إتمام البيع الآجل
        if (customer is not null && customer.CreditLimit > 0 && customer.CurrentBalance + remainingAmount > customer.CreditLimit)
        {
            // إرجاع خطأ تجاوز سقف الائتمان
            return ServiceResult<SalesInvoiceResponseDto>.Failure("قيمة الفاتورة تتجاوز سقف ائتمان العميل", ErrorCodes.ValidationError);
        }

        // متغير لجلسة الصندوق النقدية
        CashSession? cashSession = null;

        // معرف الخزينة المعتمدة للتحصيل
        Guid? effectiveSafeId = dto.PaymentSafeId;

        // في حال ربط الفاتورة بجلسة صندوق نقدية (وردية كاشير)
        if (dto.CashSessionId.HasValue)
        {
            // منع ربط الوردية بفاتورة غير نقدية عند الدفع الفردي
            if (dto.Payments is null && dto.PaymentMethod != PaymentMethod.Cash)
            {
                // إرجاع خطأ عدم توافق وسيلة الدفع مع جلسة النقدية
                return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يجوز ربط وردية درج نقود بفاتورة غير نقدية", ErrorCodes.ValidationError);
            }

            // التحقق من أن جلسة الصندوق مفتوحة وصالحة للفرع
            var sessionResult = await _posCashService.ResolveOpenSessionForPostingAsync(dto.CashSessionId.Value, dto.BranchId, ct);

            // في حال عدم صلاحية الجلسة نرجع الخطأ
            if (!sessionResult.IsSuccess)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);
            }

            // إسناد كائن جلسة الصندوق
            cashSession = sessionResult.Data;

            // تحديد خزينة التحصيل كدرج النقدية التابع للجلسة
            effectiveSafeId = cashSession!.CashDrawerId;
        }
        else if (dto.Payments is not null ||
                 (dto.PaymentMethod == PaymentMethod.Cash && dto.PaidAmount > 0 && dto.InvoiceNumber.StartsWith("POS-", StringComparison.OrdinalIgnoreCase)))
        {
            // إلزام فتح جلسة صندوق نقدية لمبيعات نقاط البيع النقدية
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب فتح وردية درج نقود وربطها قبل تنفيذ بيع نقدي من نقطة البيع", ErrorCodes.ValidationError);
        }

        // حساب المبلغ النقدي المراد ترحيله للخزينة
        var cashToPost = dto.Payments is null
            ? dto.PaidAmount
            : dto.Payments.Where(p => p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount);

        // التحقق من صلاحية ونشاط خزينة التحصيل
        var safeResult = await _financialPosting.ResolveSafeAsync(dto.BranchId, effectiveSafeId, cashToPost, outbound: false, ct);
        if (!safeResult.IsSuccess)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
        }

        // الخزينة المعتمدة للتحصيل
        Safe? paymentSafe = safeResult.Data;

        // جلب الباركودات الافتراضية للبنوك الناقصة إن وجدت
        var productIdsNeedingDefaultBarcode = dto.Items.Where(i => !i.ProductBarCodeId.HasValue).Select(i => i.ProductId).Distinct().ToList();
        if (productIdsNeedingDefaultBarcode.Count > 0)
        {
            // جلب باركودات الأصناف من قاعدة البيانات
            var defaultBarcodes = await _unitOfWork.Products.GetBarcodesByProductIdsAsync(productIdsNeedingDefaultBarcode, ct);

            // تجميع أول باركود لكل منتج
            var defaultBarcodeByProduct = defaultBarcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.First());

            // تعيين الباركود لكل بند ناقص
            foreach (var item in dto.Items)
            {
                if (!item.ProductBarCodeId.HasValue && defaultBarcodeByProduct.TryGetValue(item.ProductId, out var defaultBarcode))
                {
                    item.ProductBarCodeId = defaultBarcode.Id;
                }
            }
        }

        // فحص كفاية الأرصدة المتوفرة في المخزون حسب نوع المستودع
        if (warehouse.Type == WarehouseType.Show)
        {
            // تجميع إجمالي الكميات المطلوبة لكل منتج في الصالة
            var productTotals = dto.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));

            // جلب الأرصدة المتتبعة في الصالة
            var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productTotals.Keys.ToList(), ct)).ToDictionary(s => s.ProductId);

            // التحقق من كفاية رصيد كل منتج
            foreach (var (productId, totalQty) in productTotals)
            {
                if (!stocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < totalQty)
                {
                    // إرجاع خطأ عدم كفاية رصيد الصالة
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد", ErrorCodes.InsufficientShowroomStock);
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            // التحقق من وجود باركود لكل بند للبيع من المستودع
            foreach (var item in dto.Items)
            {
                if (!item.ProductBarCodeId.HasValue)
                {
                    // إرجاع خطأ عدم وجود باركود
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("لم يتم العثور على باركود للصنف المحدد في المخزن", ErrorCodes.InsufficientStock);
                }
            }

            // تجميع الكميات المطلوبة لكل باركود
            var barcodeTotals = dto.Items.GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));

            // جلب أرصدة المستودع المتتبعة للباركودات
            var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeTotals.Keys.ToList(), ct)).ToDictionary(s => s.ProductBarcodeId);

            // التحقق من كفاية رصيد المستودع لكل باركود
            foreach (var (barcodeId, totalQty) in barcodeTotals)
            {
                if (!stocksByBarcode.TryGetValue(barcodeId, out var stock) || stock.Quantity < totalQty)
                {
                    // إرجاع خطأ عدم كفاية رصيد المستودع
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن للصنف المحدد", ErrorCodes.InsufficientStock);
                }
            }
        }

        // تحويل كائن DTO إلى كيان فاتورة المبيعات وضبط القيم المحسوبة الآمنة
        var invoice = _mapper.Map<SalesInvoice>(dto);
        invoice.PosRequestHash = posRequestHash;
        invoice.InvoiceDate = dto.InvoiceDate == default ? DateTime.UtcNow : dto.InvoiceDate;
        invoice.SubTotal = grossSubTotal;
        invoice.TaxAmount = dto.TaxAmount;
        invoice.TotalAmount = calculatedTotal;
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingAmount = remainingAmount;
        invoice.PaymentSafeId = dto.Payments is not null ? cashSession!.CashDrawerId : paymentSafe?.Id;
        invoice.CashSessionId = cashSession?.Id;
        invoice.PosTerminalId = cashSession?.PosTerminalId;
        invoice.Status = calculatedTotal == dto.PaidAmount
            ? InvoiceStatus.Paid
            : dto.PaidAmount > 0
                ? InvoiceStatus.PartiallyPaid
                : InvoiceStatus.Pending;

        // بناء قائمة بنود الفاتورة وحساب التكلفة وإجماليات السطور بدقة
        invoice.Items = dto.Items.Select(item => new SalesInvoiceItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            UnitCost = productsById[item.ProductId].AveragePrice > 0
                ? productsById[item.ProductId].AveragePrice
                : productsById[item.ProductId].CostPrice,
            DiscountAmount = item.DiscountAmount,
            LineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount
        }).ToList();

        // بدء معاملة قاعدة بيانات لحفظ الفاتورة وخصم المخزون وترحيل الحركات المالية
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. خصم الكميات من أرصدة المخزون المتتبعة مباشرة
            if (warehouse.Type == WarehouseType.Show)
            {
                var productTotals = invoice.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productTotals.Keys.ToList(), ct)).ToDictionary(s => s.ProductId);
                foreach (var (productId, totalQty) in productTotals)
                {
                    if (stocksByProduct.TryGetValue(productId, out var stock))
                    {
                        stock.Quantity = checked(stock.Quantity - (int)totalQty);
                    }
                }
            }
            else if (warehouse.Type == WarehouseType.Storge)
            {
                var barcodeTotals = invoice.Items.Where(i => i.ProductBarCodeId.HasValue).GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeTotals.Keys.ToList(), ct)).ToDictionary(s => s.ProductBarcodeId);
                foreach (var (barcodeId, totalQty) in barcodeTotals)
                {
                    if (stocksByBarcode.TryGetValue(barcodeId, out var stock))
                    {
                        stock.Quantity = checked(stock.Quantity - (int)totalQty);
                    }
                }
            }

            // 2. إضافة الفاتورة إلى مستودع الفواتير
            if (invoice.Id == Guid.Empty) invoice.Id = Guid.NewGuid();
            await _unitOfWork.SalesInvoices.AddAsync(invoice, ct);

            // 3. ترحيل حساب العميل (قيد إثبات المديونية + قيد الدفعة إن وجدت)
            if (customer is not null)
            {
                // إثبات إجمالي الفاتورة مديونية على العميل
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    invoice.TotalAmount,
                    CustomerTransactionType.SalesInvoice,
                    invoice.BranchId,
                    FinancialDocumentType.SalesInvoice,
                    invoice.Id,
                    invoice.InvoiceDate,
                    invoice.PaymentMethod == PaymentMethod.Multiple ? null : invoice.PaymentMethod,
                    $"فاتورة مبيعات رقم {invoice.InvoiceNumber}",
                    ct);

                // إثبات سداد الدفعة المسددة في رصيد العميل
                if (dto.Payments is null)
                {
                    await _financialPosting.PostCustomerBalanceAsync(
                        customer,
                        -invoice.PaidAmount,
                        CustomerTransactionType.Payment,
                        invoice.BranchId,
                        FinancialDocumentType.SalesInvoice,
                        invoice.Id,
                        invoice.InvoiceDate,
                        invoice.PaymentMethod,
                        $"دفعة فاتورة مبيعات رقم {invoice.InvoiceNumber}",
                        ct);
                }
                else
                {
                    foreach (var payment in dto.Payments)
                    {
                        await _financialPosting.PostCustomerBalanceAsync(
                            customer,
                            -payment.Amount,
                            CustomerTransactionType.Payment,
                            invoice.BranchId,
                            FinancialDocumentType.SalesInvoice,
                            invoice.Id,
                            invoice.InvoiceDate,
                            payment.PaymentMethod,
                            $"دفعة فاتورة مبيعات رقم {invoice.InvoiceNumber}",
                            ct);
                    }
                }
            }

            // 4. ترحيل رصيد الخزينة للجزء النقدي المحصل
            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(
                    paymentSafe,
                    cashToPost,
                    FinancialDocumentType.SalesInvoice,
                    invoice.Id,
                    invoice.InvoiceDate,
                    $"تحصيل فاتورة مبيعات رقم {invoice.InvoiceNumber}",
                    ct,
                    invoice.CashSessionId,
                    $"sales-invoice:{invoice.Id}:payment");
            }

            // 5. تسجيل حركات المدفوعات المجزأة لنقطة البيع إن وجدت
            if (dto.Payments is not null)
            {
                await _posPayments!.RecordAsync(invoice, cashSession!, dto.Payments, ct);
            }

            // حفظ كافة التغييرات في سياق قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);

            // اعتماد وتأكيد المعاملة الذرية
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // التراجع عن المعاملة عند حدوث تعارض تزامن وإرجاع خطأ التزامن
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغير المخزون أو رصيد الدرج أثناء الترحيل؛ أعد تحميل البيانات وحاول مجدداً", ErrorCodes.ConcurrencyError);
        }
        catch
        {
            // التراجع عن المعاملة عند أي استثناء آخر
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // جلب الفاتورة المحدثة مع كامل تفاصيلها
        var created = await _unitOfWork.SalesInvoices.GetDetailsAsync(invoice.Id, ct) ?? invoice;

        // تحويل الكيان إلى DTO مع جهة الإصدار والمدفوعات
        var responseDto = await MapWithIssuerAsync(created, ct);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة بيانات التحديث
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // منع تحويل المسودة إلى حالة مرحلة عبر هذا المسار
        if (dto.Status is not null and not InvoiceStatus.Draft || dto.PaidAmount.GetValueOrDefault() != 0)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تعديل المسودة لا يسمح بالترحيل أو تسجيل دفعات", ErrorCodes.ValidationError);
        }

        // جلب الفاتورة للتعديل ككيان متتبع
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // منع تعديل الفاتورة المرحلة أو المرتبطة بوردية أو نقاط بيع
        if (invoice.Status != InvoiceStatus.Draft || invoice.PaidAmount != 0 || invoice.CashSessionId.HasValue ||
            invoice.InvoiceNumber.StartsWith("POS-", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مرحلة — استخدم الإلغاء أو تسجيل مرتجع", ErrorCodes.ValidationError);
        }

        // جلب المستودع المرتبط
        var warehouse = invoice.Warehouse ?? await _unitOfWork.Warehouses.GetByIdForUpdateAsync(invoice.WarehouseId, ct);
        if (warehouse is null || !Enum.IsDefined(warehouse.Type))
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // قائمة البنود البديلة الجديدة
        List<SalesInvoiceItem>? replacement = null;

        // معالجة البنود الجديدة إن تم إرسالها
        if (dto.Items is not null)
        {
            if (dto.Items.Count == 0)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب أن تحتوي الفاتورة على بند واحد على الأقل", ErrorCodes.ValidationError);
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).ToDictionary(p => p.Id);
            if (products.Count != productIds.Count)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
            }

            var barcodes = await _unitOfWork.Products.GetBarcodesByProductIdsAsync(productIds, ct);
            var barcodeById = barcodes.ToDictionary(b => b.Id);
            var defaults = barcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).First().Id);

            replacement = new List<SalesInvoiceItem>();
            foreach (var item in dto.Items)
            {
                var barcodeId = item.ProductBarCodeId ?? (defaults.TryGetValue(item.ProductId, out var fallback) ? fallback : (Guid?)null);
                if (barcodeId.HasValue && (!barcodeById.TryGetValue(barcodeId.Value, out var barcode) || barcode.ProductId != item.ProductId) ||
                    warehouse.Type == WarehouseType.Storge && !barcodeId.HasValue)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الباركود لا يتبع الصنف المحدد أو غير متوفر", ErrorCodes.ValidationError);
                }

                var gross = item.Quantity * item.UnitPrice;
                if (item.DiscountAmount > gross)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("خصم البند يتجاوز قيمته", ErrorCodes.ValidationError);
                }

                var product = products[item.ProductId];
                replacement.Add(new SalesInvoiceItem
                {
                    TenantId = invoice.TenantId,
                    SalesInvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                    ProductBarCodeId = barcodeId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    UnitCost = product.AveragePrice > 0 ? product.AveragePrice : product.CostPrice,
                    DiscountAmount = item.DiscountAmount,
                    LineTotal = gross - item.DiscountAmount
                });
            }
        }

        // حساب القيم المالية الجديدة للمسودة
        var lines = replacement ?? invoice.Items.ToList();
        var subTotal = lines.Sum(i => i.Quantity * i.UnitPrice);
        var net = lines.Sum(i => i.LineTotal);
        var tax = dto.TaxAmount ?? invoice.TaxAmount;
        var total = net - invoice.DiscountAmount + tax;

        // التحقق من صحة الإجماليات والحدود المسموحة
        if (invoice.DiscountAmount < 0 || invoice.DiscountAmount > net || total < 0 || total > MoneyAmountAttribute.Maximum ||
            subTotal > MoneyAmountAttribute.Maximum)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إجماليات المسودة أو الخصم تتجاوز الحدود المسموحة", ErrorCodes.ValidationError);
        }

        // استبدال البنود القديمة بالجديدة
        if (replacement is not null)
        {
            foreach (var old in invoice.Items.ToList())
            {
                _unitOfWork.SalesInvoices.HardDeleteItem(old);
            }
            invoice.Items = replacement;
        }

        // تحديث الخصائص المالية والبيانات الوصفية
        invoice.SubTotal = subTotal;
        invoice.TaxAmount = tax;
        invoice.TotalAmount = total;
        invoice.RemainingAmount = total;
        if (dto.PaymentMethod.HasValue) invoice.PaymentMethod = dto.PaymentMethod.Value;
        invoice.Notes = dto.Notes;

        // تحديث الفاتورة وحفظها
        _unitOfWork.SalesInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        // جلب الفاتورة المحدثة وإرجاعها
        var updated = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct) ?? invoice;
        return ServiceResult<SalesInvoiceResponseDto>.Success(_mapper.Map<SalesInvoiceResponseDto>(updated));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default)
    {
        // استعلام الفاتورة ككيان متتبع للتعديل
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // حصر التعديل المباشر للحالة في الإلغاء أو الإبطال فقط
        if (status is not InvoiceStatus.Cancelled and not InvoiceStatus.Voided)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيير حالة الفاتورة المرحلة غير مسموح مباشرة؛ المتاح هنا هو الإلغاء فقط", ErrorCodes.ValidationError);
        }

        // التحقق من أن الفاتورة ليست ملغاة مسبقاً
        bool isCancelling = (status == InvoiceStatus.Cancelled || status == InvoiceStatus.Voided) &&
                            (invoice.Status != InvoiceStatus.Cancelled && invoice.Status != InvoiceStatus.Voided);
        if (!isCancelling)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.ValidationError);
        }

        // التحقق مما إذا كانت الفاتورة قد تم ترحيلها مسبقاً
        var wasPosted = invoice.Status != InvoiceStatus.Draft;

        // منع إلغاء فاتورة مرتبطة بمرتجعات قائمة
        if (wasPosted && await _unitOfWork.SalesReturns.HasInvoiceReferencesAsync(id, ct))
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن إلغاء فاتورة لها مرتجعات مرتبطة؛ اعكس المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        // جلب حساب العميل في حال وجوده
        Customer? customer = invoice.CustomerId.HasValue
            ? await _unitOfWork.Customers.GetByIdForUpdateAsync(invoice.CustomerId.Value, ct)
            : null;

        // الخزينة المعتمدة لعكس الدفعة
        Safe? paymentSafe = null;

        // فحص وجود حركات دفع مسجلة لنقاط البيع
        var hasPaymentMovements = _posPayments is not null && await _posPayments.HasPaymentsAsync(invoice.Id, ct);

        // معالجة الخزينة لعكس الفواتير التقليدية غير المرتبطة بمدفوعات POS
        if (wasPosted && invoice.PaidAmount > 0 && !hasPaymentMovements)
        {
            if (!invoice.PaymentSafeId.HasValue)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن إلغاء فاتورة قديمة مدفوعة قبل تحديد خزينة التسوية لها", ErrorCodes.ValidationError);
            }

            var safeResult = await _financialPosting.ResolveSafeAsync(invoice.BranchId, invoice.PaymentSafeId, invoice.PaidAmount, outbound: true, ct);
            if (!safeResult.IsSuccess)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            }
            paymentSafe = safeResult.Data;
        }

        // بدء معاملة قاعدة بيانات لعكس الفاتورة
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // عكس حركات مدفوعات نقطة البيع في حال وجودها
            if (wasPosted && hasPaymentMovements)
            {
                var reversal = await _posPayments!.ReverseAsync(invoice, FinancialDocumentType.SalesInvoice, invoice.Id, invoice.PaidAmount, ct);
                if (!reversal.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    return ServiceResult<SalesInvoiceResponseDto>.Failure(reversal.ErrorMessage!, reversal.ErrorCode);
                }
            }

            // تعيين حالة الفاتورة الجديدة (ملغاة)
            invoice.Status = status;

            // استعادة الكميات المباعة إلى رصيد المخزون في الصالة أو المستودع
            if (wasPosted && isCancelling && invoice.Items != null && invoice.Items.Any())
            {
                var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(invoice.WarehouseId, ct);
                if (warehouse != null)
                {
                    if (warehouse.Type == WarehouseType.Show)
                    {
                        var productIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
                        var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);
                        foreach (var item in invoice.Items)
                        {
                            if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                            {
                                stock.Quantity = checked(stock.Quantity + item.Quantity);
                            }
                        }
                    }
                    else if (warehouse.Type == WarehouseType.Storge)
                    {
                        var barcodeIds = invoice.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                        if (barcodeIds.Count > 0)
                        {
                            var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                            foreach (var item in invoice.Items)
                            {
                                if (item.ProductBarCodeId.HasValue && stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                                {
                                    stock.Quantity = checked(stock.Quantity + item.Quantity);
                                }
                            }
                        }
                    }
                }
            }

            // عكس قيود حساب العميل
            if (wasPosted && customer is not null)
            {
                // عكس الدفعة المسددة
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    invoice.PaidAmount,
                    CustomerTransactionType.Reversal,
                    invoice.BranchId,
                    FinancialDocumentType.SalesInvoice,
                    invoice.Id,
                    DateTime.UtcNow,
                    invoice.PaymentMethod == PaymentMethod.Multiple ? null : invoice.PaymentMethod,
                    $"عكس دفعة فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}",
                    ct);

                // عكس إجمالي الفاتورة
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    -invoice.TotalAmount,
                    CustomerTransactionType.Reversal,
                    invoice.BranchId,
                    FinancialDocumentType.SalesInvoice,
                    invoice.Id,
                    DateTime.UtcNow,
                    invoice.PaymentMethod == PaymentMethod.Multiple ? null : invoice.PaymentMethod,
                    $"عكس فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}",
                    ct);
            }

            // عكس رصيد الخزينة للجزء النقدي
            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(
                    paymentSafe,
                    -invoice.PaidAmount,
                    FinancialDocumentType.SalesInvoice,
                    invoice.Id,
                    DateTime.UtcNow,
                    $"رد تحصيل فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}",
                    ct);
            }

            // حفظ التغييرات واعتماد المعاملة
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيرت الفاتورة أو أرصدتها أثناء الإلغاء؛ أعد تحميلها قبل إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }
        catch (OverflowException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إلغاء الفاتورة يتجاوز سعة المخزون المسموحة", ErrorCodes.ValidationError);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // جلب الفاتورة المحدثة وإرجاعها
        var updated = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);
        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام الفاتورة ككيان متتبع للحذف
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة لحماية التكامل المرجعي
        bool hasReturns = await _unitOfWork.SalesReturns.HasInvoiceReferencesAsync(id, ct);
        if (hasReturns)
        {
            return ServiceResult.Failure("لا يمكن حذف فاتورة لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        // منع حذف فاتورة مرحلة لحفظ سجل التدقيق المالي
        if (invoice.Status != InvoiceStatus.Draft)
        {
            return ServiceResult.Failure("لا يجوز حذف فاتورة مرحلة حفاظاً على سجل التدقيق؛ استخدم الإلغاء بدلاً من الحذف", ErrorCodes.ValidationError);
        }

        // تطبيق الحذف المنطقي للفاتورة
        _unitOfWork.SalesInvoices.SoftDelete(invoice);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
