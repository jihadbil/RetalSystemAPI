using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Catalog;

public partial class ProductImportWizardViewModel : BaseViewModel
{
    private readonly IProductApiService _productApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    private string _selectedFileName = string.Empty;

    [ObservableProperty]
    private ProductImportMode _importMode = ProductImportMode.Upsert;

    [ObservableProperty]
    private bool _autoCreateCategories = true;

    [ObservableProperty]
    private bool _autoCreateUnits = true;

    [ObservableProperty]
    private bool _autoGenerateMissingBarcodes = true;

    [ObservableProperty]
    private Guid? _selectedShowroomWarehouseId;

    [ObservableProperty]
    private Guid? _selectedStorageWarehouseId;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _availableShowroomWarehouses = new();

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _availableStorageWarehouses = new();

    [ObservableProperty]
    private ProductExcelValidationResultDto? _validationResult;

    [ObservableProperty]
    private ObservableCollection<ProductExcelRowValidationDto> _validationRows = new();

    [ObservableProperty]
    private ObservableCollection<ProductExcelRowValidationDto> _filteredValidationRows = new();

    [ObservableProperty]
    private ProductExcelRowValidationDto? _selectedRow;

    [ObservableProperty]
    private string _manualBarcodeEntry = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = "all";

    [ObservableProperty]
    private ProductImportResultDto? _importResult;

    [ObservableProperty]
    private double _executionProgress = 0;

    [ObservableProperty]
    private string _executionStatusText = string.Empty;

    public Action? RequestCloseHandler { get; set; }
    public Action? ImportCompletedCallback { get; set; }

    public ProductImportWizardViewModel(
        IProductApiService productApiService,
        IWarehouseApiService warehouseApiService)
    {
        _productApiService = productApiService;
        _warehouseApiService = warehouseApiService;
        _ = LoadWarehousesAsync();
    }

    partial void OnSelectedRowChanged(ProductExcelRowValidationDto? value)
    {
        if (value != null && !string.IsNullOrWhiteSpace(value.BarCode) &&
            !value.BarCode.Contains("تلقائي") && !value.BarCode.Contains("بدون كود"))
        {
            ManualBarcodeEntry = value.BarCode;
        }
        else
        {
            ManualBarcodeEntry = string.Empty;
        }
    }

    private async Task LoadWarehousesAsync()
    {
        try
        {
            var res = await _warehouseApiService.GetAllAsync();
            if (res.Success && res.Data != null)
            {
                var shows = res.Data.Where(w => w.Type == WarehouseType.Show).ToList();
                var storages = res.Data.Where(w => w.Type == WarehouseType.Storge).ToList();

                AvailableShowroomWarehouses = new ObservableCollection<WarehouseSummaryDto>(shows.Count > 0 ? shows : res.Data);
                AvailableStorageWarehouses = new ObservableCollection<WarehouseSummaryDto>(storages.Count > 0 ? storages : res.Data);

                SelectedShowroomWarehouseId = AvailableShowroomWarehouses.FirstOrDefault()?.Id;
                SelectedStorageWarehouseId = AvailableStorageWarehouses.FirstOrDefault()?.Id;
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task BrowseFileAsync()
    {
        var ofd = new OpenFileDialog
        {
            Filter = "ملفات Excel (*.xlsx)|*.xlsx",
            Title = "اختر ملف Excel لاستيراد الأصناف"
        };

        if (ofd.ShowDialog() == true)
        {
            SelectedFilePath = ofd.FileName;
            SelectedFileName = Path.GetFileName(ofd.FileName);
            await RunValidationAsync();
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateSingleAsync()
    {
        var sfd = new SaveFileDialog
        {
            Filter = "ملف Excel (*.xlsx)|*.xlsx",
            FileName = "قالب_استيراد_الأصناف_المسطح.xlsx",
            Title = "حفظ قالب الاستيراد البسيط (ورقة واحدة)"
        };

        if (sfd.ShowDialog() == true)
        {
            await ExecuteAsync(async () =>
            {
                var bytes = await _productApiService.DownloadTemplateAsync(singleSheet: true);
                if (bytes != null && bytes.Length > 0)
                {
                    await File.WriteAllBytesAsync(sfd.FileName, bytes);
                    MessageBox.Show("تم تنزيل قالب الاستيراد البسيط بنجاح!", "تنزيل القالب", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "تعذر تنزيل القالب من الخادم";
                }
            });
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateMultiAsync()
    {
        var sfd = new SaveFileDialog
        {
            Filter = "ملف Excel (*.xlsx)|*.xlsx",
            FileName = "قالب_استيراد_الأصناف_المتعدد.xlsx",
            Title = "حفظ قالب الاستيراد المتقدم (3 أوراق عمل)"
        };

        if (sfd.ShowDialog() == true)
        {
            await ExecuteAsync(async () =>
            {
                var bytes = await _productApiService.DownloadTemplateAsync(singleSheet: false);
                if (bytes != null && bytes.Length > 0)
                {
                    await File.WriteAllBytesAsync(sfd.FileName, bytes);
                    MessageBox.Show("تم تنزيل قالب الاستيراد المتعدد بنجاح!", "تنزيل القالب", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "تعذر تنزيل القالب من الخادم";
                }
            });
        }
    }

    [RelayCommand]
    private async Task RunValidationAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilePath) || !File.Exists(SelectedFilePath))
        {
            ErrorMessage = "يرجى اختيار ملف Excel صالح أولاً";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var options = new ProductExcelImportOptionsDto
            {
                ImportMode = ImportMode,
                AutoCreateCategories = AutoCreateCategories,
                AutoCreateUnits = AutoCreateUnits,
                AutoGenerateMissingBarcodes = AutoGenerateMissingBarcodes,
                DefaultShowroomWarehouseId = SelectedShowroomWarehouseId,
                DefaultStorageWarehouseId = SelectedStorageWarehouseId
            };

            var res = await _productApiService.ValidateExcelAsync(SelectedFilePath, options);
            if (res.Success && res.Data != null)
            {
                ValidationResult = res.Data;
                ValidationRows = new ObservableCollection<ProductExcelRowValidationDto>(res.Data.Rows);
                SelectedRow = ValidationRows.FirstOrDefault();
                ApplyFilter("all");
                CurrentStep = 2; // الانتقال لخطوة المعاينة
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل فحص ملف Excel";
                MessageBox.Show($"فشل فحص الملف:\n{ErrorMessage}", "خطأ في الفحص", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    [RelayCommand]
    private void SetFilter(string filter)
    {
        SelectedFilter = filter;
        ApplyFilter(filter);
    }

    private void ApplyFilter(string filter)
    {
        if (ValidationRows == null) return;

        IEnumerable<ProductExcelRowValidationDto> query = filter switch
        {
            "errors" => ValidationRows.Where(r => r.Status == RowValidationStatus.Error),
            "warnings" => ValidationRows.Where(r => r.Status == RowValidationStatus.Warning),
            "new" => ValidationRows.Where(r => r.Action == RowActionType.Create && r.Status != RowValidationStatus.Error),
            "updates" => ValidationRows.Where(r => r.Action == RowActionType.Update && r.Status != RowValidationStatus.Error),
            "no_barcode" => ValidationRows.Where(r => r.IsAutoBarcode || r.IsCustomBarcode || string.IsNullOrWhiteSpace(r.BarCode) || r.BarCode.Contains("صرف كود") || r.BarCode.Contains("تلقائي") || r.BarCode.Contains("بدون كود")),
            _ => ValidationRows
        };

        FilteredValidationRows = new ObservableCollection<ProductExcelRowValidationDto>(query);
        if (SelectedRow == null || !FilteredValidationRows.Contains(SelectedRow))
        {
            SelectedRow = FilteredValidationRows.FirstOrDefault();
        }
    }

    [RelayCommand]
    private void AssignManualBarcode()
    {
        if (SelectedRow == null)
        {
            MessageBox.Show("يرجى تحديد صنف من الجدول أولاً لتعيين الكود له.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(ManualBarcodeEntry))
        {
            MessageBox.Show("يرجى كتابة أو مسح كود الباركود المطلوب.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string trimmed = ManualBarcodeEntry.Trim();

        // Check if barcode already used in other rows
        bool isDuplicate = ValidationRows.Any(r => r != SelectedRow && !string.IsNullOrWhiteSpace(r.BarCode) && r.BarCode.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (isDuplicate)
        {
            var mbr = MessageBox.Show($"الباركود '{trimmed}' مستخدم بالفعل لصنف آخر في الملف. هل تريد تعيينه على أية حال؟", "تنبيه تكرار الكود", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr != MessageBoxResult.Yes) return;
        }

        SelectedRow.BarCode = trimmed;
        SelectedRow.IsCustomBarcode = true;
        SelectedRow.IsAutoBarcode = false;
        SelectedRow.Message = "تم تحديد الكود يدوياً بنجاح";
        if (SelectedRow.Status == RowValidationStatus.Warning && (SelectedRow.Message.Contains("كود") || SelectedRow.Message.Contains("تلقائي")))
        {
            SelectedRow.Status = RowValidationStatus.Valid;
        }

        // Move to next row in list for fast barcode scanning workflow
        int idx = FilteredValidationRows.IndexOf(SelectedRow);
        if (idx >= 0 && idx < FilteredValidationRows.Count - 1)
        {
            SelectedRow = FilteredValidationRows[idx + 1];
            ManualBarcodeEntry = string.Empty;
        }
    }

    [RelayCommand]
    private void GenerateSequenceBarcode()
    {
        if (SelectedRow == null)
        {
            MessageBox.Show("يرجى تحديد صنف من الجدول أولاً.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string generated = GenerateNextUniqueBarcode();
        SelectedRow.BarCode = generated;
        SelectedRow.IsCustomBarcode = true;
        SelectedRow.IsAutoBarcode = false;
        SelectedRow.Message = "تم صرف كود تسلسلي يدوي";
        if (SelectedRow.Status == RowValidationStatus.Warning)
        {
            SelectedRow.Status = RowValidationStatus.Valid;
        }
        ManualBarcodeEntry = generated;
    }

    [RelayCommand]
    private void AutoGenerateAllMissingBarcodes()
    {
        if (ValidationRows == null || ValidationRows.Count == 0) return;

        int count = 0;
        foreach (var row in ValidationRows)
        {
            if (string.IsNullOrWhiteSpace(row.BarCode) || row.BarCode.Contains("صرف كود") || row.BarCode.Contains("تلقائي") || row.BarCode.Contains("بدون كود") || row.IsAutoBarcode)
            {
                string code = GenerateNextUniqueBarcode();
                row.BarCode = code;
                row.IsCustomBarcode = true;
                row.IsAutoBarcode = false;
                row.Message = "تم صرف كود تسلسلي يدوي";
                if (row.Status == RowValidationStatus.Warning)
                {
                    row.Status = RowValidationStatus.Valid;
                }
                count++;
            }
        }

        ApplyFilter(SelectedFilter);
        MessageBox.Show($"تم صرف وتعيين أكواد تسلسلية لعدد ({count}) صنف بنجاح!", "صرف الأكواد", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ClearBarcodeForRow()
    {
        if (SelectedRow == null) return;
        SelectedRow.BarCode = string.Empty;
        SelectedRow.IsCustomBarcode = false;
        SelectedRow.IsAutoBarcode = false;
        SelectedRow.Message = "الصنف بدون كود (يتطلب إدخال كود)";
        SelectedRow.Status = RowValidationStatus.Warning;
        ManualBarcodeEntry = string.Empty;
    }

    private string GenerateNextUniqueBarcode()
    {
        var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long maxFound = 200000000000L;

        if (ValidationRows != null)
        {
            foreach (var r in ValidationRows)
            {
                if (!string.IsNullOrWhiteSpace(r.BarCode) && !r.BarCode.Contains("تلقائي") && !r.BarCode.Contains("بدون كود"))
                {
                    string bc = r.BarCode.Trim();
                    usedCodes.Add(bc);
                    if (bc.Length >= 10 && bc.Length <= 14 && long.TryParse(bc, out var num))
                    {
                        if (num > maxFound) maxFound = num;
                    }
                }
            }
        }

        long candidate = maxFound + 1;
        while (usedCodes.Contains(candidate.ToString()))
        {
            candidate++;
        }
        return candidate.ToString();
    }

    [RelayCommand]
    private void GoToStep(object? stepObj)
    {
        int step = 1;
        if (stepObj is int i) step = i;
        else if (stepObj != null && int.TryParse(stepObj.ToString(), out int parsed)) step = parsed;

        if (step == 2 && ValidationResult == null && !string.IsNullOrWhiteSpace(SelectedFilePath))
        {
            _ = RunValidationAsync();
            return;
        }
        CurrentStep = step;
    }

    [RelayCommand]
    private async Task ExecuteImportAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFilePath) || !File.Exists(SelectedFilePath))
        {
            ErrorMessage = "ملف Excel غير موجود";
            return;
        }

        CurrentStep = 3; // شاشة التقدم
        ExecutionProgress = 15;
        ExecutionStatusText = "جاري تحضير ومزامنة بيانات الملف...";

        await ExecuteAsync(async () =>
        {
            string fileToUpload = SelectedFilePath;
            bool hasCustomBarcodes = ValidationRows.Any(r => r.IsCustomBarcode);

            if (hasCustomBarcodes)
            {
                try
                {
                    string tempFile = Path.Combine(Path.GetTempPath(), $"Retal_Import_{Guid.NewGuid():N}.xlsx");
                    File.Copy(SelectedFilePath, tempFile, true);

                    using (var workbook = new ClosedXML.Excel.XLWorkbook(tempFile))
                    {
                        if (ValidationResult?.IsSingleSheetFormat == true)
                        {
                            var ws = workbook.Worksheets.FirstOrDefault();
                            if (ws != null)
                            {
                                int barcodeCol = 3;
                                int lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 12;
                                for (int c = 1; c <= lastCol; c++)
                                {
                                    string h = ws.Cell(1, c).GetString().Trim();
                                    if (h.Contains("باركود", StringComparison.OrdinalIgnoreCase) || h.Contains("barcode", StringComparison.OrdinalIgnoreCase))
                                    {
                                        barcodeCol = c;
                                        break;
                                    }
                                }

                                foreach (var r in ValidationRows)
                                {
                                    if (!string.IsNullOrWhiteSpace(r.BarCode) && !r.BarCode.Contains("تلقائي") && !r.BarCode.Contains("بدون كود"))
                                    {
                                        ws.Cell(r.RowNumber, barcodeCol).Value = r.BarCode;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var wsBarcodes = workbook.Worksheets.FirstOrDefault(w =>
                                w.Name.Contains("باركود", StringComparison.OrdinalIgnoreCase) ||
                                w.Name.Contains("Barcode", StringComparison.OrdinalIgnoreCase) ||
                                w.Name.Contains("كود", StringComparison.OrdinalIgnoreCase) ||
                                w.Name.Contains("Code", StringComparison.OrdinalIgnoreCase));

                            if (wsBarcodes == null)
                            {
                                wsBarcodes = workbook.Worksheets.Add("الباركودات");
                                wsBarcodes.Cell(1, 1).Value = "كود الباركود";
                                wsBarcodes.Cell(1, 2).Value = "كود الصنف";
                                wsBarcodes.Cell(1, 3).Value = "الباركود";
                                wsBarcodes.Cell(1, 4).Value = "العنوان";
                            }

                            int lastBarRow = wsBarcodes.LastRowUsed()?.RowNumber() ?? 1;
                            var existingInSheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                            for (int br = 2; br <= lastBarRow; br++)
                            {
                                string prodIdStr = wsBarcodes.Cell(br, 2).GetString().Trim();
                                if (!string.IsNullOrWhiteSpace(prodIdStr)) existingInSheet[prodIdStr] = br;
                            }

                            foreach (var r in ValidationRows)
                            {
                                if (r.IsCustomBarcode && !string.IsNullOrWhiteSpace(r.BarCode) && !r.BarCode.Contains("تلقائي") && !r.BarCode.Contains("بدون كود"))
                                {
                                    string lookupKey = !string.IsNullOrWhiteSpace(r.ItemCode) ? r.ItemCode : r.Name;
                                    if (existingInSheet.TryGetValue(lookupKey, out int targetRow))
                                    {
                                        wsBarcodes.Cell(targetRow, 3).Value = r.BarCode;
                                    }
                                    else
                                    {
                                        lastBarRow++;
                                        wsBarcodes.Cell(lastBarRow, 1).Value = lastBarRow - 1;
                                        wsBarcodes.Cell(lastBarRow, 2).Value = lookupKey;
                                        wsBarcodes.Cell(lastBarRow, 3).Value = r.BarCode;
                                        wsBarcodes.Cell(lastBarRow, 4).Value = r.Name;
                                        existingInSheet[lookupKey] = lastBarRow;
                                    }
                                }
                            }
                        }
                        workbook.Save();
                    }
                    fileToUpload = tempFile;
                }
                catch { }
            }

            await Task.Delay(200);
            ExecutionProgress = 40;
            ExecutionStatusText = "جاري مطابقة التصنيفات والوحدات وتهيئة المخازن...";

            var options = new ProductExcelImportOptionsDto
            {
                ImportMode = ImportMode,
                AutoCreateCategories = AutoCreateCategories,
                AutoCreateUnits = AutoCreateUnits,
                AutoGenerateMissingBarcodes = AutoGenerateMissingBarcodes,
                DefaultShowroomWarehouseId = SelectedShowroomWarehouseId,
                DefaultStorageWarehouseId = SelectedStorageWarehouseId
            };

            await Task.Delay(200);
            ExecutionProgress = 70;
            ExecutionStatusText = "جاري حفظ الأصناف والباركودات والأرصدة في قاعدة البيانات...";

            var res = await _productApiService.ImportExcelAsync(fileToUpload, options);

            ExecutionProgress = 100;
            if (res.Success && res.Data != null)
            {
                ImportResult = res.Data;
                ExecutionStatusText = "اكتملت العملية بنجاح!";
                CurrentStep = 4; // شاشة الملخص
                ImportCompletedCallback?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message ?? "فشلت عملية استيراد الأصناف";
                ExecutionStatusText = $"فشلت العملية: {ErrorMessage}";
                MessageBox.Show($"فشلت عملية الاستيراد:\n{ErrorMessage}", "خطأ في الاستيراد", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentStep = 2;
            }
        });
    }

    [RelayCommand]
    private async Task ExportFailedRowsAsync()
    {
        if (ImportResult == null || ImportResult.FailedRows.Count == 0)
        {
            MessageBox.Show("لا توجد أي أسطر مرفوضة لتصديرها.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var sfd = new SaveFileDialog
        {
            Filter = "ملف Excel (*.xlsx)|*.xlsx",
            FileName = $"الأسطر_المرفوضة_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
            Title = "حفظ ملف الأسطر المرفوضة"
        };

        if (sfd.ShowDialog() == true)
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                workbook.RightToLeft = true;
                var ws = workbook.Worksheets.Add("الأسطر المرفوضة");
                ws.RightToLeft = true;

                // Headers
                string[] headers = { "رقم السطر", "كود الصنف", "اسم الصنف", "الباركود", "التصنيف", "الوحدة", "سعر التكلفة", "سعر البيع", "كمية الصالة", "كمية المخزن", "سبب الرفض والخطأ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#DC2626");
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                }

                int r = 2;
                foreach (var f in ImportResult.FailedRows)
                {
                    ws.Cell(r, 1).Value = f.RowNumber;
                    ws.Cell(r, 2).Value = f.ItemCode ?? "";
                    ws.Cell(r, 3).Value = f.Name ?? "";
                    ws.Cell(r, 4).Value = f.BarCode ?? "";
                    ws.Cell(r, 5).Value = f.CategoryName ?? "";
                    ws.Cell(r, 6).Value = f.UnitName ?? "";
                    ws.Cell(r, 7).Value = f.CostPrice;
                    ws.Cell(r, 8).Value = f.SalePrice;
                    ws.Cell(r, 9).Value = f.ShowroomQuantity;
                    ws.Cell(r, 10).Value = f.StorageQuantity;
                    ws.Cell(r, 11).Value = f.ErrorReason;
                    r++;
                }

                ws.Columns().AdjustToContents();
                workbook.SaveAs(sfd.FileName);

                MessageBox.Show("تم حفظ ملف الأسطر المرفوضة بنجاح! يمكنك تصحيحه وإعادة رفعه.", "تم التصدير", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تصدير الملف: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void Close()
    {
        RequestCloseHandler?.Invoke();
    }
}
