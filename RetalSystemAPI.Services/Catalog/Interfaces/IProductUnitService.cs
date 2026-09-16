using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة إسناد وتعدد وحدات القياس للمنتجات (حبة، كرتون، باكت) وتحديد معاملات التحويل والوحدة الافتراضية.
/// </summary>
public interface IProductUnitService
{
    /// <summary>جلب قائمة بكافة الوحدات المعرفة للمنتج مع معامل التحويل لكل منها</summary>
    Task<ServiceResult<IReadOnlyList<ProductUnitResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>إسناد وحدة قياس جديدة للمنتج مع معامل التحويل وتحديد ما إذا كانت افتراضية</summary>
    Task<ServiceResult<ProductUnitResponseDto>> AddUnitToProductAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default);

    /// <summary>إزالة وحدة قياس مسندة للمنتج</summary>
    Task<ServiceResult> RemoveUnitFromProductAsync(Guid productUnitId, CancellationToken ct = default);

    /// <summary>تعيين وحدة قياس كالوحدة الافتراضية للبيع والعرض للمنتج</summary>
    Task<ServiceResult> SetDefaultUnitAsync(Guid productUnitId, CancellationToken ct = default);
}
