using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.FileUpload.Interfaces;

/// <summary>
/// واجهة خدمة رفع وإدارة الملفات المحلية على السيرفر.
/// </summary>
public interface IFileUploadService
{
    Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default);
    bool IsValidImageExtension(string fileName);
    bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024);
}
