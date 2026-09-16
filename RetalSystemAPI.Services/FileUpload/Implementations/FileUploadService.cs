using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.FileUpload.Interfaces;

namespace RetalSystemAPI.Services.FileUpload.Implementations;

/// <summary>
/// تنفيذ خدمة رفع الملفات لحفظ الصور على الخادم المحلي داخل wwwroot/uploads مقسمة حسب المستأجرين.
/// </summary>
public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentTenantService _tenantService;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    /// <summary>
    /// تهيئة خدمة رفع الملفات مع حقن بيئة الاستضافة وخدمة المستأجر الحالي.
    /// </summary>
    public FileUploadService(IWebHostEnvironment environment, ICurrentTenantService tenantService)
    {
        _environment = environment;
        _tenantService = tenantService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return ServiceResult<string>.Failure("الملف المرفق فارغ أو غير موجود", ErrorCodes.ValidationError);
        }

        if (!IsValidImageExtension(file.FileName))
        {
            return ServiceResult<string>.Failure("امتداد الملف غير مسموح به. الامتدادات المسموحة: jpg, jpeg, png, webp", ErrorCodes.InvalidFileType);
        }

        if (!IsWithinSizeLimit(file.Length))
        {
            return ServiceResult<string>.Failure("حجم الملف يتجاوز الحد الأقصى المسموح به (5 ميجابايت)", ErrorCodes.FileTooLarge);
        }

        try
        {
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var tenantId = _tenantService.TenantId != Guid.Empty ? _tenantService.TenantId.ToString() : "global";
            var uploadsDirectory = Path.Combine(webRootPath, "uploads", subfolder, tenantId);

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var fullPath = Path.Combine(uploadsDirectory, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            var relativeUrl = $"/uploads/{subfolder}/{tenantId}/{uniqueFileName}";
            return ServiceResult<string>.Success(relativeUrl);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure($"حدث خطأ أثناء رفع الملف: {ex.Message}", ErrorCodes.UploadFailed);
        }
    }

    /// <inheritdoc />
    public Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.FromResult(ServiceResult.Success());
        }

        try
        {
            var webRootPath = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // تحويل الرابط النسبي إلى مسار ملف محلي
            var normalizedUrl = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, normalizedUrl);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.FromResult(ServiceResult.Success());
        }
        catch
        {
            return Task.FromResult(ServiceResult.Success()); // عدم رمي خطأ عند فشل حذف ملف قد يكون محذوفاً بالفعل
        }
    }

    /// <inheritdoc />
    public bool IsValidImageExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }

    /// <inheritdoc />
    public bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024)
    {
        return fileSizeBytes <= maxSizeBytes;
    }
}
