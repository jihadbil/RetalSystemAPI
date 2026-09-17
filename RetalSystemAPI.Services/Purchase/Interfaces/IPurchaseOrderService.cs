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
    /// <summary>
    /// جلب تفاصيل أمر شراء محدد بالمعرف مع كافة بنوده وتفاصيل المورد والمستودع.
    /// </summary>
    /// <param name="id">معرف أمر الشراء الفريد</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات أمر الشراء الكاملة</returns>
    Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة أوامر الشراء مع إمكانية الفلترة بالفرع والمستودع والحالة.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name="status">حالة أمر الشراء (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات أوامر الشراء</returns>
    Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أوامر الشراء مع الترقيم وخيارات الفلترة والبحث.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة الحالية</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name="status">حالة أمر الشراء (اختياري)</param>
    /// <param name="search">نص البحث في رقم أمر الشراء (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صفحة نتائج تحتوي على عناصر أوامر الشراء وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default);

    /// <summary>
    /// إنشاء أمر شراء جديد مع التحقق من عدم تكرار رقم الطلبية وحساب الإجماليات.
    /// </summary>
    /// <param name="dto">بيانات إنشاء أمر الشراء وبنوده</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر الشراء المنشأ حديثاً</returns>
    Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات وبنود أمر شراء موجود (فقط إذا كان في حالة مسودة/معلق).
    /// </summary>
    /// <param name="id">معرف أمر الشراء المراد تعديله</param>
    /// <param name="dto">البيانات والبنود الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر الشراء المحدثة</returns>
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default);

    /// <summary>
    /// تغيير حالة أمر الشراء وإجراء التسويات المخزنية وتوليد الفاتورة عند الاستلام.
    /// </summary>
    /// <param name="id">معرف أمر الشراء</param>
    /// <param name="status">الحالة الجديدة المطلوب تطبيقها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر الشراء بعد تعديل الحالة</returns>
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default);

    /// <summary>
    /// حذف أمر شراء منطقياً من النظام.
    /// </summary>
    /// <param name="id">معرف أمر الشراء المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
