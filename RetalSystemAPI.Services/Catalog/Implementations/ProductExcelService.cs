using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة استيراد وتصدير الأصناف والباركودات والتصنيفات عبر ملفات Excel ذات الـ 3 أوراق عمل.
/// </summary>
public class ProductExcelService : IProductExcelService
{
    private readonly IUnitOfWork _unitOfWork;

    private const string SheetProducts = "الأصناف";
    private const string SheetBarcodes = "الباركودات";
    private const string SheetCategories = "التصنيفات";

    public ProductExcelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> ExportProductsToExcelAsync(CancellationToken ct = default)
    {
        var products = await _unitOfWork.Products.GetAllAsync(ct);
        var barcodes = await _unitOfWork.ProductBarCodes.GetAllAsync(ct);
        var categories = await _unitOfWork.Categories.GetAllAsync(ct);

        using var workbook = new XLWorkbook();
        workbook.RightToLeft = true;

        // 1. ورقة الأصناف
        var wsProducts = workbook.Worksheets.Add(SheetProducts);
        wsProducts.RightToLeft = true;
        SetupHeader(wsProducts, new[] { "معرف الصنف", "اسم الصنف", "سعر التكلفة", "سعر البيع", "معرف التصنيف" });

        int prodRow = 2;
        foreach (var p in products)
        {
            wsProducts.Cell(prodRow, 1).Value = p.Id.ToString();
            wsProducts.Cell(prodRow, 2).Value = p.Name;
            wsProducts.Cell(prodRow, 3).Value = p.CostPrice;
            wsProducts.Cell(prodRow, 4).Value = p.SalePrice;
            wsProducts.Cell(prodRow, 5).Value = p.CategoryId.ToString();
            prodRow++;
        }
        wsProducts.Columns().AdjustToContents();

        // 2. ورقة الباركودات
        var wsBarcodes = workbook.Worksheets.Add(SheetBarcodes);
        wsBarcodes.RightToLeft = true;
        SetupHeader(wsBarcodes, new[] { "معرف الباركود", "معرف الصنف", "الباركود", "عنوان الباركود" });

        int barRow = 2;
        foreach (var b in barcodes)
        {
            wsBarcodes.Cell(barRow, 1).Value = b.Id.ToString();
            wsBarcodes.Cell(barRow, 2).Value = b.ProductId.ToString();
            wsBarcodes.Cell(barRow, 3).Value = b.BarCode;
            wsBarcodes.Cell(barRow, 4).Value = b.Title;
            barRow++;
        }
        wsBarcodes.Columns().AdjustToContents();

        // 3. ورقة التصنيفات
        var wsCategories = workbook.Worksheets.Add(SheetCategories);
        wsCategories.RightToLeft = true;
        SetupHeader(wsCategories, new[] { "معرف التصنيف", "اسم التصنيف" });

        int catRow = 2;
        foreach (var c in categories)
        {
            wsCategories.Cell(catRow, 1).Value = c.Id.ToString();
            wsCategories.Cell(catRow, 2).Value = c.Name;
            catRow++;
        }
        wsCategories.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> DownloadTemplateAsync(CancellationToken ct = default)
    {
        var existingCategories = await _unitOfWork.Categories.GetAllAsync(ct);
        var sampleCategory = existingCategories.FirstOrDefault();
        Guid sampleCatId = sampleCategory?.Id ?? Guid.NewGuid();
        string sampleCatName = sampleCategory?.Name ?? "تصنيف عام";

        Guid sampleProdId = Guid.NewGuid();
        Guid sampleBarId = Guid.NewGuid();

        using var workbook = new XLWorkbook();
        workbook.RightToLeft = true;

        // 1. ورقة الأصناف
        var wsProducts = workbook.Worksheets.Add(SheetProducts);
        wsProducts.RightToLeft = true;
        SetupHeader(wsProducts, new[] { "معرف الصنف", "اسم الصنف", "سعر التكلفة", "سعر البيع", "معرف التصنيف" });
        wsProducts.Cell(2, 1).Value = sampleProdId.ToString();
        wsProducts.Cell(2, 2).Value = "صنف تجريبي مثال";
        wsProducts.Cell(2, 3).Value = 10.50m;
        wsProducts.Cell(2, 4).Value = 15.00m;
        wsProducts.Cell(2, 5).Value = sampleCatId.ToString();
        wsProducts.Columns().AdjustToContents();

        // 2. ورقة الباركودات
        var wsBarcodes = workbook.Worksheets.Add(SheetBarcodes);
        wsBarcodes.RightToLeft = true;
        SetupHeader(wsBarcodes, new[] { "معرف الباركود", "معرف الصنف", "الباركود", "عنوان الباركود" });
        wsBarcodes.Cell(2, 1).Value = sampleBarId.ToString();
        wsBarcodes.Cell(2, 2).Value = sampleProdId.ToString();
        wsBarcodes.Cell(2, 3).Value = "629100000001";
        wsBarcodes.Cell(2, 4).Value = "باركود تجريبي مثال";
        wsBarcodes.Columns().AdjustToContents();

        // 3. ورقة التصنيفات
        var wsCategories = workbook.Worksheets.Add(SheetCategories);
        wsCategories.RightToLeft = true;
        SetupHeader(wsCategories, new[] { "معرف التصنيف", "اسم التصنيف" });
        wsCategories.Cell(2, 1).Value = sampleCatId.ToString();
        wsCategories.Cell(2, 2).Value = sampleCatName;
        wsCategories.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<ServiceResult<ProductImportResultDto>> ImportProductsFromExcelAsync(Stream excelStream, CancellationToken ct = default)
    {
        var result = new ProductImportResultDto();

        if (excelStream == null || excelStream.Length == 0)
        {
            result.Errors.Add("ملف Excel غير صالح أو فارغ");
            return ServiceResult<ProductImportResultDto>.Failure("ملف Excel غير صالح أو فارغ", ErrorCodes.ValidationError);
        }

        try
        {
            // نسخ الملف إلى MemoryStream لضمان دعم السحب والبحث (Seekable Stream)
            using var ms = new MemoryStream();
            await excelStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);

            // الحصول على ورقة التصنيفات، الأصناف، والباركودات
            var wsCategories = workbook.Worksheets.FirstOrDefault(w => w.Name.Trim().Equals(SheetCategories, StringComparison.OrdinalIgnoreCase)) ?? workbook.Worksheets.ElementAtOrDefault(2);
            var wsProducts = workbook.Worksheets.FirstOrDefault(w => w.Name.Trim().Equals(SheetProducts, StringComparison.OrdinalIgnoreCase)) ?? workbook.Worksheets.FirstOrDefault();
            var wsBarcodes = workbook.Worksheets.FirstOrDefault(w => w.Name.Trim().Equals(SheetBarcodes, StringComparison.OrdinalIgnoreCase)) ?? workbook.Worksheets.ElementAtOrDefault(1);

            if (wsProducts == null)
            {
                result.Errors.Add($"لم يتم العثور على أي ورقة عمل في ملف Excel");
                return ServiceResult<ProductImportResultDto>.Failure("ورقة عمل الأصناف غير موجودة في ملف Excel", ErrorCodes.ValidationError);
            }

            // الخرائط والقواميس للربط (دعم الأكواد النصية والرقمية والـ GUID والأسماء)
            var categoryCodeMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var categoryNameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var productCodeMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var productNameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            // جلب التصنيفات الموجودة مسبقاً مع التتبع
            var existingCategories = (await _unitOfWork.Categories.GetAllTrackedAsync(ct)).ToList();
            foreach (var cat in existingCategories)
            {
                if (!string.IsNullOrWhiteSpace(cat.Name))
                {
                    categoryNameMap[cat.Name.Trim()] = cat.Id;
                }
            }

            // 1. معالجة التصنيفات أولاً (من ورقة التصنيفات إن وجدت)
            if (wsCategories != null && wsCategories != wsProducts)
            {
                int lastCatRow = wsCategories.LastRowUsed()?.RowNumber() ?? 1;
                for (int r = 2; r <= lastCatRow; r++)
                {
                    string catIdCode = wsCategories.Cell(r, 1).GetString().Trim();
                    string catName = wsCategories.Cell(r, 2).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(catName))
                        continue;

                    Category? targetCat = null;

                    if (!string.IsNullOrWhiteSpace(catIdCode) && Guid.TryParse(catIdCode, out var gCat))
                    {
                        targetCat = existingCategories.FirstOrDefault(c => c.Id == gCat);
                    }

                    if (targetCat == null && categoryNameMap.TryGetValue(catName, out var existingIdByName))
                    {
                        targetCat = existingCategories.FirstOrDefault(c => c.Id == existingIdByName);
                    }

                    if (targetCat == null)
                    {
                        targetCat = new Category
                        {
                            Id = Guid.NewGuid(),
                            Name = catName,
                            IsActive = true
                        };
                        await _unitOfWork.Categories.AddAsync(targetCat, ct);
                        existingCategories.Add(targetCat);
                        result.CategoriesImported++;
                    }

                    categoryNameMap[catName] = targetCat.Id;
                    if (!string.IsNullOrWhiteSpace(catIdCode))
                    {
                        categoryCodeMap[catIdCode] = targetCat.Id;
                    }
                }
            }

            // التأكد من وجود تصنيف عام افتراضي للأصناف التي ليس لها تصنيف
            Category? defaultCategory = existingCategories.FirstOrDefault(c => c.Name.Equals("عام", StringComparison.OrdinalIgnoreCase) || c.Name.Equals("غير محدد", StringComparison.OrdinalIgnoreCase));
            if (defaultCategory == null)
            {
                defaultCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "عام",
                    IsActive = true
                };
                await _unitOfWork.Categories.AddAsync(defaultCategory, ct);
                existingCategories.Add(defaultCategory);
                categoryNameMap[defaultCategory.Name] = defaultCategory.Id;
                result.CategoriesImported++;
            }

            await SaveTrackedChangesAsync(ct);

            // جلب المنتجات الموجودة مسبقاً مع التتبع
            var existingProducts = (await _unitOfWork.Products.GetAllTrackedAsync(ct)).ToList();
            foreach (var prod in existingProducts)
            {
                if (!string.IsNullOrWhiteSpace(prod.Name))
                {
                    productNameMap[prod.Name.Trim()] = prod.Id;
                }
            }

            // التعرّف على عناوين شيت الأصناف في السطر الأول
            string prodH1 = wsProducts.Cell(1, 1).GetString().Trim();
            string prodH2 = wsProducts.Cell(1, 2).GetString().Trim();
            bool hasProductCodeColumn = prodH1.Contains("معرف", StringComparison.OrdinalIgnoreCase) ||
                                       prodH1.Contains("كود", StringComparison.OrdinalIgnoreCase) ||
                                       prodH1.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                                       (prodH2.Contains("اسم", StringComparison.OrdinalIgnoreCase) || prodH2.Contains("صنف", StringComparison.OrdinalIgnoreCase));

            // 2. معالجة الأصناف (الورقة الرئيسية)
            int lastProdRow = wsProducts.LastRowUsed()?.RowNumber() ?? 1;
            for (int r = 2; r <= lastProdRow; r++)
            {
                string col1 = wsProducts.Cell(r, 1).GetString().Trim();
                string col2 = wsProducts.Cell(r, 2).GetString().Trim();
                string col3 = wsProducts.Cell(r, 3).GetString().Trim();
                string col4 = wsProducts.Cell(r, 4).GetString().Trim();
                string col5 = wsProducts.Cell(r, 5).GetString().Trim();

                if (string.IsNullOrWhiteSpace(col1) && string.IsNullOrWhiteSpace(col2))
                    continue;

                string? rawProdIdCode;
                string prodName;
                string costStr;
                string saleStr;
                string catInputStr;

                if (hasProductCodeColumn || (decimal.TryParse(col3, out _) && decimal.TryParse(col4, out _)))
                {
                    rawProdIdCode = col1;
                    prodName = col2;
                    costStr = col3;
                    saleStr = col4;
                    catInputStr = col5;
                }
                else if (decimal.TryParse(col2, out _) && decimal.TryParse(col3, out _))
                {
                    rawProdIdCode = null;
                    prodName = col1;
                    costStr = col2;
                    saleStr = col3;
                    catInputStr = col4;
                }
                else
                {
                    rawProdIdCode = col1;
                    prodName = string.IsNullOrWhiteSpace(col2) ? col1 : col2;
                    costStr = col3;
                    saleStr = col4;
                    catInputStr = col5;
                }

                if (string.IsNullOrWhiteSpace(prodName))
                {
                    continue;
                }

                decimal costPrice = decimal.TryParse(costStr, out var cp) ? cp : 0m;
                decimal salePrice = decimal.TryParse(saleStr, out var sp) ? sp : 0m;

                // تحديد معرف التصنيف
                Guid finalCategoryId = defaultCategory.Id;

                if (!string.IsNullOrWhiteSpace(catInputStr))
                {
                    if (categoryCodeMap.TryGetValue(catInputStr, out var catIdByCode))
                    {
                        finalCategoryId = catIdByCode;
                    }
                    else if (categoryNameMap.TryGetValue(catInputStr, out var catIdByName))
                    {
                        finalCategoryId = catIdByName;
                    }
                    else if (Guid.TryParse(catInputStr, out var rawCatGuid) && existingCategories.Any(c => c.Id == rawCatGuid))
                    {
                        finalCategoryId = rawCatGuid;
                    }
                    else
                    {
                        // إنشاء تصنيف جديد تلقائياً باسم catInputStr
                        var newCat = new Category
                        {
                            Id = Guid.NewGuid(),
                            Name = catInputStr,
                            IsActive = true
                        };
                        await _unitOfWork.Categories.AddAsync(newCat, ct);
                        existingCategories.Add(newCat);
                        categoryNameMap[catInputStr] = newCat.Id;
                        categoryCodeMap[catInputStr] = newCat.Id;
                        finalCategoryId = newCat.Id;
                        result.CategoriesImported++;
                    }
                }

                Product? targetProduct = null;

                // 1. البحث بكود/معرف الصنف الممرر من Excel (مثل "1", "101", أو GUID)
                if (!string.IsNullOrWhiteSpace(rawProdIdCode) && productCodeMap.TryGetValue(rawProdIdCode, out var prodIdByCode))
                {
                    targetProduct = existingProducts.FirstOrDefault(p => p.Id == prodIdByCode);
                }

                // 2. البحث بالـ GUID
                if (targetProduct == null && !string.IsNullOrWhiteSpace(rawProdIdCode) && Guid.TryParse(rawProdIdCode, out var parsedGuid))
                {
                    targetProduct = existingProducts.FirstOrDefault(p => p.Id == parsedGuid);
                }

                // 3. البحث باسم الصنف
                if (targetProduct == null)
                {
                    targetProduct = existingProducts.FirstOrDefault(p => p.Name.Equals(prodName, StringComparison.OrdinalIgnoreCase));
                }

                if (targetProduct != null)
                {
                    targetProduct.Name = prodName;
                    targetProduct.CostPrice = costPrice;
                    targetProduct.SalePrice = salePrice;
                    targetProduct.CategoryId = finalCategoryId;
                    result.ProductsImported++;
                }
                else
                {
                    targetProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = prodName,
                        CostPrice = costPrice,
                        SalePrice = salePrice,
                        CategoryId = finalCategoryId
                    };
                    await _unitOfWork.Products.AddAsync(targetProduct, ct);
                    existingProducts.Add(targetProduct);
                    result.ProductsImported++;
                }

                if (!string.IsNullOrWhiteSpace(rawProdIdCode))
                {
                    productCodeMap[rawProdIdCode] = targetProduct.Id;
                }
                productNameMap[prodName] = targetProduct.Id;
            }

            await SaveTrackedChangesAsync(ct);

            // 3. معالجة الباركودات (الورقة الثانية إن وجدت)
            if (wsBarcodes != null && wsBarcodes != wsProducts)
            {
                var existingBarcodes = (await _unitOfWork.ProductBarCodes.GetAllTrackedAsync(ct)).ToList();
                int lastBarRow = wsBarcodes.LastRowUsed()?.RowNumber() ?? 1;

                for (int r = 2; r <= lastBarRow; r++)
                {
                    string barIdCode = wsBarcodes.Cell(r, 1).GetString().Trim();
                    string rawProdIdStr = wsBarcodes.Cell(r, 2).GetString().Trim();
                    string barCode = wsBarcodes.Cell(r, 3).GetString().Trim();
                    string title = wsBarcodes.Cell(r, 4).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(barCode))
                    {
                        if (!string.IsNullOrWhiteSpace(rawProdIdStr) && string.IsNullOrWhiteSpace(barIdCode))
                        {
                            barCode = rawProdIdStr;
                            rawProdIdStr = barIdCode;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    Guid targetProductId = Guid.Empty;

                    // 1. البحث في كود/رقم معرف الصنف (مثل "1", "2")
                    if (!string.IsNullOrWhiteSpace(rawProdIdStr) && productCodeMap.TryGetValue(rawProdIdStr, out var prodIdByCode))
                    {
                        targetProductId = prodIdByCode;
                    }
                    // 2. البحث باسم الصنف
                    else if (!string.IsNullOrWhiteSpace(rawProdIdStr) && productNameMap.TryGetValue(rawProdIdStr, out var prodIdByName))
                    {
                        targetProductId = prodIdByName;
                    }
                    // 3. البحث بالـ GUID
                    else if (!string.IsNullOrWhiteSpace(rawProdIdStr) && Guid.TryParse(rawProdIdStr, out var gP))
                    {
                        var p = existingProducts.FirstOrDefault(x => x.Id == gP);
                        if (p != null) targetProductId = p.Id;
                    }

                    if (targetProductId == Guid.Empty)
                    {
                        result.Warnings.Add($"الصف {r} في ورقة الباركودات تم تجاهله لعدم معرفة الصنف المربوط ({rawProdIdStr})");
                        continue;
                    }

                    var parentProd = existingProducts.FirstOrDefault(p => p.Id == targetProductId);
                    if (parentProd == null)
                    {
                        result.Warnings.Add($"الصف {r} في ورقة الباركودات تم تجاهله لأن الصنف غير موجود");
                        continue;
                    }

                    ProductBarCode? targetBarCode = existingBarcodes.FirstOrDefault(b => b.BarCode.Equals(barCode, StringComparison.OrdinalIgnoreCase));

                    string finalTitle = string.IsNullOrWhiteSpace(title) ? parentProd.Name : title;

                    if (targetBarCode != null)
                    {
                        targetBarCode.BarCode = barCode;
                        targetBarCode.Title = finalTitle;
                        targetBarCode.ProductId = targetProductId;
                        result.BarcodesImported++;
                    }
                    else
                    {
                        targetBarCode = new ProductBarCode
                        {
                            Id = Guid.NewGuid(),
                            ProductId = targetProductId,
                            BarCode = barCode,
                            Title = finalTitle
                        };
                        await _unitOfWork.ProductBarCodes.AddAsync(targetBarCode, ct);
                        existingBarcodes.Add(targetBarCode);
                        result.BarcodesImported++;
                    }
                }

                await SaveTrackedChangesAsync(ct);
            }

            return ServiceResult<ProductImportResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"حدث خطأ أثناء معالجة ملف Excel: {ex.Message}");
            return ServiceResult<ProductImportResultDto>.Failure($"حدث خطأ أثناء قراءة ملف Excel: {ex.Message}", ErrorCodes.UploadFailed);
        }
    }

    private async Task SaveTrackedChangesAsync(CancellationToken ct)
    {
        // 1. فحص الكيانات المعدلة، وإذا كانت RowVersion فارغة، نقوم بجلب قيمتها الأصلية لتفادي 'WHERE RowVersion IS NULL'
        foreach (var entry in _unitOfWork.ChangeTrackerEntries())
        {
            if (entry.State == EntityState.Modified)
            {
                try
                {
                    var rowVersionProp = entry.Property("RowVersion");
                    if (rowVersionProp.OriginalValue == null)
                    {
                        rowVersionProp.IsModified = false;
                        if (rowVersionProp.CurrentValue != null)
                        {
                            rowVersionProp.OriginalValue = rowVersionProp.CurrentValue;
                        }
                    }
                }
                catch
                {
                    // تجاهل إذا كان الكيان لا يملك RowVersion
                }
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var conflictDetails = new List<string>();

            foreach (var entry in ex.Entries)
            {
                string entityName = entry.Entity.GetType().Name;
                string entityInfo = entityName;

                if (entry.Entity is Product p)
                {
                    entityInfo = $"المنتج: '{p.Name}' (ID: {p.Id})";
                }
                else if (entry.Entity is Category c)
                {
                    entityInfo = $"التصنيف: '{c.Name}' (ID: {c.Id})";
                }
                else if (entry.Entity is ProductBarCode b)
                {
                    entityInfo = $"الباركود: '{b.BarCode}' (ID: {b.Id})";
                }

                var databaseValues = await entry.GetDatabaseValuesAsync(ct);
                if (databaseValues == null)
                {
                    entry.State = EntityState.Detached;
                    conflictDetails.Add($"{entityInfo} (غير موجود في قاعدة البيانات - تم حذفه)");
                }
                else
                {
                    entry.OriginalValues.SetValues(databaseValues);
                    conflictDetails.Add($"{entityInfo} (تم تحديث نسخة البيانات RowVersion تلقائياً)");
                }
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
            catch (Exception retryEx)
            {
                string infoStr = conflictDetails.Count > 0 ? string.Join(" | ", conflictDetails) : retryEx.Message;
                throw new Exception($"فشل التحديث بسبب تعارض التزامن في قاعدة البيانات. التفاصيل: {infoStr}");
            }
        }
    }

    private static void SetupHeader(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }
}
