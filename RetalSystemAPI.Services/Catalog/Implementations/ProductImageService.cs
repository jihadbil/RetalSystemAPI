using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.FileUpload.Interfaces;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة صور المنتجات والتحكم بالصور الافتراضية وحذف الملفات المرفوعة من الخادم.
/// </summary>
public class ProductImageService : IProductImageService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    // خدمة رفع وإدارة الملفات على الخادم
    private readonly IFileUploadService _fileUploadService;

    /// <summary>
    /// تهيئة خدمة صور المنتجات مع حقن وحدة العمل، والمحول، وخدمة رفع الملفات.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    /// <param name="fileUploadService">خدمة إدارة وحذف الملفات على القرص</param>
    public ProductImageService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileUploadService fileUploadService)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
        // تعيين مرجع خدمة رفع الملفات
        _fileUploadService = fileUploadService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        // استعلام كافة الصور التابعة للمنتج بالمعرف
        var images = await _unitOfWork.ProductImages.FindAsync(i => i.ProductId == productId, ct);
        // تحويل قائمة الكيانات إلى قائمة كائنات نقل البيانات DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductImageResponseDto>>(images);

        // إرجاع النتيجة الناجحة مع قائمة الصور
        return ServiceResult<IReadOnlyList<ProductImageResponseDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(
        Guid productId,
        CreateProductImageDto dto,
        CancellationToken ct = default)
    {
        // التحقق من صحة وجود المنتج المستهدف
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == productId, ct);
        // في حال عدم وجود المنتج
        if (!productExists)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductImageResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من صحة الباركود في حال تم تخصيص الصورة لنكهة/باركود محدد
        if (dto.BarcodeId.HasValue)
        {
            // الاستعلام عن وجود الباركود
            bool barcodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.Id == dto.BarcodeId.Value, ct);
            // في حال عدم وجود الباركود
            if (!barcodeExists)
            {
                // إرجاع خطأ بعدم وجود الباركود
                return ServiceResult<ProductImageResponseDto>.Failure("الباركود المحدد غير موجود", ErrorCodes.BarCodeNotFound);
            }
        }

        // فحص ما إذا كان للمنتج أي صور سابقة
        bool hasImages = await _unitOfWork.ProductImages.ExistsAsync(i => i.ProductId == productId, ct);

        // تحويل بيانات الإدخال إلى كيان ProductImage
        var imageEntity = _mapper.Map<ProductImage>(dto);
        // ربط معرف المنتج
        imageEntity.ProductId = productId;
        // تعيين الصورة كافتراضية إذا تم طلب ذلك أو إذا كانت هي الصورة الأولى للمنتج
        imageEntity.IsDefault = dto.IsDefault || !hasImages;

        // إضافة كيان الصورة إلى المستودع
        await _unitOfWork.ProductImages.AddAsync(imageEntity, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان إلى كائن الاستجابة المنقول
        var responseDto = _mapper.Map<ProductImageResponseDto>(imageEntity);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductImageResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default)
    {
        // استعلام الصورة بالمعرف
        var imageEntity = await _unitOfWork.ProductImages.GetByIdAsync(imageId, ct);
        // التحقق من وجود سجل الصورة
        if (imageEntity is null)
        {
            // إرجاع خطأ بعدم وجود الصورة
            return ServiceResult.Failure("الصورة غير موجودة", ErrorCodes.ImageNotFound);
        }

        // حذف الملف الفعلي للصورة من القرص المحلي عبر خدمة الملفات
        await _fileUploadService.DeleteAsync(imageEntity.ImageUrl, ct);

        // حذف سجل الصورة نهائياً من قاعدة البيانات
        _unitOfWork.ProductImages.HardDelete(imageEntity);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default)
    {
        // استعلام الصورة المستهدفة بالمعرف
        var targetImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId, ct);
        // التحقق من وجود الصورة
        if (targetImage is null)
        {
            // إرجاع خطأ بعدم وجود الصورة
            return ServiceResult.Failure("الصورة غير موجودة", ErrorCodes.ImageNotFound);
        }

        // بدء معاملة قاعدة بيانات لضمان ضبط حالة الصورة الافتراضية بشكل متزامن
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // جلب كافة الصور المرتبطة بنفس المنتج
            var allImages = await _unitOfWork.ProductImages.FindAsync(i => i.ProductId == targetImage.ProductId, ct);
            // تعديل حالة الافتراضية: جعل الصورة المختارة فقط افتراضية
            foreach (var img in allImages)
            {
                // تعيين حالة الافتراضية
                img.IsDefault = (img.Id == imageId);
                // تحديث السجل في المستودع
                _unitOfWork.ProductImages.Update(img);
            }

            // تأكيد وحفظ المعاملة
            await _unitOfWork.CommitTransactionAsync(ct);
            // إرجاع نتيجة النجاح
            return ServiceResult.Success();
        }
        catch
        {
            // التراجع عن المعاملة في حال حدوث استثناء
            await _unitOfWork.RollbackTransactionAsync(ct);
            // إعادة رمي الاستثناء
            throw;
        }
    }
}
