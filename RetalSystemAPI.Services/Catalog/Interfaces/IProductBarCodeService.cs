using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة الباركودات والنكهات المتعددة للأصناف، وإدارة العناوين الفرعية والأوصاف وتأسيس أرصدة المخازن.
/// </summary>
public interface IProductBarCodeService
{
    /// <summary>
    /// جلب كافة الباركودات مع إمكانية البحث بالرقم أو العنوان أو اسم المنتج.
    /// </summary>
    /// <param name="search">نص البحث الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>قائمة بالباركودات المطابقة</returns>
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetAllAsync(string? search = null, CancellationToken ct = default);

    /// <summary>
    /// جلب جميع الباركودات والنكهات التابعة لصنف محدد مع الصور المرتبطة.
    /// </summary>
    /// <param name="productId">معرف المنتج</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة باركودات ونكهات المنتج</returns>
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// إضافة باركود/نكهة جديدة لصنف وتأسيس رصيدها في مستودعات التخزين تلقائياً.
    /// </summary>
    /// <param name="productId">معرف المنتج المراد إضافة الباركود له</param>
    /// <param name="dto">بيانات الباركود الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الباركود المضاف حديثاً</returns>
    Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات باركود موجود (الرقم، العنوان، الوصف).
    /// </summary>
    /// <param name="barCodeId">معرف سجل الباركود المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة للباركود</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الباركود بعد التحديث</returns>
    Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف باركود لصنف بعد التحقق من عدم وجود حركات أو أرصدة مرتبطة به.
    /// </summary>
    /// <param name="barCodeId">معرف الباركود المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default);
}
