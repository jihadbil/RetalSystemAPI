using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة استيراد وتصدير الأصناف والباركودات والتصنيفات والوحدات والمخزون عبر ملفات Excel.
/// تدعم نمط الشيت الواحد المسطح (Single-Sheet Flat) ونمط أوراق العمل المتعددة (Multi-Sheet).
/// </summary>
public interface IProductExcelService
{
    /// <summary>
    /// تصدير كافة الأصناف والباركودات والتصنيفات إلى ملف Excel (بصيغة ورقة واحدة مسطحة أو 3 أوراق عمل).
    /// </summary>
    /// <param name="singleSheetFormat">تحديد ما إذا كان التصدير بصيغة ورقة واحدة مدمجة أو 3 أوراق عمل</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>مصفوفة بايتات تمثل ملف إكسيل المصدر</returns>
    Task<byte[]> ExportProductsToExcelAsync(bool singleSheetFormat = false, CancellationToken ct = default);

    /// <summary>
    /// تنزيل قالب Excel فارغ ومصمم بالأعمدة المطلوبة ومزود ببيانات توضيحية (بصيغة ورقة واحدة مسطحة أو 3 أوراق عمل).
    /// </summary>
    /// <param name="singleSheetFormat">تحديد شكل القالب (ورقة واحدة أو 3 أوراق)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>مصفوفة بايتات تمثل ملف قالب إكسيل</returns>
    Task<byte[]> DownloadTemplateAsync(bool singleSheetFormat = true, CancellationToken ct = default);

    /// <summary>
    /// فحص ومعاينة ملف Excel قبل الاستيراد الفعلي (Dry-Run Preview) للتأكد من سلامة البيانات وعرض التنبيهات والأخطاء.
    /// </summary>
    /// <param name="excelStream">دفق ملف إكسيل المرفوع</param>
    /// <param name="options">خيارات الاستيراد والتحقق الإضافية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة المعاينة والفحص المسبق بالأسطر الصالحة وغير الصالحة والتنبيهات</returns>
    Task<ServiceResult<ProductExcelValidationResultDto>> ValidateExcelAsync(Stream excelStream, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);

    /// <summary>
    /// استيراد الأصناف والباركودات والتصنيفات والوحدات والمخزون من ملف Excel (يتعرف تلقائياً على الملف بصيغة ورقة واحدة أو 3 أوراق).
    /// </summary>
    /// <param name="excelStream">دفق ملف إكسيل المراد استيراده</param>
    /// <param name="options">خيارات الاستيراد (استراتيجية الدمج/التحديث، الصرف التلقائي للباركود، المستودعات الافتراضية)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الاستيراد وإحصائيات السجلات المنشأة والمحدثة وقائمة الأسطر المرفوضة</returns>
    Task<ServiceResult<ProductImportResultDto>> ImportProductsFromExcelAsync(Stream excelStream, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);

    /// <summary>
    /// توليد ملف Excel يحتوي على الأسطر المرفوضة فقط مع عمود لسبب الرفض لإعادة تصحيحها ورفعها.
    /// </summary>
    /// <param name="failedRows">قائمة تفاصيل الأسطر التي فشل استيرادها وأسباب الرفض</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>مصفوفة بايتات تمثل ملف إكسيل يحتوي الأسطر المرفوضة</returns>
    Task<byte[]> GenerateFailedRowsExcelAsync(List<FailedRowDetailsDto> failedRows, CancellationToken ct = default);
}
