using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة الباركودات والنكهات المتعددة للأصناف، وإدارة العناوين الفرعية والأوصاف.
/// </summary>
public interface IProductBarCodeService
{
    /// <summary>جلب كافة الباركودات مع إمكانية البحث بالرقم أو الاسم</summary>
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetAllAsync(string? search = null, CancellationToken ct = default);

    /// <summary>جلب جميع الباركودات والنكهات التابعة لصنف محدد</summary>
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>إضافة باركود/نكهة جديدة لصنف وتأسيس رصيدها في مستودعات التخزين</summary>
    Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات باركود موجود</summary>
    Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default);

    /// <summary>حذف باركود لصنف بعد التحقق من عدم وجود حركات أو أرصدة مرتبطة به</summary>
    Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default);
}
