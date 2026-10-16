using RetalSystemAPI.Models.DTOs.Common;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;
/// <summary>
/// تنفيذ خدمة وحدات المنتجات ومعاملات التحويل والوحدات الافتراضية.
/// </summary>
public class ProductUnitService : IProductUnitService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة وحدات المنتجات مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name = "mapper">خدمة تحويل النماذج</param>
    public ProductUnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ProductUnitResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام وحدات المنتج بالمعرف
        // جلب قائمة وحدات المنتج من المستودع
        var productUnits = await _unitOfWork.Products.GetUnitsWithDetailsAsync(productId, ct);
        // تحويل الكيانات إلى قائمة كائنات نقل البيانات DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductUnitResponseDto>>(productUnits);
        // إرجاع النتيجة الناجحة مع قائمة وحدات المنتج
        return ServiceResult<IReadOnlyList<ProductUnitResponseDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductUnitResponseDto>> AddUnitToProductAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProductUnitResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من صحة وجود المنتج المستهدف
        bool productExists = await _unitOfWork.Products.ExistsByIdAsync(productId, ct);
        // في حال عدم وجود المنتج
        if (!productExists)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductUnitResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من وجود وحدة القياس العامة في النظام
        bool unitExists = await _unitOfWork.Units.ExistsByIdAsync(dto.UnitId, ct);
        // في حال عدم وجود الوحدة
        if (!unitExists)
        {
            // إرجاع خطأ بعدم وجود الوحدة
            return ServiceResult<ProductUnitResponseDto>.Failure("الوحدة المحددة غير موجودة", ErrorCodes.UnitNotFound);
        }

        // التحقق من عدم إضافة نفس الوحدة للمنتج مسبقاً
        bool duplicate = await _unitOfWork.Products.ProductUnitExistsAsync(productId, dto.UnitId, ct);
        // في حال كانت مضافة مسبقاً
        if (duplicate)
        {
            // إرجاع خطأ تكرار إسناد الوحدة
            return ServiceResult<ProductUnitResponseDto>.Failure("هذه الوحدة مضافة بالفعل للمنتج", ErrorCodes.ProductUnitDuplicate);
        }

        // التحقق من أن معامل التحويل أكبر من الصفر
        if (dto.ConversionFactor <= 0)
        {
            // إرجاع خطأ عدم صحة معامل التحويل
            return ServiceResult<ProductUnitResponseDto>.Failure("معامل التحويل يجب أن يكون أكبر من صفر", ErrorCodes.ValidationError);
        }

        // فحص ما إذا كان للمنتج أي وحدات قياس مسجلة مسبقاً
        bool hasUnits = await _unitOfWork.Products.HasUnitsAsync(productId, ct);
        // تحويل بيانات الإدخال إلى كيان ProductUnit
        var productUnit = _mapper.Map<ProductUnit>(dto);
        // ربط معرف المنتج
        productUnit.ProductId = productId;
        // تعيين الوحدة كافتراضية إذا تم طلب ذلك أو إذا كانت هي الوحدة الأولى للمنتج
        productUnit.IsDefault = dto.IsDefault || !hasUnits;
        // إضافة وحدة المنتج إلى المستودع
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            if (productUnit.IsDefault)
            {
                var defaults = await _unitOfWork.Products.GetDefaultUnitsForUpdateAsync(productId, ct);
                foreach (var unitEntry in defaults)
                    unitEntry.IsDefault = false;
                await _unitOfWork.SaveChangesAsync(ct);
            }

            await _unitOfWork.Products.AddUnitAsync(productUnit, ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // جلب بيانات الوحدة لربطها في كائن الاستجابة
        var unit = await _unitOfWork.Units.GetByIdForUpdateAsync(dto.UnitId, ct);
        // إسناد الوحدة المرجعية
        productUnit.Unit = unit;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<ProductUnitResponseDto>(productUnit);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductUnitResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> RemoveUnitFromProductAsync(Guid productUnitId, CancellationToken ct = default)
    {
        // استعلام سجل وحدة المنتج بالمعرف
        var productUnit = await _unitOfWork.Products.GetUnitByIdAsync(productUnitId, ct);
        // التحقق من وجود السجل
        if (productUnit is null)
        {
            // إرجاع خطأ بعدم وجود وحدة المنتج
            return ServiceResult.Failure("وحدة المنتج غير موجودة", ErrorCodes.ProductUnitNotFound);
        }

        // إجراء الحذف الصلب لارتباط وحدة المنتج
        _unitOfWork.Products.HardDeleteUnit(productUnit);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> SetDefaultUnitAsync(Guid productUnitId, CancellationToken ct = default)
    {
        // استعلام وحدة المنتج المستهدفة
        var targetUnit = await _unitOfWork.Products.GetUnitByIdAsync(productUnitId, ct);
        // التحقق من وجود وحدة المنتج
        if (targetUnit is null)
        {
            // إرجاع خطأ بعدم وجود السجل
            return ServiceResult.Failure("وحدة المنتج غير موجودة", ErrorCodes.ProductUnitNotFound);
        }

        // بدء معاملة مالية/قاعدة بيانات لضمان تعديل الافتراضية بشكل متزامن
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // جلب كافة الوحدات المرتبطة بنفس المنتج
            var allUnits = await _unitOfWork.Products.GetUnitsForUpdateAsync(targetUnit.ProductId, ct);
            // تعديل الافتراضية: جعل الوحدة المستهدفة هي الافتراضية وتجريد البقية
            foreach (var pu in allUnits)
            {
                // تعيين حالة الافتراضية
                pu.IsDefault = false;
                // تحديث السجل في المستودع
                _unitOfWork.Products.UpdateUnit(pu);
            }

            // تأكيد وحفظ المعاملة
            await _unitOfWork.SaveChangesAsync(ct);
            targetUnit.IsDefault = true;
            await _unitOfWork.CommitTransactionAsync(ct);
            // إرجاع نتيجة النجاح
            return ServiceResult.Success();
        }
        catch
        {
            // التراجع عن المعاملة في حال حدوث أي خطأ
            await _unitOfWork.RollbackTransactionAsync(ct);
            // إعادة رمي الاستثناء لمعالجته في المستويات العليا
            throw;
        }
    }
}
