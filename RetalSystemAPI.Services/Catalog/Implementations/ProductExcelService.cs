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
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة استيراد وتصدير الأصناف والباركودات والتصنيفات والوحدات والمخزون عبر ملفات Excel.
/// يدعم كلاً من نظام الشيت الواحد المسطح (Single-Sheet Flat) ونظام الـ 3 أوراق عمل (Multi-Sheet).
/// </summary>
public class ProductExcelService : IProductExcelService
{
    private readonly IUnitOfWork _unitOfWork;

    private const string SheetProducts = "الأصناف";
    private const string SheetBarcodes = "الباركودات";
    private const string SheetCategories = "التصنيفات";

    /// <summary>
    /// تهيئة خدمة معالجة إكسيل مع حقن وحدة العمل.
    /// </summary>
    public ProductExcelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportProductsToExcelAsync(bool singleSheetFormat = false, CancellationToken ct = default)
    {
        // تحميل أعمدة نحيفة فقط (المفاتيح والأسماء والأسعار) بدل الجداول الكاملة في الذاكرة
        var products = await _unitOfWork.Products.SelectAsync(
            p => new { p.Id, p.Name, p.CostPrice, p.SalePrice, p.CategoryId, p.Description }, ct);
        var barcodes = await _unitOfWork.ProductBarCodes.SelectAsync(
            b => new { b.Id, b.ProductId, b.BarCode, b.Title }, ct);
        var categories = await _unitOfWork.Categories.SelectAsync(
            c => new { c.Id, c.Name }, ct);
        var units = await _unitOfWork.Units.SelectAsync(u => new { u.Id, u.Name }, ct);
        var productUnits = await _unitOfWork.ProductUnits.SelectAsync(
            pu => new { pu.ProductId, pu.UnitId, pu.ConversionFactor, pu.IsDefault }, ct);
        var showroomStocks = await _unitOfWork.ShowroomStocks.SelectAsync(
            s => new { s.ProductId, s.Quantity }, ct);
        var storageStocks = await _unitOfWork.StorgeStocks.SelectAsync(
            s => new { s.ProductBarcodeId, s.Quantity }, ct);

        using var workbook = new XLWorkbook();
        workbook.RightToLeft = true;

        if (singleSheetFormat)
        {
            // ورقة واحدة مدمجة تحوي كافة التفاصيل
            var ws = workbook.Worksheets.Add("الأصناف والمخزون");
            ws.RightToLeft = true;
            SetupHeader(ws, new[]
            {
                "كود الصنف", "اسم الصنف", "الباركود", "التصنيف", "الوحدة", "معامل التحويل",
                "سعر التكلفة", "سعر البيع", "كمية الصالة", "كمية المخزن", "حد الطلب", "الوصف"
            });

            var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);
            var barcodeLookup = barcodes.ToLookup(b => b.ProductId);
            var unitLookup = productUnits.ToLookup(u => u.ProductId);
            var unitDict = units.ToDictionary(u => u.Id, u => u.Name);
            var showStockLookup = showroomStocks.GroupBy(s => s.ProductId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));
            var storeStockLookup = storageStocks.GroupBy(s => s.ProductBarcodeId).ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            int row = 2;
            foreach (var p in products)
            {
                var prodBarcodes = barcodeLookup[p.Id].ToList();
                var primaryBarcode = prodBarcodes.FirstOrDefault();
                var defaultProdUnit = unitLookup[p.Id].FirstOrDefault(u => u.IsDefault) ?? unitLookup[p.Id].FirstOrDefault();
                string unitName = defaultProdUnit != null && unitDict.TryGetValue(defaultProdUnit.UnitId, out var uName) ? uName : "حبة";
                int conversionFactor = defaultProdUnit?.ConversionFactor ?? 1;

                string categoryName = categoryDict.TryGetValue(p.CategoryId, out var cName) ? cName : "عام";
                int showQty = showStockLookup.TryGetValue(p.Id, out int sQty) ? sQty : 0;
                int storeQty = primaryBarcode != null && storeStockLookup.TryGetValue(primaryBarcode.Id, out int stQty) ? stQty : 0;

                ws.Cell(row, 1).Value = p.Id.ToString();
                ws.Cell(row, 2).Value = p.Name;
                ws.Cell(row, 3).Value = primaryBarcode?.BarCode ?? string.Empty;
                ws.Cell(row, 4).Value = categoryName;
                ws.Cell(row, 5).Value = unitName;
                ws.Cell(row, 6).Value = conversionFactor;
                ws.Cell(row, 7).Value = p.CostPrice;
                ws.Cell(row, 8).Value = p.SalePrice;
                ws.Cell(row, 9).Value = showQty;
                ws.Cell(row, 10).Value = storeQty;
                ws.Cell(row, 11).Value = 0; // حد الطلب
                ws.Cell(row, 12).Value = p.Description ?? string.Empty;
                row++;
            }
            ws.Columns().AdjustToContents();
        }
        else
        {
            // نظام الـ 3 أوراق عمل التقليدي
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
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> DownloadTemplateAsync(bool singleSheetFormat = true, CancellationToken ct = default)
    {
        // صف واحد فقط بدل تحميل جدول التصنيفات كاملاً
        var sampleCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(c => true, ct);
        string sampleCatName = sampleCategory?.Name ?? "مواد غذائية";
        Guid sampleCatId = sampleCategory?.Id ?? Guid.NewGuid();

        using var workbook = new XLWorkbook();
        workbook.RightToLeft = true;

        if (singleSheetFormat)
        {
            var ws = workbook.Worksheets.Add("قالب استيراد الأصناف");
            ws.RightToLeft = true;
            SetupHeader(ws, new[]
            {
                "كود الصنف", "اسم الصنف *", "الباركود *", "التصنيف", "الوحدة", "معامل التحويل",
                "سعر التكلفة *", "سعر البيع *", "كمية الصالة", "كمية المخزن", "حد الطلب", "الوصف"
            });

            // سطر تجريبي 1
            ws.Cell(2, 1).Value = "101";
            ws.Cell(2, 2).Value = "عصير تفاح طبيعي 250 مل";
            ws.Cell(2, 3).Value = "629100000001";
            ws.Cell(2, 4).Value = sampleCatName;
            ws.Cell(2, 5).Value = "حبة";
            ws.Cell(2, 6).Value = 1;
            ws.Cell(2, 7).Value = 2.50m;
            ws.Cell(2, 8).Value = 3.50m;
            ws.Cell(2, 9).Value = 24;
            ws.Cell(2, 10).Value = 120;
            ws.Cell(2, 11).Value = 10;
            ws.Cell(2, 12).Value = "عصير تفاح بدون سكر مضاف";

            // سطر تجريبي 2
            ws.Cell(3, 1).Value = "102";
            ws.Cell(3, 2).Value = "حليب كامل الدسم 1 لتر";
            ws.Cell(3, 3).Value = "629100000002";
            ws.Cell(3, 4).Value = "ألبان وأجبان";
            ws.Cell(3, 5).Value = "كرتونة";
            ws.Cell(3, 6).Value = 12;
            ws.Cell(3, 7).Value = 45.00m;
            ws.Cell(3, 8).Value = 55.00m;
            ws.Cell(3, 9).Value = 10;
            ws.Cell(3, 10).Value = 50;
            ws.Cell(3, 11).Value = 5;
            ws.Cell(3, 12).Value = "كرتونة تحتوي 12 عبوة";

            ws.Columns().AdjustToContents();
        }
        else
        {
            Guid sampleProdId = Guid.NewGuid();
            Guid sampleBarId = Guid.NewGuid();

            var wsProducts = workbook.Worksheets.Add(SheetProducts);
            wsProducts.RightToLeft = true;
            SetupHeader(wsProducts, new[] { "معرف الصنف", "اسم الصنف", "سعر التكلفة", "سعر البيع", "معرف التصنيف" });
            wsProducts.Cell(2, 1).Value = sampleProdId.ToString();
            wsProducts.Cell(2, 2).Value = "صنف تجريبي مثال";
            wsProducts.Cell(2, 3).Value = 10.50m;
            wsProducts.Cell(2, 4).Value = 15.00m;
            wsProducts.Cell(2, 5).Value = sampleCatId.ToString();
            wsProducts.Columns().AdjustToContents();

            var wsBarcodes = workbook.Worksheets.Add(SheetBarcodes);
            wsBarcodes.RightToLeft = true;
            SetupHeader(wsBarcodes, new[] { "معرف الباركود", "معرف الصنف", "الباركود", "عنوان الباركود" });
            wsBarcodes.Cell(2, 1).Value = sampleBarId.ToString();
            wsBarcodes.Cell(2, 2).Value = sampleProdId.ToString();
            wsBarcodes.Cell(2, 3).Value = "629100000001";
            wsBarcodes.Cell(2, 4).Value = "باركود تجريبي مثال";
            wsBarcodes.Columns().AdjustToContents();

            var wsCategories = workbook.Worksheets.Add(SheetCategories);
            wsCategories.RightToLeft = true;
            SetupHeader(wsCategories, new[] { "معرف التصنيف", "اسم التصنيف" });
            wsCategories.Cell(2, 1).Value = sampleCatId.ToString();
            wsCategories.Cell(2, 2).Value = sampleCatName;
            wsCategories.Columns().AdjustToContents();
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<ServiceResult<ProductExcelValidationResultDto>> ValidateExcelAsync(
        Stream excelStream,
        ProductExcelImportOptionsDto? options = null,
        CancellationToken ct = default)
    {
        options ??= new ProductExcelImportOptionsDto();
        var result = new ProductExcelValidationResultDto();

        if (excelStream == null || excelStream.Length == 0)
        {
            result.Errors.Add("ملف Excel فارغ أو غير صالح");
            return ServiceResult<ProductExcelValidationResultDto>.Failure("ملف Excel فارغ أو غير صالح", ErrorCodes.ValidationError);
        }

        try
        {
            using var ms = new MemoryStream();
            await excelStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);
            bool isSingleSheet = DetectIfSingleSheet(workbook);
            result.IsSingleSheetFormat = isSingleSheet;

            // جلب البيانات الحالية من قاعدة البيانات للمقارنة — أعمدة نحيفة فقط بدل الجداول الكاملة
            var existingProducts = await _unitOfWork.Products.SelectAsync(p => new { p.Id, p.Name }, ct);
            var existingBarcodes = await _unitOfWork.ProductBarCodes.SelectAsync(b => new { b.BarCode, b.ProductId }, ct);
            var existingCategories = await _unitOfWork.Categories.SelectAsync(c => new { c.Name }, ct);

            var existingProductIds = new HashSet<Guid>(existingProducts.Select(p => p.Id));
            var barcodeMap = existingBarcodes
                .GroupBy(b => b.BarCode.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().ProductId, StringComparer.OrdinalIgnoreCase);
            var productsByBarcodeProductId = existingBarcodes
                .GroupBy(b => b.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(b => b.BarCode.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase));
            var productNameMap = existingProducts.ToDictionary(p => p.Name.Trim(), p => p.Id, StringComparer.OrdinalIgnoreCase);
            var categoryNameSet = new HashSet<string>(existingCategories.Select(c => c.Name.Trim()), StringComparer.OrdinalIgnoreCase);

            var fileBarcodesSeen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var newCategoriesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newBarcodesInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (isSingleSheet)
            {
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {
                    result.Errors.Add("لا توجد أي ورقة عمل في ملف Excel");
                    return ServiceResult<ProductExcelValidationResultDto>.Failure("لا توجد أوراق عمل في الملف", ErrorCodes.ValidationError);
                }

                var colMap = DetectSingleSheetColumns(ws);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                for (int r = 2; r <= lastRow; r++)
                {
                    string name = GetCellValue(ws, r, colMap.NameCol);
                    string code = GetCellValue(ws, r, colMap.ItemCodeCol);
                    string barcode = GetCellValue(ws, r, colMap.BarcodeCol);
                    string cat = GetCellValue(ws, r, colMap.CategoryCol);
                    string unit = GetCellValue(ws, r, colMap.UnitCol);
                    string costStr = GetCellValue(ws, r, colMap.CostPriceCol);
                    string saleStr = GetCellValue(ws, r, colMap.SalePriceCol);
                    string showQtyStr = GetCellValue(ws, r, colMap.ShowroomQtyCol);
                    string storeQtyStr = GetCellValue(ws, r, colMap.StorageQtyCol);

                    if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(barcode))
                        continue;

                    result.TotalRows++;

                    string displayBarcode = barcode;
                    string additionalMsg = string.Empty;
                    bool isAuto = false;

                    if (string.IsNullOrWhiteSpace(barcode))
                    {
                        result.MissingBarcodesCount++;
                        if (options.AutoGenerateMissingBarcodes)
                        {
                            if (!string.IsNullOrWhiteSpace(code) && !Guid.TryParse(code, out _))
                            {
                                displayBarcode = code;
                                additionalMsg = "سيتم استخدام كود الصنف كباركود";
                                newBarcodesInFile.Add(code);
                            }
                            else
                            {
                                displayBarcode = "(صرف كود تلقائي)";
                                additionalMsg = "سيتم صرف كود جديد تلقائياً";
                                result.AutoGeneratedBarcodesCount++;
                                newBarcodesInFile.Add($"auto_single_{r}");
                                isAuto = true;
                            }
                        }
                        else
                        {
                            displayBarcode = string.Empty;
                            additionalMsg = "الصنف بدون كود (يتطلب إدخال كود يدوي)";
                        }
                    }

                    var rowVal = new ProductExcelRowValidationDto
                    {
                        RowNumber = r,
                        ItemCode = code,
                        Name = name,
                        BarCode = displayBarcode,
                        OriginalBarCode = string.IsNullOrWhiteSpace(barcode) ? null : barcode,
                        IsAutoBarcode = isAuto,
                        IsCustomBarcode = false,
                        CategoryName = string.IsNullOrWhiteSpace(cat) ? "عام" : cat,
                        UnitName = string.IsNullOrWhiteSpace(unit) ? "حبة" : unit,
                    };

                    decimal.TryParse(costStr, out var costPrice);
                    decimal.TryParse(saleStr, out var salePrice);
                    int.TryParse(showQtyStr, out var showQty);
                    int.TryParse(storeQtyStr, out var storeQty);

                    rowVal.CostPrice = costPrice;
                    rowVal.SalePrice = salePrice;
                    rowVal.ShowroomQuantity = showQty;
                    rowVal.StorageQuantity = storeQty;

                    // التحقق من الحقول الإلزامية
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        rowVal.Status = RowValidationStatus.Error;
                        rowVal.Message = "اسم الصنف إلزامي";
                        result.InvalidRows++;
                        result.Rows.Add(rowVal);
                        continue;
                    }

                    if (costPrice < 0 || salePrice < 0)
                    {
                        rowVal.Status = RowValidationStatus.Error;
                        rowVal.Message = "الأسعار لا يمكن أن تكون سالبة";
                        result.InvalidRows++;
                        result.Rows.Add(rowVal);
                        continue;
                    }

                    // التحقق من تكرار الباركود داخل الملف نفسه
                    if (!string.IsNullOrWhiteSpace(barcode))
                    {
                        if (fileBarcodesSeen.TryGetValue(barcode, out int previousRow))
                        {
                            rowVal.Status = RowValidationStatus.Warning;
                            rowVal.Message = $"الباركود مكرر في الملف مع السطر رقم {previousRow}";
                            result.Warnings.Add($"السطر {r}: الباركود '{barcode}' مكرر مع السطر {previousRow}");
                        }
                        else
                        {
                            fileBarcodesSeen[barcode] = r;
                        }
                    }

                    // التحقق من وجود الصنف في النظام
                    bool productExists = false;
                    if (!string.IsNullOrWhiteSpace(barcode) && barcodeMap.ContainsKey(barcode))
                    {
                        productExists = true;
                    }
                    else if (productNameMap.ContainsKey(name))
                    {
                        productExists = true;
                    }
                    else if (!string.IsNullOrWhiteSpace(code) && Guid.TryParse(code, out var gCode) && existingProductIds.Contains(gCode))
                    {
                        productExists = true;
                    }

                    // التحقق من التصنيف
                    if (!string.IsNullOrWhiteSpace(cat) && !categoryNameSet.Contains(cat))
                    {
                        newCategoriesInFile.Add(cat);
                    }

                    if (!string.IsNullOrWhiteSpace(barcode) && !barcodeMap.ContainsKey(barcode))
                    {
                        newBarcodesInFile.Add(barcode);
                    }

                    // تحديد الإجراء حسب وضع الاستيراد
                    if (productExists)
                    {
                        if (options.ImportMode == ProductImportMode.InsertOnly)
                        {
                            rowVal.Action = RowActionType.Skip;
                            rowVal.Message = "الصنف موجود مسبقاً (سيتم تجاهله في وضع إضافة الجديد فقط)";
                        }
                        else
                        {
                            rowVal.Action = RowActionType.Update;
                            rowVal.Message = string.IsNullOrWhiteSpace(additionalMsg)
                                ? "الصنف موجود مسبقاً (سيتم تحديث بياناته وأسعاره)"
                                : $"تحديث الصنف ({additionalMsg})";
                            result.UpdatedProductsCount++;
                        }
                    }
                    else
                    {
                        if (options.ImportMode == ProductImportMode.UpdateExistingOnly)
                        {
                            rowVal.Action = RowActionType.Skip;
                            rowVal.Message = "الصنف جديد (سيتم تجاهله في وضع تحديث المسجل فقط)";
                        }
                        else
                        {
                            rowVal.Action = RowActionType.Create;
                            rowVal.Message = string.IsNullOrWhiteSpace(additionalMsg)
                                ? "صنف جديد (سيتم إضافته للنظام)"
                                : $"صنف جديد ({additionalMsg})";
                            result.NewProductsCount++;
                        }
                    }

                    if (!options.AutoGenerateMissingBarcodes && string.IsNullOrWhiteSpace(barcode))
                    {
                        rowVal.Status = RowValidationStatus.Warning;
                    }

                    if (rowVal.Status != RowValidationStatus.Error)
                    {
                        result.ValidRows++;
                    }

                    result.Rows.Add(rowVal);
                }
            }
            else
            {
                // معالجة فحص ملف الـ 3 شيتات (Multi-Sheet)
                var wsProducts = workbook.Worksheets.FirstOrDefault(w => IsProductsSheetName(w.Name)) ?? workbook.Worksheets.FirstOrDefault();
                var wsBarcodes = workbook.Worksheets.FirstOrDefault(w => IsBarcodesSheetName(w.Name)) ?? workbook.Worksheets.ElementAtOrDefault(1);

                if (wsProducts != null)
                {
                    var productsWithBarcodesInSheet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (wsBarcodes != null && wsBarcodes != wsProducts)
                    {
                        int lastBarRow = wsBarcodes.LastRowUsed()?.RowNumber() ?? 1;
                        for (int r = 2; r <= lastBarRow; r++)
                        {
                            string barIdCode = wsBarcodes.Cell(r, 1).GetString().Trim();
                            string rawProdIdStr = wsBarcodes.Cell(r, 2).GetString().Trim();
                            string bCode = wsBarcodes.Cell(r, 3).GetString().Trim();

                            if (string.IsNullOrWhiteSpace(bCode) && !string.IsNullOrWhiteSpace(rawProdIdStr) && string.IsNullOrWhiteSpace(barIdCode))
                            {
                                bCode = rawProdIdStr;
                                rawProdIdStr = barIdCode;
                            }

                            if (!string.IsNullOrWhiteSpace(bCode))
                            {
                                newBarcodesInFile.Add(bCode);
                            }
                            if (!string.IsNullOrWhiteSpace(rawProdIdStr))
                            {
                                productsWithBarcodesInSheet.Add(rawProdIdStr);
                            }
                        }
                    }

                    int lastRow = wsProducts.LastRowUsed()?.RowNumber() ?? 1;
                    for (int r = 2; r <= lastRow; r++)
                    {
                        string c1 = wsProducts.Cell(r, 1).GetString().Trim();
                        string c2 = wsProducts.Cell(r, 2).GetString().Trim();
                        string c3 = wsProducts.Cell(r, 3).GetString().Trim();
                        string c4 = wsProducts.Cell(r, 4).GetString().Trim();

                        if (string.IsNullOrWhiteSpace(c1) && string.IsNullOrWhiteSpace(c2)) continue;

                        result.TotalRows++;
                        string name = string.IsNullOrWhiteSpace(c2) ? c1 : c2;
                        string prodCode = string.IsNullOrWhiteSpace(c2) ? string.Empty : c1;
                        decimal.TryParse(c3, out var cost);
                        decimal.TryParse(c4, out var sale);

                        bool hasBarcodeInSheet = (!string.IsNullOrWhiteSpace(prodCode) && productsWithBarcodesInSheet.Contains(prodCode)) ||
                                                 productsWithBarcodesInSheet.Contains(name);

                        bool hasBarcodeInDb = productNameMap.TryGetValue(name, out var pId) && productsByBarcodeProductId.ContainsKey(pId);

                        string displayBarcode;
                        string extraMsg = string.Empty;
                        bool isMultiAuto = false;

                        if (hasBarcodeInSheet)
                        {
                            displayBarcode = "محدد في ورقة الأكواد";
                        }
                        else if (hasBarcodeInDb)
                        {
                            displayBarcode = "مسجل مسبقاً في النظام";
                        }
                        else
                        {
                            result.MissingBarcodesCount++;
                            if (options.AutoGenerateMissingBarcodes)
                            {
                                displayBarcode = "(صرف كود تلقائي)";
                                extraMsg = "ليس له كود في صفحة الأكواد - سيتم صرف كود جديد تلقائياً";
                                result.AutoGeneratedBarcodesCount++;
                                newBarcodesInFile.Add($"auto_multi_{r}");
                                isMultiAuto = true;
                            }
                            else
                            {
                                displayBarcode = string.Empty;
                                extraMsg = "ليس له كود في صفحة الأكواد (يتطلب إدخال كود يدوي)";
                            }
                        }

                        var rowVal = new ProductExcelRowValidationDto
                        {
                            RowNumber = r,
                            ItemCode = prodCode,
                            Name = name,
                            BarCode = displayBarcode,
                            OriginalBarCode = null,
                            IsAutoBarcode = isMultiAuto,
                            IsCustomBarcode = false,
                            CostPrice = cost,
                            SalePrice = sale,
                            CategoryName = "عام",
                            UnitName = "حبة",
                            Message = extraMsg
                        };

                        if (productNameMap.ContainsKey(name))
                        {
                            rowVal.Action = options.ImportMode == ProductImportMode.InsertOnly ? RowActionType.Skip : RowActionType.Update;
                            if (string.IsNullOrWhiteSpace(rowVal.Message)) rowVal.Message = "صنف موجود مسبقاً";
                            result.UpdatedProductsCount++;
                        }
                        else
                        {
                            rowVal.Action = options.ImportMode == ProductImportMode.UpdateExistingOnly ? RowActionType.Skip : RowActionType.Create;
                            if (string.IsNullOrWhiteSpace(rowVal.Message)) rowVal.Message = "صنف جديد";
                            result.NewProductsCount++;
                        }

                        result.ValidRows++;
                        result.Rows.Add(rowVal);
                    }
                }
            }

            result.NewCategoriesCount = newCategoriesInFile.Count;
            result.NewBarcodesCount = newBarcodesInFile.Count;

            return ServiceResult<ProductExcelValidationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"خطأ أثناء فحص ملف Excel: {ex.Message}");
            return ServiceResult<ProductExcelValidationResultDto>.Failure($"حدث خطأ أثناء فحص ملف Excel: {ex.Message}", ErrorCodes.ValidationError);
        }
    }

    public async Task<ServiceResult<ProductImportResultDto>> ImportProductsFromExcelAsync(
        Stream excelStream,
        ProductExcelImportOptionsDto? options = null,
        CancellationToken ct = default)
    {
        options ??= new ProductExcelImportOptionsDto();
        var result = new ProductImportResultDto();

        if (excelStream == null || excelStream.Length == 0)
        {
            result.Errors.Add("ملف Excel غير صالح أو فارغ");
            return ServiceResult<ProductImportResultDto>.Failure("ملف Excel غير صالح أو فارغ", ErrorCodes.ValidationError);
        }

        try
        {
            using var ms = new MemoryStream();
            await excelStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            using var workbook = new XLWorkbook(ms);
            bool isSingleSheet = DetectIfSingleSheet(workbook);

            // جلب المخازن والصالات لتهيئة المخزون
            var showrooms = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Show, ct);
            if (showrooms.Count == 0)
            {
                showrooms = await _unitOfWork.Warehouses.GetAllAsync(ct);
            }
            var targetShowroom = options.DefaultShowroomWarehouseId.HasValue
                ? showrooms.FirstOrDefault(s => s.Id == options.DefaultShowroomWarehouseId.Value) ?? showrooms.FirstOrDefault()
                : showrooms.FirstOrDefault();

            var storageWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
            if (storageWarehouses.Count == 0)
            {
                storageWarehouses = await _unitOfWork.Warehouses.GetAllAsync(ct);
            }
            var targetStorageWh = options.DefaultStorageWarehouseId.HasValue
                ? storageWarehouses.FirstOrDefault(s => s.Id == options.DefaultStorageWarehouseId.Value) ?? storageWarehouses.FirstOrDefault()
                : storageWarehouses.FirstOrDefault();

            // بدء معاملة مالية متكاملة لضمان سلامة العمليات
            await _unitOfWork.BeginTransactionAsync(ct);

            if (isSingleSheet)
            {
                await ProcessSingleSheetImportAsync(workbook, options, targetShowroom, targetStorageWh, result, ct);
            }
            else
            {
                await ProcessMultiSheetImportAsync(workbook, options, targetShowroom, targetStorageWh, result, ct);
            }

            // تثبيت المعاملة عند النجاح
            await _unitOfWork.CommitTransactionAsync(ct);

            return ServiceResult<ProductImportResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            result.Errors.Add($"حدث خطأ أثناء معالجة ملف Excel وتم التراجع عن كافة التغييرات: {ex.Message}");
            return ServiceResult<ProductImportResultDto>.Failure($"فشلت عملية الاستيراد: {ex.Message}", ErrorCodes.UploadFailed);
        }
    }

    public async Task<byte[]> GenerateFailedRowsExcelAsync(List<FailedRowDetailsDto> failedRows, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        workbook.RightToLeft = true;

        var ws = workbook.Worksheets.Add("الأسطر المرفوضة");
        ws.RightToLeft = true;

        SetupHeader(ws, new[]
        {
            "رقم السطر في الملف", "كود الصنف", "اسم الصنف", "الباركود", "التصنيف", "الوحدة",
            "سعر التكلفة", "سعر البيع", "كمية الصالة", "كمية المخزن", "سبب الرفض والخطأ *"
        });

        int row = 2;
        foreach (var f in failedRows)
        {
            ws.Cell(row, 1).Value = f.RowNumber;
            ws.Cell(row, 2).Value = f.ItemCode ?? string.Empty;
            ws.Cell(row, 3).Value = f.Name ?? string.Empty;
            ws.Cell(row, 4).Value = f.BarCode ?? string.Empty;
            ws.Cell(row, 5).Value = f.CategoryName ?? string.Empty;
            ws.Cell(row, 6).Value = f.UnitName ?? string.Empty;
            ws.Cell(row, 7).Value = f.CostPrice;
            ws.Cell(row, 8).Value = f.SalePrice;
            ws.Cell(row, 9).Value = f.ShowroomQuantity;
            ws.Cell(row, 10).Value = f.StorageQuantity;

            var reasonCell = ws.Cell(row, 11);
            reasonCell.Value = f.ErrorReason;
            reasonCell.Style.Font.FontColor = XLColor.Red;
            reasonCell.Style.Font.Bold = true;

            row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    #region Private Import Processors

    private async Task ProcessSingleSheetImportAsync(
        XLWorkbook workbook,
        ProductExcelImportOptionsDto options,
        Warehouse? targetShowroom,
        Warehouse? targetStorageWh,
        ProductImportResultDto result,
        CancellationToken ct)
    {
        var ws = workbook.Worksheets.FirstOrDefault();
        if (ws == null) return;

        var colMap = DetectSingleSheetColumns(ws);
        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        // قواميس التحميل المسبق لتسريع العمليات
        var existingCategories = (await _unitOfWork.Categories.GetAllTrackedAsync(ct)).ToList();
        var categoryNameMap = existingCategories.ToDictionary(c => c.Name.Trim(), c => c, StringComparer.OrdinalIgnoreCase);

        var existingUnits = (await _unitOfWork.Units.GetAllTrackedAsync(ct)).ToList();
        var unitNameMap = existingUnits.ToDictionary(u => u.Name.Trim(), u => u, StringComparer.OrdinalIgnoreCase);

        var existingProducts = (await _unitOfWork.Products.GetAllTrackedAsync(ct)).ToList();
        var productNameMap = existingProducts.ToDictionary(p => p.Name.Trim(), p => p, StringComparer.OrdinalIgnoreCase);
        var productIdMap = existingProducts.ToDictionary(p => p.Id, p => p);

        var existingBarcodes = (await _unitOfWork.ProductBarCodes.GetAllTrackedAsync(ct)).ToList();
        var barcodeMap = existingBarcodes.ToDictionary(b => b.BarCode.Trim(), b => b, StringComparer.OrdinalIgnoreCase);
        var usedBarcodesSet = new HashSet<string>(existingBarcodes.Select(b => b.BarCode.Trim()), StringComparer.OrdinalIgnoreCase);
        long barcodeSequence = InitializeBarcodeSequence(usedBarcodesSet);

        var existingShowroomStocks = (await _unitOfWork.ShowroomStocks.GetAllTrackedAsync(ct)).ToList();
        var existingStorageStocks = (await _unitOfWork.StorgeStocks.GetAllTrackedAsync(ct)).ToList();

        // فهرس وحدات الأصناف — استعلام واحد قبل الحلقة بدل استعلام جدول كامل لكل صف
        var existingProdUnits = (await _unitOfWork.ProductUnits.GetAllTrackedAsync(ct)).ToList();
        var existingProdUnitsByProduct = existingProdUnits.ToDictionary(u => (u.ProductId, u.UnitId), u => u);

        // فهارس قاموسية O(1) للأرصدة بدل مسح خطي لكل صف
        var showroomStocksByProductAndWarehouse = existingShowroomStocks
            .ToDictionary(s => (s.WarehouseId, s.ProductId), s => s);
        var storageStocksByBarcodeAndWarehouse = existingStorageStocks
            .ToDictionary(s => (s.WarehouseId, s.ProductBarcodeId), s => s);

        // ضمان وجود تصنيف عام
        Category? defaultCat = existingCategories.FirstOrDefault(c => c.Name.Equals("عام", StringComparison.OrdinalIgnoreCase));
        if (defaultCat == null)
        {
            defaultCat = new Category { Id = Guid.NewGuid(), Name = "عام", IsActive = true };
            await _unitOfWork.Categories.AddAsync(defaultCat, ct);
            existingCategories.Add(defaultCat);
            categoryNameMap[defaultCat.Name] = defaultCat;
            result.CategoriesImported++;
        }

        // ضمان وجود وحدة افتراضية "حبة"
        Unit? defaultUnit = existingUnits.FirstOrDefault(u => u.Name.Equals("حبة", StringComparison.OrdinalIgnoreCase) || u.Name.Equals("قطعة", StringComparison.OrdinalIgnoreCase));
        if (defaultUnit == null)
        {
            defaultUnit = new Unit { Id = Guid.NewGuid(), Name = "حبة", UnitPackage = 1 };
            await _unitOfWork.Units.AddAsync(defaultUnit, ct);
            existingUnits.Add(defaultUnit);
            unitNameMap[defaultUnit.Name] = defaultUnit;
            result.UnitsImported++;
        }

        for (int r = 2; r <= lastRow; r++)
        {
            string name = GetCellValue(ws, r, colMap.NameCol);
            string code = GetCellValue(ws, r, colMap.ItemCodeCol);
            string barcode = GetCellValue(ws, r, colMap.BarcodeCol);
            string catName = GetCellValue(ws, r, colMap.CategoryCol);
            string unitName = GetCellValue(ws, r, colMap.UnitCol);
            string convStr = GetCellValue(ws, r, colMap.ConversionFactorCol);
            string costStr = GetCellValue(ws, r, colMap.CostPriceCol);
            string saleStr = GetCellValue(ws, r, colMap.SalePriceCol);
            string showQtyStr = GetCellValue(ws, r, colMap.ShowroomQtyCol);
            string storeQtyStr = GetCellValue(ws, r, colMap.StorageQtyCol);
            string minStockStr = GetCellValue(ws, r, colMap.MinStockCol);
            string desc = GetCellValue(ws, r, colMap.DescriptionCol);

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(barcode))
                continue;

            decimal.TryParse(costStr, out var costPrice);
            decimal.TryParse(saleStr, out var salePrice);
            int.TryParse(convStr, out var convFactor);
            if (convFactor <= 0) convFactor = 1;
            int.TryParse(showQtyStr, out var showQty);
            int.TryParse(storeQtyStr, out var storeQty);
            int.TryParse(minStockStr, out var minStock);

            if (string.IsNullOrWhiteSpace(name))
            {
                result.FailedRows.Add(new FailedRowDetailsDto
                {
                    RowNumber = r,
                    ItemCode = code,
                    Name = name,
                    BarCode = barcode,
                    ErrorReason = "اسم الصنف فارغ وإلزامي"
                });
                continue;
            }

            if (costPrice < 0 || salePrice < 0)
            {
                result.FailedRows.Add(new FailedRowDetailsDto
                {
                    RowNumber = r,
                    ItemCode = code,
                    Name = name,
                    BarCode = barcode,
                    CostPrice = costPrice,
                    SalePrice = salePrice,
                    ErrorReason = "أسعار الصنف لا يمكن أن تكون سالبة"
                });
                continue;
            }

            // 1. تحديد أو إنشاء التصنيف
            Category finalCategory = defaultCat;
            if (!string.IsNullOrWhiteSpace(catName))
            {
                if (categoryNameMap.TryGetValue(catName, out var foundCat))
                {
                    finalCategory = foundCat;
                }
                else if (options.AutoCreateCategories)
                {
                    finalCategory = new Category { Id = Guid.NewGuid(), Name = catName, IsActive = true };
                    await _unitOfWork.Categories.AddAsync(finalCategory, ct);
                    existingCategories.Add(finalCategory);
                    categoryNameMap[catName] = finalCategory;
                    result.CategoriesImported++;
                }
            }

            // 2. تحديد أو إنشاء الوحدة
            Unit finalUnit = defaultUnit;
            if (!string.IsNullOrWhiteSpace(unitName))
            {
                if (unitNameMap.TryGetValue(unitName, out var foundUnit))
                {
                    finalUnit = foundUnit;
                }
                else if (options.AutoCreateUnits)
                {
                    finalUnit = new Unit { Id = Guid.NewGuid(), Name = unitName, UnitPackage = convFactor };
                    await _unitOfWork.Units.AddAsync(finalUnit, ct);
                    existingUnits.Add(finalUnit);
                    unitNameMap[unitName] = finalUnit;
                    result.UnitsImported++;
                }
            }

            // 3. البحث عن الصنف
            Product? targetProduct = null;
            if (!string.IsNullOrWhiteSpace(barcode) && barcodeMap.TryGetValue(barcode, out var existingBc))
            {
                productIdMap.TryGetValue(existingBc.ProductId, out targetProduct);
            }
            if (targetProduct == null && !string.IsNullOrWhiteSpace(code) && Guid.TryParse(code, out var parsedGuid))
            {
                productIdMap.TryGetValue(parsedGuid, out targetProduct);
            }
            if (targetProduct == null && productNameMap.TryGetValue(name, out var prodByName))
            {
                targetProduct = prodByName;
            }

            // معالجة استراتيجيات الاستيراد
            if (targetProduct != null)
            {
                if (options.ImportMode == ProductImportMode.InsertOnly)
                {
                    continue; // تخطي الصنف الموجود
                }

                // تحديث بيانات الصنف
                targetProduct.Name = name;
                targetProduct.CostPrice = costPrice;
                targetProduct.SalePrice = salePrice;
                targetProduct.CategoryId = finalCategory.Id;
                if (!string.IsNullOrWhiteSpace(desc)) targetProduct.Description = desc;
                result.ProductsImported++;
            }
            else
            {
                if (options.ImportMode == ProductImportMode.UpdateExistingOnly)
                {
                    continue; // تخطي إضافة الصنف الجديد
                }

                targetProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    CostPrice = costPrice,
                    SalePrice = salePrice,
                    CategoryId = finalCategory.Id,
                    Description = desc
                };
                await _unitOfWork.Products.AddAsync(targetProduct, ct);
                existingProducts.Add(targetProduct);
                productNameMap[name] = targetProduct;
                productIdMap[targetProduct.Id] = targetProduct;
                result.ProductsImported++;
            }

            // 4. ربط وحدة الصنف ProductUnit — فهرس قاموسي مبني مسبقاً بدل استعلام جدول كامل لكل صف
            var prodUnit = existingProdUnitsByProduct.TryGetValue((targetProduct.Id, finalUnit.Id), out var existingUnit)
                ? existingUnit
                : null;
            if (prodUnit == null)
            {
                prodUnit = new ProductUnit
                {
                    Id = Guid.NewGuid(),
                    ProductId = targetProduct.Id,
                    UnitId = finalUnit.Id,
                    ConversionFactor = convFactor,
                    IsDefault = true
                };
                await _unitOfWork.ProductUnits.AddAsync(prodUnit, ct);
                existingProdUnits.Add(prodUnit);
                existingProdUnitsByProduct[(targetProduct.Id, finalUnit.Id)] = prodUnit;
            }
            else
            {
                prodUnit.ConversionFactor = convFactor;
            }

            // 5. إنشاء أو تحديث الباركود (مع ضمان صرف كود فريد في حال عدم توفره)
            ProductBarCode? primaryBarcodeEntity = null;
            string rawBarcode = barcode?.Trim() ?? string.Empty;
            string rawCode = code?.Trim() ?? string.Empty;

            string effectiveBarcode = rawBarcode;
            bool isAutoGenerated = false;

            if (string.IsNullOrWhiteSpace(effectiveBarcode))
            {
                if (options.AutoGenerateMissingBarcodes)
                {
                    // إذا لم يوجد باركود، نتحقق من كود الصنف إن لم يكن GUID ومتاحاً، أو نولد كوداً تسلسلياً جديداً
                    string? preferredCandidate = (!string.IsNullOrWhiteSpace(rawCode) && !Guid.TryParse(rawCode, out _)) ? rawCode : null;
                    effectiveBarcode = AllocateOrGenerateBarcode(ref barcodeSequence, usedBarcodesSet, preferredCandidate);
                    isAutoGenerated = true;
                }
            }
            else
            {
                usedBarcodesSet.Add(effectiveBarcode);
            }

            if (!string.IsNullOrWhiteSpace(effectiveBarcode))
            {
                if (barcodeMap.TryGetValue(effectiveBarcode, out var foundBc))
                {
                    foundBc.ProductId = targetProduct.Id;
                    foundBc.Title = name;
                    primaryBarcodeEntity = foundBc;
                    result.BarcodesImported++;
                }
                else
                {
                    primaryBarcodeEntity = new ProductBarCode
                    {
                        Id = Guid.NewGuid(),
                        ProductId = targetProduct.Id,
                        BarCode = effectiveBarcode,
                        Title = name
                    };
                    await _unitOfWork.ProductBarCodes.AddAsync(primaryBarcodeEntity, ct);
                    existingBarcodes.Add(primaryBarcodeEntity);
                    barcodeMap[effectiveBarcode] = primaryBarcodeEntity;
                    result.BarcodesImported++;
                    if (isAutoGenerated)
                    {
                        result.AutoGeneratedBarcodesCount++;
                    }
                }
            }

            // 6. تهيئة رصيد الصالة ShowroomStock — بحث O(1) بالفهرس المسبق
            if (targetShowroom != null)
            {
                var sStock = showroomStocksByProductAndWarehouse.TryGetValue((targetShowroom.Id, targetProduct.Id), out var foundStock)
                    ? foundStock
                    : null;
                if (sStock != null)
                {
                    if (showQty > 0) sStock.Quantity = showQty;
                    if (minStock > 0) sStock.MinStockLevel = minStock;
                }
                else
                {
                    sStock = new ShowroomStock
                    {
                        Id = Guid.NewGuid(),
                        WarehouseId = targetShowroom.Id,
                        ProductId = targetProduct.Id,
                        Quantity = showQty,
                        MinStockLevel = minStock
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(sStock, ct);
                    existingShowroomStocks.Add(sStock);
                    showroomStocksByProductAndWarehouse[(targetShowroom.Id, targetProduct.Id)] = sStock;
                    result.StocksInitialized++;
                }
            }

            // 7. تهيئة رصيد المخزن StorgeStock — بحث O(1) بالفهرس المسبق
            if (targetStorageWh != null && primaryBarcodeEntity != null)
            {
                var stStock = storageStocksByBarcodeAndWarehouse.TryGetValue((targetStorageWh.Id, primaryBarcodeEntity.Id), out var foundStStock)
                    ? foundStStock
                    : null;
                if (stStock != null)
                {
                    if (storeQty > 0) stStock.Quantity = storeQty;
                    if (minStock > 0) stStock.MinStockLevel = minStock;
                }
                else
                {
                    stStock = new StorgeStock
                    {
                        Id = Guid.NewGuid(),
                        WarehouseId = targetStorageWh.Id,
                        ProductBarcodeId = primaryBarcodeEntity.Id,
                        Quantity = storeQty,
                        MinStockLevel = minStock
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(stStock, ct);
                    existingStorageStocks.Add(stStock);
                    storageStocksByBarcodeAndWarehouse[(targetStorageWh.Id, primaryBarcodeEntity.Id)] = stStock;
                    result.StocksInitialized++;
                }
            }

            // حفظ كل 500 صنف لترشيد استهلاك الذاكرة
            if (r % 500 == 0)
            {
                await SaveTrackedChangesAsync(ct);
            }
        }

        await SaveTrackedChangesAsync(ct);
    }

    private async Task ProcessMultiSheetImportAsync(
        XLWorkbook workbook,
        ProductExcelImportOptionsDto options,
        Warehouse? targetShowroom,
        Warehouse? targetStorageWh,
        ProductImportResultDto result,
        CancellationToken ct)
    {
        var wsCategories = workbook.Worksheets.FirstOrDefault(w => IsCategoriesSheetName(w.Name)) ?? workbook.Worksheets.ElementAtOrDefault(2);
        var wsProducts = workbook.Worksheets.FirstOrDefault(w => IsProductsSheetName(w.Name)) ?? workbook.Worksheets.FirstOrDefault();
        var wsBarcodes = workbook.Worksheets.FirstOrDefault(w => IsBarcodesSheetName(w.Name)) ?? workbook.Worksheets.ElementAtOrDefault(1);

        if (wsProducts == null)
        {
            result.Errors.Add("لم يتم العثور على ورقة الأصناف في ملف Excel");
            return;
        }

        var categoryCodeMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var categoryNameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var productCodeMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var productNameMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var existingCategories = (await _unitOfWork.Categories.GetAllTrackedAsync(ct)).ToList();
        // فهرس قاموسي للتصنيفات بدل المسح الخطي داخل الحلقات
        var categoriesById = existingCategories.ToDictionary(c => c.Id, c => c);
        foreach (var cat in existingCategories)
        {
            if (!string.IsNullOrWhiteSpace(cat.Name))
                categoryNameMap[cat.Name.Trim()] = cat.Id;
        }

        // 1. معالجة التصنيفات
        if (wsCategories != null && wsCategories != wsProducts)
        {
            int lastCatRow = wsCategories.LastRowUsed()?.RowNumber() ?? 1;
            for (int r = 2; r <= lastCatRow; r++)
            {
                string catIdCode = wsCategories.Cell(r, 1).GetString().Trim();
                string catName = wsCategories.Cell(r, 2).GetString().Trim();
                if (string.IsNullOrWhiteSpace(catName)) continue;

                Category? targetCat = null;
                if (!string.IsNullOrWhiteSpace(catIdCode) && Guid.TryParse(catIdCode, out var gCat) && categoriesById.TryGetValue(gCat, out var byIdCat))
                    targetCat = byIdCat;

                if (targetCat == null && categoryNameMap.TryGetValue(catName, out var existingIdByName) && categoriesById.TryGetValue(existingIdByName, out var byNameCat))
                    targetCat = byNameCat;

                if (targetCat == null && options.AutoCreateCategories)
                {
                    targetCat = new Category { Id = Guid.NewGuid(), Name = catName, IsActive = true };
                    await _unitOfWork.Categories.AddAsync(targetCat, ct);
                    existingCategories.Add(targetCat);
                    categoryNameMap[catName] = targetCat.Id;
                    result.CategoriesImported++;
                }

                if (targetCat != null)
                {
                    categoryNameMap[catName] = targetCat.Id;
                    if (!string.IsNullOrWhiteSpace(catIdCode))
                        categoryCodeMap[catIdCode] = targetCat.Id;
                }
            }
        }

        // تصنيف عام افتراضي
        Category? defaultCategory = existingCategories.FirstOrDefault(c => c.Name.Equals("عام", StringComparison.OrdinalIgnoreCase));
        if (defaultCategory == null)
        {
            defaultCategory = new Category { Id = Guid.NewGuid(), Name = "عام", IsActive = true };
            await _unitOfWork.Categories.AddAsync(defaultCategory, ct);
            existingCategories.Add(defaultCategory);
            categoryNameMap[defaultCategory.Name] = defaultCategory.Id;
            result.CategoriesImported++;
        }

        await SaveTrackedChangesAsync(ct);

        // 2. معالجة الأصناف
        var existingProducts = (await _unitOfWork.Products.GetAllTrackedAsync(ct)).ToList();
        // فهارس قاموسية للأصناف وأرصدة الصالة بدل المسح الخطي لكل صف
        var productsById = existingProducts.ToDictionary(p => p.Id, p => p);
        var productsByName = existingProducts.ToDictionary(p => p.Name.Trim(), p => p, StringComparer.OrdinalIgnoreCase);
        foreach (var prod in existingProducts)
        {
            if (!string.IsNullOrWhiteSpace(prod.Name))
                productNameMap[prod.Name.Trim()] = prod.Id;
        }

        var existingShowroomStocks = (await _unitOfWork.ShowroomStocks.GetAllTrackedAsync(ct)).ToList();
        var showroomStocksByProductAndWarehouse = existingShowroomStocks
            .ToDictionary(s => (s.WarehouseId, s.ProductId), s => s);
        var processedProductsList = new List<(Product Product, string? RawCode)>();

        int lastProdRow = wsProducts.LastRowUsed()?.RowNumber() ?? 1;
        for (int r = 2; r <= lastProdRow; r++)
        {
            string col1 = wsProducts.Cell(r, 1).GetString().Trim();
            string col2 = wsProducts.Cell(r, 2).GetString().Trim();
            string col3 = wsProducts.Cell(r, 3).GetString().Trim();
            string col4 = wsProducts.Cell(r, 4).GetString().Trim();
            string col5 = wsProducts.Cell(r, 5).GetString().Trim();

            if (string.IsNullOrWhiteSpace(col1) && string.IsNullOrWhiteSpace(col2)) continue;

            string? rawProdIdCode = col1;
            string prodName = string.IsNullOrWhiteSpace(col2) ? col1 : col2;
            decimal.TryParse(col3, out var costPrice);
            decimal.TryParse(col4, out var salePrice);
            string catInputStr = col5;

            Guid finalCategoryId = defaultCategory.Id;
            if (!string.IsNullOrWhiteSpace(catInputStr))
            {
                if (categoryCodeMap.TryGetValue(catInputStr, out var cId)) finalCategoryId = cId;
                else if (categoryNameMap.TryGetValue(catInputStr, out var cIdByName)) finalCategoryId = cIdByName;
                else if (Guid.TryParse(catInputStr, out var rawCatGuid) && categoriesById.ContainsKey(rawCatGuid)) finalCategoryId = rawCatGuid;
                else if (options.AutoCreateCategories)
                {
                    var newCat = new Category { Id = Guid.NewGuid(), Name = catInputStr, IsActive = true };
                    await _unitOfWork.Categories.AddAsync(newCat, ct);
                    existingCategories.Add(newCat);
                    categoryNameMap[catInputStr] = newCat.Id;
                    finalCategoryId = newCat.Id;
                    result.CategoriesImported++;
                }
            }

            Product? targetProduct = null;
            if (!string.IsNullOrWhiteSpace(rawProdIdCode) && productCodeMap.TryGetValue(rawProdIdCode, out var pIdByCode) && productsById.TryGetValue(pIdByCode, out var byCodeProd))
                targetProduct = byCodeProd;

            if (targetProduct == null && !string.IsNullOrWhiteSpace(rawProdIdCode) && Guid.TryParse(rawProdIdCode, out var parsedGuid) && productsById.TryGetValue(parsedGuid, out var byGuidProd))
                targetProduct = byGuidProd;

            if (targetProduct == null && productsByName.TryGetValue(prodName, out var byNameProd))
                targetProduct = byNameProd;

            if (targetProduct != null)
            {
                if (options.ImportMode != ProductImportMode.InsertOnly)
                {
                    targetProduct.Name = prodName;
                    targetProduct.CostPrice = costPrice;
                    targetProduct.SalePrice = salePrice;
                    targetProduct.CategoryId = finalCategoryId;
                    result.ProductsImported++;
                }
            }
            else
            {
                if (options.ImportMode != ProductImportMode.UpdateExistingOnly)
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
            }

            if (targetProduct != null)
            {
                if (!string.IsNullOrWhiteSpace(rawProdIdCode)) productCodeMap[rawProdIdCode] = targetProduct.Id;
                productNameMap[prodName] = targetProduct.Id;
                processedProductsList.Add((targetProduct, rawProdIdCode));

                // تهيئة مخزون الصالة الافتراضي — بحث O(1) بالفهرس المسبق
                if (targetShowroom != null)
                {
                    if (!showroomStocksByProductAndWarehouse.ContainsKey((targetShowroom.Id, targetProduct.Id)))
                    {
                        var sStock = new ShowroomStock
                        {
                            Id = Guid.NewGuid(),
                            WarehouseId = targetShowroom.Id,
                            ProductId = targetProduct.Id,
                            Quantity = 0,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.ShowroomStocks.AddAsync(sStock, ct);
                        existingShowroomStocks.Add(sStock);
                        showroomStocksByProductAndWarehouse[(targetShowroom.Id, targetProduct.Id)] = sStock;
                        result.StocksInitialized++;
                    }
                }
            }
        }

        await SaveTrackedChangesAsync(ct);

        // 3. معالجة الباركودات المسجلة في ورقة الأكواد
        var existingBarcodes = (await _unitOfWork.ProductBarCodes.GetAllTrackedAsync(ct)).ToList();
        var existingStorageStocks = (await _unitOfWork.StorgeStocks.GetAllTrackedAsync(ct)).ToList();
        var productsWithBarcodes = new HashSet<Guid>(existingBarcodes.Select(b => b.ProductId));
        // فهارس قاموسية للباركودات وأرصدة التخزين بدل المسح الخطي لكل صف
        var barcodesByCode = existingBarcodes.ToDictionary(b => b.BarCode.Trim(), b => b, StringComparer.OrdinalIgnoreCase);
        var storageStocksByBarcodeAndWarehouse = existingStorageStocks
            .ToDictionary(s => (s.WarehouseId, s.ProductBarcodeId), s => s);

        if (wsBarcodes != null && wsBarcodes != wsProducts)
        {
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
                    else continue;
                }

                Guid targetProductId = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(rawProdIdStr) && productCodeMap.TryGetValue(rawProdIdStr, out var prodIdByCode)) targetProductId = prodIdByCode;
                else if (!string.IsNullOrWhiteSpace(rawProdIdStr) && productNameMap.TryGetValue(rawProdIdStr, out var prodIdByName)) targetProductId = prodIdByName;
                else if (!string.IsNullOrWhiteSpace(rawProdIdStr) && Guid.TryParse(rawProdIdStr, out var gP) && productsById.ContainsKey(gP))
                {
                    targetProductId = gP;
                }

                if (targetProductId == Guid.Empty)
                {
                    result.Warnings.Add($"السطر {r} في ورقة الباركودات تم تجاهله لعدم التعرف على الصنف المربوط ({rawProdIdStr})");
                    continue;
                }

                var parentProd = productsById.TryGetValue(targetProductId, out var foundProd) ? foundProd : null;
                if (parentProd == null) continue;

                ProductBarCode? targetBarCode = barcodesByCode.TryGetValue(barCode.Trim(), out var foundBarCode) ? foundBarCode : null;
                string finalTitle = string.IsNullOrWhiteSpace(title) ? parentProd.Name : title;

                if (targetBarCode != null)
                {
                    targetBarCode.BarCode = barCode;
                    targetBarCode.Title = finalTitle;
                    targetBarCode.ProductId = targetProductId;
                    productsWithBarcodes.Add(targetProductId);
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
                    barcodesByCode[targetBarCode.BarCode.Trim()] = targetBarCode;
                    productsWithBarcodes.Add(targetProductId);
                    result.BarcodesImported++;
                }

                // تهيئة مخزون التخزين — بحث O(1) بالفهرس المسبق
                if (targetStorageWh != null)
                {
                    if (!storageStocksByBarcodeAndWarehouse.ContainsKey((targetStorageWh.Id, targetBarCode.Id)))
                    {
                        var stStock = new StorgeStock
                        {
                            Id = Guid.NewGuid(),
                            WarehouseId = targetStorageWh.Id,
                            ProductBarcodeId = targetBarCode.Id,
                            Quantity = 0,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.StorgeStocks.AddAsync(stStock, ct);
                        existingStorageStocks.Add(stStock);
                        storageStocksByBarcodeAndWarehouse[(targetStorageWh.Id, targetBarCode.Id)] = stStock;
                        result.StocksInitialized++;
                    }
                }
            }

            await SaveTrackedChangesAsync(ct);
        }

        // 4. ضمان صرف كود لجميع الأصناف المستوردة التي ليس لها أي باركود
        if (options.AutoGenerateMissingBarcodes)
        {
            var usedBarcodesSet = new HashSet<string>(existingBarcodes.Select(b => b.BarCode.Trim()), StringComparer.OrdinalIgnoreCase);
            long barcodeSequence = InitializeBarcodeSequence(usedBarcodesSet);

            foreach (var (product, rawCode) in processedProductsList)
            {
                bool hasBarcode = productsWithBarcodes.Contains(product.Id);
                if (!hasBarcode)
                {
                    // لم يتم العثور على أي كود للصنف في صفحة أكواد المنتجات -> صرف كود جديد تلقائياً
                    string? preferredCandidate = (!string.IsNullOrWhiteSpace(rawCode) && !Guid.TryParse(rawCode, out _)) ? rawCode : null;
                    string generatedBarcode = AllocateOrGenerateBarcode(ref barcodeSequence, usedBarcodesSet, preferredCandidate);

                    var newBarcodeEntity = new ProductBarCode
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        BarCode = generatedBarcode,
                        Title = product.Name
                    };

                    await _unitOfWork.ProductBarCodes.AddAsync(newBarcodeEntity, ct);
                    existingBarcodes.Add(newBarcodeEntity);
                    productsWithBarcodes.Add(product.Id);
                    result.BarcodesImported++;
                    result.AutoGeneratedBarcodesCount++;

                    // تهيئة مخزون التخزين للصنف — بحث O(1) بالفهرس المسبق
                    if (targetStorageWh != null)
                    {
                        if (!storageStocksByBarcodeAndWarehouse.ContainsKey((targetStorageWh.Id, newBarcodeEntity.Id)))
                        {
                            var stStock = new StorgeStock
                            {
                                Id = Guid.NewGuid(),
                                WarehouseId = targetStorageWh.Id,
                                ProductBarcodeId = newBarcodeEntity.Id,
                                Quantity = 0,
                                MinStockLevel = 0
                            };
                            await _unitOfWork.StorgeStocks.AddAsync(stStock, ct);
                            existingStorageStocks.Add(stStock);
                            storageStocksByBarcodeAndWarehouse[(targetStorageWh.Id, newBarcodeEntity.Id)] = stStock;
                            result.StocksInitialized++;
                        }
                    }
                }
            }

            await SaveTrackedChangesAsync(ct);
        }
    }

    #endregion

    #region Helper Methods

    private static bool IsBarcodesSheetName(string name)
    {
        var n = name.Trim();
        return n.Equals(SheetBarcodes, StringComparison.OrdinalIgnoreCase) ||
               n.Equals("باركودات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("اكواد المنتجات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("أكواد المنتجات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("اكواد الاصناف", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("أكواد الأصناف", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("الأكواد", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("الاكواد", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Barcodes", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Codes", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("ProductBarcodes", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Product Codes", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("ProductCodes", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCategoriesSheetName(string name)
    {
        var n = name.Trim();
        return n.Equals(SheetCategories, StringComparison.OrdinalIgnoreCase) ||
               n.Equals("تصنيفات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("الفئات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("فئات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("المجموعات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("مجموعات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Categories", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Category", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Groups", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProductsSheetName(string name)
    {
        var n = name.Trim();
        return n.Equals(SheetProducts, StringComparison.OrdinalIgnoreCase) ||
               n.Equals("الاصناف", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("أصناف", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("اصناف", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("المنتجات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("منتجات", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Products", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Product", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Items", StringComparison.OrdinalIgnoreCase) ||
               n.Equals("Item", StringComparison.OrdinalIgnoreCase);
    }

    private static long InitializeBarcodeSequence(IEnumerable<string> existingBarcodes)
    {
        long baseSeq = 200000000001L;
        long maxFound = 0;

        foreach (var bc in existingBarcodes)
        {
            if (string.IsNullOrWhiteSpace(bc)) continue;
            var trimmed = bc.Trim();
            if (trimmed.Length >= 10 && trimmed.Length <= 14 && long.TryParse(trimmed, out var num))
            {
                if (num > maxFound)
                {
                    maxFound = num;
                }
            }
        }

        if (maxFound >= baseSeq)
        {
            return maxFound + 1;
        }

        return baseSeq;
    }

    private static string AllocateOrGenerateBarcode(
        ref long currentSeq,
        HashSet<string> usedBarcodes,
        string? preferredCandidate = null)
    {
        if (!string.IsNullOrWhiteSpace(preferredCandidate))
        {
            string candidate = preferredCandidate.Trim();
            if (!Guid.TryParse(candidate, out _) && !usedBarcodes.Contains(candidate))
            {
                usedBarcodes.Add(candidate);
                return candidate;
            }
        }

        while (true)
        {
            string candidate = currentSeq.ToString();
            currentSeq++;
            if (!usedBarcodes.Contains(candidate))
            {
                usedBarcodes.Add(candidate);
                return candidate;
            }
        }
    }

    private bool DetectIfSingleSheet(IXLWorkbook workbook)
    {
        if (workbook.Worksheets.Count == 1) return true;

        var firstWs = workbook.Worksheets.FirstOrDefault();
        if (firstWs == null) return true;

        // فحص عناوين الشيت الأول: إذا كانت تحتوي على الباركود مع السعر والاسم، فهو شيت مسطح
        int lastCol = firstWs.LastColumnUsed()?.ColumnNumber() ?? 1;
        bool hasBarcodeHeader = false;
        for (int c = 1; c <= lastCol; c++)
        {
            string h = firstWs.Cell(1, c).GetString().Trim();
            if (h.Contains("باركود", StringComparison.OrdinalIgnoreCase) || h.Contains("barcode", StringComparison.OrdinalIgnoreCase))
            {
                hasBarcodeHeader = true;
                break;
            }
        }

        if (hasBarcodeHeader) return true;

        // إذا كان هناك شيتات باسم الباركودات والتصنيفات فهو Multi-sheet
        bool hasBarcodesSheet = workbook.Worksheets.Any(w => IsBarcodesSheetName(w.Name));
        bool hasCategoriesSheet = workbook.Worksheets.Any(w => IsCategoriesSheetName(w.Name));

        return !(hasBarcodesSheet || hasCategoriesSheet);
    }

    private class SingleSheetColumnMapping
    {
        public int ItemCodeCol { get; set; } = 1;
        public int NameCol { get; set; } = 2;
        public int BarcodeCol { get; set; } = 3;
        public int CategoryCol { get; set; } = 4;
        public int UnitCol { get; set; } = 5;
        public int ConversionFactorCol { get; set; } = 6;
        public int CostPriceCol { get; set; } = 7;
        public int SalePriceCol { get; set; } = 8;
        public int ShowroomQtyCol { get; set; } = 9;
        public int StorageQtyCol { get; set; } = 10;
        public int MinStockCol { get; set; } = 11;
        public int DescriptionCol { get; set; } = 12;
    }

    private SingleSheetColumnMapping DetectSingleSheetColumns(IXLWorksheet ws)
    {
        var mapping = new SingleSheetColumnMapping();
        int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 12;

        for (int c = 1; c <= lastCol; c++)
        {
            string h = ws.Cell(1, c).GetString().Trim();
            if (string.IsNullOrWhiteSpace(h)) continue;

            if (MatchesHeader(h, "اسم الصنف", "اسم المنتج", "الاسم", "الصنف", "المنتج", "name", "product name", "item name", "product"))
                mapping.NameCol = c;
            else if (MatchesHeader(h, "الباركود", "باركود", "كود الباركود", "barcode", "bar code", "upc", "ean"))
                mapping.BarcodeCol = c;
            else if (MatchesHeader(h, "كود الصنف", "كود", "معرف الصنف", "المعرف", "رمز الصنف", "code", "itemcode", "item code", "id"))
                mapping.ItemCodeCol = c;
            else if (MatchesHeader(h, "التصنيف", "اسم التصنيف", "المجموعة", "الفئة", "القسم", "category", "category name", "group"))
                mapping.CategoryCol = c;
            else if (MatchesHeader(h, "الوحدة", "اسم الوحدة", "نوع الوحدة", "unit", "unit name", "uom"))
                mapping.UnitCol = c;
            else if (MatchesHeader(h, "معامل التحويل", "العبوة", "سعة العبوة", "معامل", "conversion factor", "factor"))
                mapping.ConversionFactorCol = c;
            else if (MatchesHeader(h, "سعر التكلفة", "التكلفة", "سعر الشراء", "شراء", "تكلفة", "cost", "cost price", "costprice", "purchase price"))
                mapping.CostPriceCol = c;
            else if (MatchesHeader(h, "سعر البيع", "سعر بيع", "البيع", "بيع", "السعر", "price", "sale price", "saleprice"))
                mapping.SalePriceCol = c;
            else if (MatchesHeader(h, "كمية الصالة", "رصيد الصالة", "المعرض", "صالة", "showroom qty", "showroom quantity"))
                mapping.ShowroomQtyCol = c;
            else if (MatchesHeader(h, "كمية المخزن", "رصيد المخزن", "المستودع", "مخزن", "storage qty", "storage quantity", "stock", "الكمية"))
                mapping.StorageQtyCol = c;
            else if (MatchesHeader(h, "حد الطلب", "الحد الأدنى", "حد إعادة الطلب", "min stock", "min stock level"))
                mapping.MinStockCol = c;
            else if (MatchesHeader(h, "الوصف", "ملاحظات", "البيان", "description", "notes"))
                mapping.DescriptionCol = c;
        }

        return mapping;
    }

    private static bool MatchesHeader(string header, params string[] candidates)
    {
        return candidates.Any(cand => header.Equals(cand, StringComparison.OrdinalIgnoreCase) ||
                                      header.StartsWith(cand + " ", StringComparison.OrdinalIgnoreCase) ||
                                      header.StartsWith(cand + "*", StringComparison.OrdinalIgnoreCase) ||
                                      header.Contains(cand, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetCellValue(IXLWorksheet ws, int row, int col)
    {
        if (col <= 0) return string.Empty;
        return ws.Cell(row, col).GetString().Trim();
    }

    private async Task SaveTrackedChangesAsync(CancellationToken ct)
    {
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
                catch { }
            }
        }

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var dbValues = await entry.GetDatabaseValuesAsync(ct);
                if (dbValues == null)
                {
                    entry.State = EntityState.Detached;
                }
                else
                {
                    entry.OriginalValues.SetValues(dbValues);
                }
            }
            await _unitOfWork.SaveChangesAsync(ct);
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
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }

    #endregion
}
