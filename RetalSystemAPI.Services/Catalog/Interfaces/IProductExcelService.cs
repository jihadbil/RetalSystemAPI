using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة استيراد وتصدير الأصناف والباركودات والتصنيفات عبر ملفات Excel.
/// </summary>
public interface IProductExcelService
{
    /// <summary>
    /// تصدير كافة الأصناف والباركودات والتصنيفات إلى ملف Excel يحوي 3 اوراق عمل.
    /// </summary>
    Task<byte[]> ExportProductsToExcelAsync(CancellationToken ct = default);

    /// <summary>
    /// تنزيل قالب Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات توضيحية.
    /// </summary>
    Task<byte[]> DownloadTemplateAsync(CancellationToken ct = default);

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات من ملف Excel بحسب هيكلية 3 اوراق عمل.
    /// </summary>
    Task<ServiceResult<ProductImportResultDto>> ImportProductsFromExcelAsync(Stream excelStream, CancellationToken ct = default);
}
