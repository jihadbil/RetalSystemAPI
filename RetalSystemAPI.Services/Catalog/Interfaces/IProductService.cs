using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة المنتجات والأصناف لإدارة كتالوج المنتجات، الأسعار، الوحدات، والباركودات وتغذية الأرصدة الافتتاحية.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// جلب قائمة ملخصة بجميع المنتجات مع إمكانية الفلترة بتصنيف محدد.
    /// </summary>
    /// <param name="categoryId">معرف التصنيف للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة بملخصات المنتجات</returns>
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> GetAllAsync(Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل صنف كاملة مع باركوداته ووحداته وصوره وتفاصيل التصنيف.
    /// </summary>
    /// <param name="id">معرف المنتج الفريد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المنتج التفصيلية</returns>
    Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من الأصناف مع الترقيم وخيارات الفلترة بالتصنيف.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة</param>
    /// <param name="pageSize">عدد المنتجات في الصفحة</param>
    /// <param name="categoryId">معرف التصنيف للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صفحة نتائج تحتوي على عناصر المنتجات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>
    /// البحث السريع في الأصناف بالاسم أو الوصف أو الباركود مع ترتيب النتائج بالأولوية.
    /// </summary>
    /// <param name="query">كلمة أو رقم البحث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بالمنتجات المطابقة للبحث</returns>
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// جلب صنف كامل بواسطة قراءة أي باركود مرتبط به.
    /// </summary>
    /// <param name="barCode">رقم الباركود المقروء</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المنتج المرتبط بهذا الباركود</returns>
    Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);

    /// <summary>
    /// إنشاء صنف جديد مع باركوداته ووحداته وتوليد أرصدته الافتتاحية في المخازن والصالات.
    /// </summary>
    /// <param name="dto">بيانات المنتج ووحداته وباركوداته وأرصدته الافتتاحية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المنتج المنشأ حديثاً</returns>
    Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات الصنف وأسعاره وإدارة إضافة وحذف وحداته وباركوداته مع حماية الحركات المخزنية.
    /// </summary>
    /// <param name="id">معرف المنتج المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة للمنتج</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المنتج المحدثة</returns>
    Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف الصنف منطقياً (Soft Delete) بعد التحقق من عدم وجود حركات مبيعات أو مشتريات أو أرصدة قائمة.
    /// </summary>
    /// <param name="id">معرف المنتج المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
