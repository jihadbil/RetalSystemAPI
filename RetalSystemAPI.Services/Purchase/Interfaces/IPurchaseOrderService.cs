using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

/// <summary>
/// واجهة خدمة أوامر الشراء وإدارة بنود الطلبيات وتتبع حالتها (معلق، معتمد، ملغى، مكتمل).
/// </summary>
public interface IPurchaseOrderService
{
    /// <summary>جلب تفاصيل أمر شراء محدد بالمعرف مع كافة بنوده وتفاصيل المورد والمستودع</summary>
    Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة أوامر الشراء مع إمكانية الفلترة بالفرع والمستودع والحالة</summary>
    Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من أوامر الشراء مع الترقيم وخيارات الفلترة والبحث</summary>
    Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default);

    /// <summary>إنشاء أمر شراء جديد مع التحقق من عدم تكرار رقم الطلبية وحساب الإجماليات</summary>
    Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات وبنود أمر شراء موجود (فقط إذا كان في حالة مسودة/معلق)</summary>
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default);

    /// <summary>تغيير حالة أمر الشراء</summary>
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default);

    /// <summary>حذف أمر شراء منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
