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
/// تنفيذ خدمة صور المنتجات.
/// </summary>
public class ProductImageService : IProductImageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _fileUploadService;

    public ProductImageService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileUploadService fileUploadService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileUploadService = fileUploadService;
    }

    public async Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        var images = await _unitOfWork.ProductImages.FindAsync(i => i.ProductId == productId, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductImageResponseDto>>(images);

        return ServiceResult<IReadOnlyList<ProductImageResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(
        Guid productId,
        CreateProductImageDto dto,
        CancellationToken ct = default)
    {
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == productId, ct);
        if (!productExists)
        {
            return ServiceResult<ProductImageResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        if (dto.BarcodeId.HasValue)
        {
            bool barcodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.Id == dto.BarcodeId.Value, ct);
            if (!barcodeExists)
            {
                return ServiceResult<ProductImageResponseDto>.Failure("الباركود المحدد غير موجود", ErrorCodes.BarCodeNotFound);
            }
        }

        bool hasImages = await _unitOfWork.ProductImages.ExistsAsync(i => i.ProductId == productId, ct);

        var imageEntity = _mapper.Map<ProductImage>(dto);
        imageEntity.ProductId = productId;
        imageEntity.IsDefault = dto.IsDefault || !hasImages; // تعيين افتراضية إذا كانت الصورة الأولى

        await _unitOfWork.ProductImages.AddAsync(imageEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<ProductImageResponseDto>(imageEntity);
        return ServiceResult<ProductImageResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default)
    {
        var imageEntity = await _unitOfWork.ProductImages.GetByIdAsync(imageId, ct);
        if (imageEntity is null)
        {
            return ServiceResult.Failure("الصورة غير موجودة", ErrorCodes.ImageNotFound);
        }

        // حذف الملف الفعلي من القرص المحلي
        await _fileUploadService.DeleteAsync(imageEntity.ImageUrl, ct);

        _unitOfWork.ProductImages.HardDelete(imageEntity);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default)
    {
        var targetImage = await _unitOfWork.ProductImages.GetByIdAsync(imageId, ct);
        if (targetImage is null)
        {
            return ServiceResult.Failure("الصورة غير موجودة", ErrorCodes.ImageNotFound);
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var allImages = await _unitOfWork.ProductImages.FindAsync(i => i.ProductId == targetImage.ProductId, ct);
            foreach (var img in allImages)
            {
                img.IsDefault = (img.Id == imageId);
                _unitOfWork.ProductImages.Update(img);
            }

            await _unitOfWork.CommitTransactionAsync(ct);
            return ServiceResult.Success();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
