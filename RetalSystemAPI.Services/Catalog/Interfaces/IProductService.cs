using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة المنتجات والأصناف لإدارة كتالوج المنتجات، الأسعار، الوحدات، والباركودات.
/// </summary>
public interface IProductService
{
    /// <summary>جلب قائمة ملخصة بجميع المنتجات مع إمكانية الفلترة بتصنيف محدد</summary>
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> GetAllAsync(Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>جلب تفاصيل صنف كاملة مع باركوداته ووحداته وصوره</summary>
    Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من الأصناف مع الترقيم</summary>
    Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>البحث السريع في الأصناف بالاسم أو الوصف أو الباركود</summary>
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>جلب صنف كامل بواسطة قراءة أي باركود مرتبط به</summary>
    Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);

    /// <summary>إنشاء صنف جديد مع باركوده ووحداته وصوره الأولية</summary>
    Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات الصنف وأسعاره</summary>
    Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);

    /// <summary>حذف الصنف منطقياً (Soft Delete)</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
