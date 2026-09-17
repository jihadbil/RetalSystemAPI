using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// مواصفة جلب جميع باركودات صنف محدد مع المنتج وصوره.
/// </summary>
public class ProductBarCodesByProductSpec : BaseSpecification<ProductBarCode>
{
    /// <summary>
    /// تهيئة مواصفة باركودات الصنف بالمعرف وتضمين المنتج والصور.
    /// </summary>
    /// <param name="productId">معرف المنتج</param>
    public ProductBarCodesByProductSpec(Guid productId) : base(b => b.ProductId == productId)
    {
        // تضمين بيانات المنتج التابع له الباركود
        AddInclude(b => b.Product!);
        // تضمين الصور المخصصة لهذا الباركود أو النكهة
        AddInclude(b => b.ProductImages);
    }
}

/// <summary>
/// مواصفة جلب كافة الباركودات مع البحث بالرقم أو العنوان أو اسم المنتج
/// (تضمّن المنتج وصوره — حقل Images في الاستجابة يعتمد عليهما).
/// </summary>
public class AllProductBarCodesSpec : BaseSpecification<ProductBarCode>
{
    /// <summary>
    /// تهيئة مواصفة البحث الشامل في الباركودات بالرقم أو العنوان أو اسم المنتج.
    /// </summary>
    /// <param name="search">نص البحث الاختياري</param>
    public AllProductBarCodesSpec(string? search = null)
        : base(b => string.IsNullOrWhiteSpace(search) ||
                   b.BarCode.Contains(search) ||
                   b.Title.Contains(search) ||
                   (b.Product != null && b.Product.Name.Contains(search)))
    {
        // تضمين المنتج
        AddInclude(b => b.Product!);
        // تضمين الصور
        AddInclude(b => b.ProductImages);
    }
}

/// <summary>
/// تنفيذ خدمة إدارة باركودات ونكهات المنتجات وتحديث أرصدة التخزين التلقائية.
/// </summary>
public class ProductBarCodeService : IProductBarCodeService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة الباركود مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    public ProductBarCodeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام الباركودات الشاملة مع البحث
        var spec = new AllProductBarCodesSpec(search);
        // جلب قائمة الباركودات المطابقة
        var barCodes = await _unitOfWork.ProductBarCodes.FindAsync(spec, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);

        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام باركودات المنتج المحدد
        var spec = new ProductBarCodesByProductSpec(productId);
        // جلب باركودات ونكهات المنتج
        var barCodes = await _unitOfWork.ProductBarCodes.FindAsync(spec, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);

        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(
        Guid productId,
        CreateProductBarCodeDto dto,
        CancellationToken ct = default)
    {
        // التحقق من صحة وجود المنتج المستهدف
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == productId, ct);
        // في حال عدم وجود المنتج
        if (!productExists)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductBarCodeResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من عدم استخدام كود الباركود مسبقاً في النظام
        bool barCodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.BarCode == dto.BarCode, ct);
        // في حال وجود الباركود
        if (barCodeExists)
        {
            // إرجاع خطأ تكرار الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        // تحويل بيانات الإدخال إلى كيان ProductBarCode
        var barCodeEntity = _mapper.Map<ProductBarCode>(dto);
        // ربط معرف المنتج بالباركود
        barCodeEntity.ProductId = productId;

        // إضافة الباركود إلى المستودع
        await _unitOfWork.ProductBarCodes.AddAsync(barCodeEntity, ct);
        // حفظ التغييرات لتوليد معرف الباركود الجديد
        await _unitOfWork.SaveChangesAsync(ct);

        // توليد سجل مخزون التخزين لكل مخزن تخزين قائم لهذه النكهة الجديدة
        var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
        // المرور على مخازن التخزين القائمة
        foreach (var storgeWh in storgeWarehouses)
        {
            // إنشاء سجل رصيد المخزن للباركود الجديد
            var storgeStock = new StorgeStock
            {
                WarehouseId = storgeWh.Id,
                ProductBarcodeId = barCodeEntity.Id,
                Quantity = dto.InitialQuantity,
                MinStockLevel = 0
            };
            // إضافة سجل الرصيد المخزني
            await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
        }
        // حفظ سجلات الأرصدة المخزنية في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان إلى كائن الاستجابة
        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(
        Guid barCodeId,
        UpdateProductBarCodeDto dto,
        CancellationToken ct = default)
    {
        // استعلام الباركود المطلوب تعديله بالمعرف
        var barCodeEntity = await _unitOfWork.ProductBarCodes.GetByIdAsync(barCodeId, ct);
        // التحقق من وجود الباركود
        if (barCodeEntity is null)
        {
            // إرجاع خطأ بعدم وجود الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // التحقق من عدم استخدام كود الباركود الجديد لدى باركود آخر
        bool barCodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.BarCode == dto.BarCode && b.Id != barCodeId, ct);
        // في حال تكرار الباركود
        if (barCodeExists)
        {
            // إرجاع خطأ تكرار الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        // تحديث رقم الباركود
        barCodeEntity.BarCode = dto.BarCode;
        // تحديث عنوان أو نكهة الباركود
        barCodeEntity.Title = dto.Title;
        // تحديث الوصف
        barCodeEntity.Description = dto.Description;

        // تحديث السجل في المستودع
        _unitOfWork.ProductBarCodes.Update(barCodeEntity);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default)
    {
        // استعلام الباركود المراد حذفه
        var barCodeEntity = await _unitOfWork.ProductBarCodes.GetByIdAsync(barCodeId, ct);
        // التحقق من وجود الباركود
        if (barCodeEntity is null)
        {
            // إرجاع خطأ بعدم وجود الباركود
            return ServiceResult.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // إجراء الحذف الصلب للباركود
        _unitOfWork.ProductBarCodes.HardDelete(barCodeEntity);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
