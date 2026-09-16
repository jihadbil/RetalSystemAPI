using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة استيراد وتصدير الأصناف والباركودات والتصنيفات والوحدات عبر ملفات Excel.
/// </summary>
public interface IProductExcelService
{
    /// <summary>
    /// تصدير كافة الأصناف والباركودات والتصنيفات إلى ملف Excel (بصيغة ورقة واحدة مسطحة أو 3 أوراق عمل).
    /// </summary>
    Task<byte[]> ExportProductsToExcelAsync(bool singleSheetFormat = false, CancellationToken ct = default);

    /// <summary>
    /// تنزيل قالب Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات توضيحية (بصيغة ورقة واحدة مسطحة أو 3 أوراق عمل).
    /// </summary>
    Task<byte[]> DownloadTemplateAsync(bool singleSheetFormat = true, CancellationToken ct = default);

    /// <summary>
    /// فحص ومعاينة ملف Excel قبل الاستيراد الفعلي (Dry-Run Preview) للتأكد من سلامة البيانات وعرض التنبيهات والأخطاء.
    /// </summary>
    Task<ServiceResult<ProductExcelValidationResultDto>> ValidateExcelAsync(Stream excelStream, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات والوحدات والمخزون من ملف Excel (يتعرف تلقائياً على الملف بصيغة ورقة واحدة أو 3 أوراق).
    /// </summary>
    Task<ServiceResult<ProductImportResultDto>> ImportProductsFromExcelAsync(Stream excelStream, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);

    /// <summary>
    /// توليد ملف Excel يحتوي على الأسطر المرفوضة فقط مع عمود لسبب الرفض لإعادة تصحيحها ورفعها.
    /// </summary>
    Task<byte[]> GenerateFailedRowsExcelAsync(List<FailedRowDetailsDto> failedRows, CancellationToken ct = default);
}
