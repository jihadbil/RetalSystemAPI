using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.FileUpload.Interfaces;

/// <summary>
/// واجهة خدمة رفع وإدارة الملفات والصور على السيرفر والتحقق من صحة الامتدادات والأحجام المسموحة وتخزينها في مسارات المستأجرين.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// رفع ملف جديد وتخزينه في مسار المستأجر وإرجاع الرابط النسبي للملف على الخادم.
    /// </summary>
    /// <param name="file">الملف المرفوع عبر الطلب</param>
    /// <param name="subfolder">المجلد الفرعي المراد الحفظ فيه (مثل products, tenants)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة الرابط النسبي للملف المرفوع</returns>
    Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default);

    /// <summary>
    /// حذف ملف فيزيائياً من القرص الصلب عبر رابطه النسبي.
    /// </summary>
    /// <param name="fileUrl">الرابط النسبي للملف المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>
    /// التحقق من توافق امتداد الملف مع أنواع الصور المسموحة (jpg, jpeg, png, webp).
    /// </summary>
    /// <param name="fileName">اسم الملف أو مساره</param>
    /// <returns>صحيح إذا كان الامتداد مسموحاً به</returns>
    bool IsValidImageExtension(string fileName);

    /// <summary>
    /// التحقق من عدم تجاوز حجم الملف للحد الأقصى المسموح به.
    /// </summary>
    /// <param name="fileSizeBytes">حجم الملف بالبايت</param>
    /// <param name="maxSizeBytes">الحد الأقصى المسموح بالبايت (الافتراضي 5 ميجابايت)</param>
    /// <returns>صحيح إذا كان الحجم ضمن الحدود المقبولة</returns>
    bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024);
}
