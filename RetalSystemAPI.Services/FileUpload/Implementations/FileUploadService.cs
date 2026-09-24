using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.FileUpload.Interfaces;

namespace RetalSystemAPI.Services.FileUpload.Implementations;

/// <summary>
/// تنفيذ خدمة رفع الملفات لحفظ الصور على الخادم المحلي داخل wwwroot/uploads مقسمة حسب المستأجرين.
/// </summary>
public class FileUploadService : IFileUploadService
{
    // بيئة الاستضافة لتحديد مسار الجذر wwwroot
    private readonly IWebHostEnvironment _environment;

    // خدمة المستأجر الحالي لتحديد مجلد العزل الخاص بالمستأجر
    private readonly ICurrentTenantService _tenantService;
    private readonly ILogger<FileUploadService> _logger;

    // قائمة الامتدادات المسموح بها لرفع الصور
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly HashSet<string> AllowedSubfolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "products",
        "tenants/logos"
    };

    /// <summary>
    /// تهيئة خدمة رفع الملفات مع حقن بيئة الاستضافة وخدمة المستأجر الحالي.
    /// </summary>
    /// <param name="environment">بيئة الاستضافة لتحديد المسارات الفيزيائية</param>
    /// <param name="tenantService">خدمة المستأجر الحالي لعزل مجلدات الملفات</param>
    public FileUploadService(
        IWebHostEnvironment environment,
        ICurrentTenantService tenantService,
        ILogger<FileUploadService> logger)
    {
        // تعيين مرجع بيئة الاستضافة
        _environment = environment;

        // تعيين مرجع خدمة المستأجر الحالي
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default)
    {
        // التحقق من وجود الملف وعدم كونه فارغاً
        if (file is null || file.Length == 0)
        {
            // إرجاع خطأ تحقق عند غياب الملف
            return ServiceResult<string>.Failure("الملف المرفق فارغ أو غير موجود", ErrorCodes.ValidationError);
        }

        // التحقق من صحة امتداد الملف وفق قائمة الامتدادات المعتمدة
        if (!IsValidImageExtension(file.FileName))
        {
            // إرجاع خطأ نوع الملف غير مدعوم
            return ServiceResult<string>.Failure("امتداد الملف غير مسموح به. الامتدادات المسموحة: jpg, jpeg, png, webp", ErrorCodes.InvalidFileType);
        }

        if (!AllowedSubfolders.Contains(subfolder.Replace('\\', '/').Trim('/')))
        {
            return ServiceResult<string>.Failure("مجلد الرفع غير مسموح به", ErrorCodes.ValidationError);
        }

        // التحقق من أن حجم الملف لا يتجاوز الحد المسموح
        if (!IsWithinSizeLimit(file.Length))
        {
            // إرجاع خطأ تجاوز الحد الأقصى للحجم
            return ServiceResult<string>.Failure("حجم الملف يتجاوز الحد الأقصى المسموح به (5 ميجابايت)", ErrorCodes.FileTooLarge);
        }


        if (!await HasValidImageSignatureAsync(file, ct))
        {
            return ServiceResult<string>.Failure("محتوى الملف لا يطابق صورة مدعومة", ErrorCodes.InvalidFileType);
        }

        try
        {
            // استخراج مسار جذر الويب wwwroot من بيئة الاستضافة
            var webRootPath = _environment.WebRootPath;

            // في حال عدم توفر المسار، يتم اعتماده نسبة إلى مجلد العمل الحالي
            if (string.IsNullOrEmpty(webRootPath))
            {
                // دمج مسار wwwroot مع مجلد التشغيل
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // استخراج معرف المستأجر الحالي أو استخدام "global" في حال عدم وجوده
            var tenantId = _tenantService.TenantId != Guid.Empty ? _tenantService.TenantId.ToString() : "global";

            // بناء المسار الفيزيائي الكامل لمجلد التخزين
            var uploadsDirectory = Path.Combine(webRootPath, "uploads", subfolder, tenantId);

            // التحقق من وجود مجلد التخزين وإنشائه إذا لم يكن موجوداً
            if (!Directory.Exists(uploadsDirectory))
            {
                // إنشاء شجرة المجلدات المطلوبة
                Directory.CreateDirectory(uploadsDirectory);
            }

            // استخراج امتداد الملف بحروف صغيرة
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // توليد اسم فريد جديد للملف باستخدام Guid لتفادي تعارض الأسماء
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // دمج المسار الكامل لتخزين الملف
            var fullPath = Path.Combine(uploadsDirectory, uniqueFileName);

            // فتح مجرى كتابة لحفظ محتويات الملف على القرص الصلب
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                // نسخ محتويات الملف المرفوع إلى المجرى
                await file.CopyToAsync(stream, ct);
            }

            // تكوين الرابط النسبي للوصول للملف عبر المتصفح
            var relativeUrl = $"/uploads/{subfolder}/{tenantId}/{uniqueFileName}";

            // إرجاع النتيجة بنجاح مع الرابط النسبي
            return ServiceResult<string>.Success(relativeUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist an uploaded image in {Subfolder}", subfolder);
            return ServiceResult<string>.Failure("تعذر حفظ الصورة المرفوعة", ErrorCodes.UploadFailed);
        }
    }

    /// <inheritdoc />
    public Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        // التحقق من صحة الرابط المدخل
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            // إرجاع نجاح فوري إذا كان الرابط فارغاً
            return Task.FromResult(ServiceResult.Success());
        }

        try
        {
            // استخراج مسار جذر الويب wwwroot
            var webRootPath = _environment.WebRootPath;

            // اعتماده مع المجلد الحالي إذا كان فارغاً
            if (string.IsNullOrEmpty(webRootPath))
            {
                // دمج مسار wwwroot
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // تسوية الرابط وتحويل فواصل الويب إلى فواصل نظام التشغيل
            var normalizedUrl = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            // تكوين المسار الفيزيائي الكامل للملف
            var uploadsRoot = Path.GetFullPath(Path.Combine(webRootPath, "uploads")) + Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(Path.Combine(webRootPath, normalizedUrl));
            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(ServiceResult.Failure("مسار الملف غير صالح", ErrorCodes.ValidationError));
            }

            // التحقق من وجود الملف فيزيائياً على القرص
            if (File.Exists(fullPath))
            {
                // حذف الملف من القرص
                File.Delete(fullPath);
            }

            // إرجاع نجاح العملية
            return Task.FromResult(ServiceResult.Success());
        }
        catch
        {
            // إرجاع نجاح وعدم رمي خطأ عند فشل حذف ملف قد يكون محذوفاً بالفعل
            return Task.FromResult(ServiceResult.Success());
        }
    }

    /// <inheritdoc />
    public bool IsValidImageExtension(string fileName)
    {
        // التحقق من وجود اسم الملف
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        // استخراج الامتداد مع تحويله لحروف صغيرة
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        // التحقق من وجود الامتداد ضمن القائمة المسموحة
        return AllowedExtensions.Contains(ext);
    }

    /// <inheritdoc />
    public bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024)
    {
        // مقارنة حجم الملف بالحد الأقصى المسموح به
        return fileSizeBytes <= maxSizeBytes;
    }

    private static async Task<bool> HasValidImageSignatureAsync(IFormFile file, CancellationToken ct)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), ct);
        if (bytesRead < 3) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 &&
                      header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                      header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            ".webp" => bytesRead >= 12 &&
                       header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F' &&
                       header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
            _ => false
        };
    }
}
