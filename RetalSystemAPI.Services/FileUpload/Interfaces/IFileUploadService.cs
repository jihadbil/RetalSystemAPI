using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.FileUpload.Interfaces;

/// <summary>
/// واجهة خدمة رفع وإدارة الملفات والصور على السيرفر والتحقق من صحة الامتدادات والأحجام المسموحة.
/// </summary>
public interface IFileUploadService
{
    /// <summary>رفع ملف جديد وتخزينه في مسار المستأجر وإرجاع الرابط النسبي للملف</summary>
    Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default);

    /// <summary>حذف ملف فيزيائياً من القرص عبر رابطه النسبي</summary>
    Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default);

    /// <summary>التحقق من توافق امتداد الصورة مع الأنواع المسموحة (jpg, jpeg, png, webp)</summary>
    bool IsValidImageExtension(string fileName);

    /// <summary>التحقق من عدم تجاوز حجم الملف للحد الأقصى المسموح</summary>
    bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024);
}
