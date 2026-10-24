using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Services.Warehouses.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة التحويلات المخزنية ونقل البضائع وتحديث الأرصدة في المصدر والوجهة.
/// </summary>
public class StockTransferService : IStockTransferService
{
    // وحدة العمل للتعامل مع مستودعات التحويلات والمخازن والأرصدة
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل النماذج بين قواعد البيانات وDTOs
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة التحويلات المخزنية مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public StockTransferService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockTransferResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام أمر التحويل بالمعرف مع تفاصيل المستودعات والبنود
        var transfer = await _unitOfWork.StockTransfers.GetDetailsAsync(id, ct);
        // التحقق من وجود أمر التحويل
        if (transfer is null)
        {
            // إرجاع خطأ عدم وجود أمر التحويل
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<StockTransferResponseDto>(transfer);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockTransferResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockTransferResponseDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default)
    {
        // استعلام أمر التحويل برقم التحويل
        var transfer = await _unitOfWork.StockTransfers.GetByNumberAsync(transferNumber, ct);
        // التحقق من وجود أمر التحويل
        if (transfer is null)
        {
            // إرجاع خطأ عدم وجود أمر التحويل
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<StockTransferResponseDto>(transfer);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockTransferResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(Guid? fromWarehouseId = null, Guid? toWarehouseId = null, StockTransferStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام القوائم الخفيفة لأوامر التحويل
        // جلب قائمة أوامر التحويل
        var transfers = await _unitOfWork.StockTransfers.ListAsync(new StockTransferQuery { FromWarehouseId = fromWarehouseId, ToWarehouseId = toWarehouseId, Status = status, FromDate = fromDate, ToDate = toDate, Search = search }, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(transfers);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StockTransferSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? fromWarehouseId = null, Guid? toWarehouseId = null, StockTransferStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة القوائم الخفيفة
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.StockTransfers.SearchAsync(new StockTransferQuery { FromWarehouseId = fromWarehouseId, ToWarehouseId = toWarehouseId, Status = status, FromDate = fromDate, ToDate = toDate, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(items);
        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<StockTransferSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<StockTransferSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockTransferResponseDto>> CreateAsync(CreateStockTransferDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<StockTransferResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من عدم مطابقة المستودع المصدر مع المستودع الهدف
        if (dto.FromWarehouseId == dto.ToWarehouseId)
        {
            // إرجاع خطأ التحويل لنفس المستودع
            return ServiceResult<StockTransferResponseDto>.Failure("لا يمكن التحويل لنفس المستودع أو الصالة", ErrorCodes.StockTransferSameWarehouse);
        }

        // استعلام المستودع المصدر
        var fromWh = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.FromWarehouseId, ct);
        // التحقق من وجود المستودع المصدر
        if (fromWh is null)
        {
            // إرجاع خطأ عدم وجود المستودع المصدر
            return ServiceResult<StockTransferResponseDto>.Failure("المستودع المصدر غير موجود", ErrorCodes.WarehouseNotFound);
        }

        // استعلام المستودع الوجهة
        var toWh = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.ToWarehouseId, ct);
        // التحقق من وجود المستودع الوجهة
        if (toWh is null)
        {
            // إرجاع خطأ عدم وجود المستودع الوجهة
            return ServiceResult<StockTransferResponseDto>.Failure("المستودع الوجهة غير موجود", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من عدم تكرار رقم أمر التحويل
        bool numExists = await _unitOfWork.StockTransfers.NumberExistsAsync(dto.TransferNumber, ct);
        // في حال وجود أمر تحويل بنفس الرقم
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم التحويل
            return ServiceResult<StockTransferResponseDto>.Failure("رقم أمر التحويل مستخدم بالفعل", ErrorCodes.StockTransferNumberExists);
        }

        // التحقق من احتواء أمر التحويل على بنود
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ تحقق عند غياب البنود
            return ServiceResult<StockTransferResponseDto>.Failure("يجب إضافة بند واحد على الأقل لأمر التحويل", ErrorCodes.ValidationError);
        }

        // تحويل DTO إلى كيان أمر التحويل
        var itemError = await ValidateItemsAsync(dto.Items.Select(i => (i.ProductId, i.ProductBarCodeId, i.Quantity)), fromWh, toWh, ct);
        if (itemError is not null)
            return ServiceResult<StockTransferResponseDto>.Failure(itemError, ErrorCodes.ValidationError);

        var transfer = _mapper.Map<StockTransfer>(dto);
        // ضبط تاريخ التحويل
        transfer.TransferDate = dto.TransferDate == default ? DateTime.UtcNow : dto.TransferDate;
        // تعيين الحالة الأولية كمسودة
        transfer.Status = StockTransferStatus.Draft;
        // بناء قائمة بنود التحويل
        transfer.Items = dto.Items.Select(item => new StockTransferItem { // تعيين معرف المنتج
 ProductId = item.ProductId, // تعيين معرف الباركود
 ProductBarCodeId = item.ProductBarCodeId, // تعيين الكمية المحولة
 Quantity = item.Quantity, // تعيين الملاحظات
 Notes = item.Notes }).ToList();
        // إضافة أمر التحويل وبنوده للمستودع
        await _unitOfWork.StockTransfers.AddAsync(transfer, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب أمر التحويل مع كامل التفاصيل
        var created = await _unitOfWork.StockTransfers.GetDetailsAsync(transfer.Id, ct) ?? transfer;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<StockTransferResponseDto>(created);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockTransferResponseDto>> UpdateAsync(Guid id, UpdateStockTransferDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<StockTransferResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        var transfer = await _unitOfWork.StockTransfers.GetDetailsForUpdateAsync(id, ct);
        if (transfer is null)
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);

        // منع تعديل الأوامر التي تم اكتمالها وترحيلها فعلياً
        if (transfer.Status == StockTransferStatus.Completed)
        {
            // إرجاع خطأ حالة الأمر
            return ServiceResult<StockTransferResponseDto>.Failure("لا يمكن تعديل أمر تحويل تم ترحيله وتنفيذه بالفعل", ErrorCodes.StockTransferInvalidStatus);
        }

        // تغيير الحالة محصور في مسار الاعتماد الذي يتحقق من صلاحية Approve.
        if (dto.Status != transfer.Status)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("استخدم مسار اعتماد التحويل لتغيير حالته", ErrorCodes.ValidationError);
        }

        transfer.Notes = dto.Notes;

        // وسم السجل للتحديث
        _unitOfWork.StockTransfers.Update(transfer);
        // حفظ التعديلات
        await _unitOfWork.SaveChangesAsync(ct);
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<StockTransferResponseDto>(transfer);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.GetDetailsForUpdateAsync(id, ct);
        if (transfer is null)
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        return await UpdateStatusCoreAsync(transfer, status, null, false, ct);
    }

    private async Task<ServiceResult<StockTransferResponseDto>> UpdateStatusCoreAsync(
        StockTransfer transfer, StockTransferStatus status, string? notes, bool updateNotes, CancellationToken ct)
    {
        if (!Enum.IsDefined(status))
            return ServiceResult<StockTransferResponseDto>.Failure("حالة التحويل غير صحيحة", ErrorCodes.ValidationError);
        if (transfer.Status is StockTransferStatus.Completed or StockTransferStatus.Cancelled ||
            (transfer.Status == StockTransferStatus.Confirmed && status == StockTransferStatus.Draft))
            return ServiceResult<StockTransferResponseDto>.Failure("لا يمكن تغيير حالة تحويل نهائي أو إعادته إلى مسودة", ErrorCodes.StockTransferInvalidStatus);

        if (status == StockTransferStatus.Completed)
        {
            var fromWh = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(transfer.FromWarehouseId, ct);
            var toWh = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(transfer.ToWarehouseId, ct);
            if (fromWh is null || toWh is null)
                return ServiceResult<StockTransferResponseDto>.Failure("أحد المستودعات غير موجود", ErrorCodes.WarehouseNotFound);
            var itemError = await ValidateItemsAsync(transfer.Items.Select(i => (i.ProductId, i.ProductBarCodeId, i.Quantity)), fromWh, toWh, ct);
            if (itemError is not null)
                return ServiceResult<StockTransferResponseDto>.Failure(itemError, ErrorCodes.ValidationError);

            // 1. التحقق من كفاية المخزون في المستودع المصدر
            if (fromWh.Type == WarehouseType.Storge)
            {
                // استخراج معرفات الباركودات المطلوبة للتحويل
                var barcodeIds = transfer.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                // التحقق من وجود بنود باركود
                if (barcodeIds.Count > 0)
                {
                    // تجميع إجمالي الكميات المطلوبة لكل باركود
                    var barcodeTotals = transfer.Items.Where(i => i.ProductBarCodeId.HasValue).GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(x => (long)x.Quantity));
                    // استرجاع أرصدة المصدر المتتبعة للباركودات
                    var sourceStocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(fromWh.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                    // فحص رصيد كل باركود ومقارنته بالكمية المطلوبة
                    foreach (var(productBarcodeId, totalQuantity)in barcodeTotals)
                    {
                        // التحقق من كفاية الرصيد
                        if (!sourceStocksByBarcode.TryGetValue(productBarcodeId, out var stock) || stock.Quantity < totalQuantity)
                        {
                            // إرجاع خطأ نقص المخزون في المصدر
                            return ServiceResult<StockTransferResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن المصدر للنكهة/الباركود المحدد", ErrorCodes.InsufficientStock);
                        }
                    }
                }
            }
            else if (fromWh.Type == WarehouseType.Show)
            {
                // استخراج معرفات المنتجات لصالة العرض
                var productIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();
                // فحص وجود بنود منتجات
                if (productIds.Count > 0)
                {
                    // تجميع إجمالي الكميات المطلوبة لكل منتج
                    var productTotals = transfer.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(x => (long)x.Quantity));
                    // استرجاع أرصدة الصالة المتتبعة
                    var sourceStocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(fromWh.Id, productIds, ct)).ToDictionary(s => s.ProductId);
                    // مقارنة رصيد كل صنف بالكمية المطلوبة
                    foreach (var(productId, totalQuantity)in productTotals)
                    {
                        // التحقق من كفاية الرصيد
                        if (!sourceStocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < totalQuantity)
                        {
                            // إرجاع خطأ نقص المخزون في الصالة
                            return ServiceResult<StockTransferResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض المصدر للصنف المحدد", ErrorCodes.InsufficientStock);
                        }
                    }
                }
            }

            // افحص سعة الوجهة قبل تعديل أي رصيد، بما يشمل البنود المتكررة.
            if (toWh.Type == WarehouseType.Storge)
            {
                var totals = transfer.Items.GroupBy(i => i.ProductBarCodeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var stocks = (await _unitOfWork.Stock.GetStorageForUpdateAsync(toWh.Id, totals.Keys.ToList(), ct))
                    .ToDictionary(s => s.ProductBarcodeId);
                if (totals.Any(t => t.Value > int.MaxValue - (long)(stocks.GetValueOrDefault(t.Key)?.Quantity ?? 0)))
                    return ServiceResult<StockTransferResponseDto>.Failure("الكمية تتجاوز سعة المخزون في المستودع الوجهة", ErrorCodes.ValidationError);
            }
            else
            {
                var totals = transfer.Items.GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var stocks = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(toWh.Id, totals.Keys.ToList(), ct))
                    .ToDictionary(s => s.ProductId);
                if (totals.Any(t => t.Value > int.MaxValue - (long)(stocks.GetValueOrDefault(t.Key)?.Quantity ?? 0)))
                    return ServiceResult<StockTransferResponseDto>.Failure("الكمية تتجاوز سعة المخزون في الصالة الوجهة", ErrorCodes.ValidationError);
            }

            // 2. تنفيذ خصم الكميات من المستودع المصدر
            if (fromWh.Type == WarehouseType.Storge)
            {
                var totals = transfer.Items.GroupBy(i => i.ProductBarCodeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var source = (await _unitOfWork.Stock.GetStorageForUpdateAsync(fromWh.Id, totals.Keys.ToList(), ct))
                    .ToDictionary(s => s.ProductBarcodeId);
                foreach (var total in totals)
                    source[total.Key].Quantity = checked(source[total.Key].Quantity - (int)total.Value);
            }
            else if (fromWh.Type == WarehouseType.Show)
            {
                var totals = transfer.Items.GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                var source = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(fromWh.Id, totals.Keys.ToList(), ct))
                    .ToDictionary(s => s.ProductId);
                foreach (var total in totals)
                    source[total.Key].Quantity = checked(source[total.Key].Quantity - (int)total.Value);
            }

            // 3. تنفيذ إضافة الكميات إلى المستودع الوجهة
            if (toWh.Type == WarehouseType.Storge)
            {
                // استخراج معرفات الباركودات للوجهة
                var toBarcodeIds = transfer.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                // استرجاع الأرصدة المتتبعة في مستودع الوجهة
                var targetStocksByBarcode = toBarcodeIds.Count > 0 ? (await _unitOfWork.Stock.GetStorageForUpdateAsync(toWh.Id, toBarcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId) : new Dictionary<Guid, StorgeStock>();
                // إضافة كل بند إلى رصيد الوجهة
                foreach (var item in transfer.Items)
                {
                    // التحقق من وجود معرف الباركود
                    if (!item.ProductBarCodeId.HasValue)
                        continue;
                    // إذا كان السجل موجوداً في الوجهة يتم زيادة الكمية
                    if (targetStocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                    {
                        // زيادة الكمية
                        stock.Quantity = checked(stock.Quantity + item.Quantity);
                    }
                    else
                    {
                        // في حال عدم وجود سجل رصيد مسبق في الوجهة يتم إنشاؤه
                        var newStock = new StorgeStock
                        {
                            TenantId = transfer.TenantId,
                            WarehouseId = toWh.Id,
                            ProductBarcodeId = item.ProductBarCodeId.Value,
                            Quantity = item.Quantity,
                            MinStockLevel = 0
                        };
                        // إضافة السجل الجديد
                        await _unitOfWork.Stock.AddStorgeStockAsync(newStock, ct);
                        // حفظه بالقاموس
                        targetStocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                    }
                }
            }
            else if (toWh.Type == WarehouseType.Show)
            {
                // استخراج معرفات المنتجات للوجهة
                var toProductIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();
                // استرجاع أرصدة الصالة الوجهة
                var targetStocksByProduct = toProductIds.Count > 0 ? (await _unitOfWork.Stock.GetShowroomForUpdateAsync(toWh.Id, toProductIds, ct)).ToDictionary(s => s.ProductId) : new Dictionary<Guid, ShowroomStock>();
                // إضافة كل بند إلى رصيد الصالة الوجهة
                foreach (var item in transfer.Items)
                {
                    // إذا كان سجل الرصيد موجوداً يتم زيادة الكمية
                    if (targetStocksByProduct.TryGetValue(item.ProductId, out var stock))
                    {
                        // زيادة الكمية
                        stock.Quantity = checked(stock.Quantity + item.Quantity);
                    }
                    else
                    {
                        // إنشاء سجل رصيد جديد في الصالة الوجهة
                        var newStock = new ShowroomStock
                        {
                            TenantId = transfer.TenantId,
                            WarehouseId = toWh.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            MinStockLevel = 0
                        };
                        // إضافة السجل الجديد
                        await _unitOfWork.Stock.AddShowroomStockAsync(newStock, ct);
                        // حفظه بالقاموس
                        targetStocksByProduct[item.ProductId] = newStock;
                    }
                }
            }
        }
        transfer.Status = status;
        if (updateNotes) transfer.Notes = notes;
        await _unitOfWork.SaveChangesAsync(ct);
        var updated = await _unitOfWork.StockTransfers.GetDetailsAsync(transfer.Id, ct) ?? transfer;
        return ServiceResult<StockTransferResponseDto>.Success(_mapper.Map<StockTransferResponseDto>(updated));
    }

    private async Task<string?> ValidateItemsAsync(IEnumerable<(Guid ProductId, Guid? BarcodeId, int Quantity)> items,
        Warehouse from, Warehouse to, CancellationToken ct)
    {
        var lines = items.ToList();
        if (from.Id == to.Id || !Enum.IsDefined(from.Type) || !Enum.IsDefined(to.Type) || lines.Count == 0)
            return "بيانات مستودعات التحويل أو بنوده غير صحيحة";
        var productIds = lines.Select(i => i.ProductId).Distinct().ToList();
        if ((await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).Count != productIds.Count)
            return "أحد أصناف التحويل غير موجود";
        var barcodeIds = lines.Where(i => i.BarcodeId.HasValue).Select(i => i.BarcodeId!.Value).Distinct().ToList();
        var barcodes = (await _unitOfWork.Products.GetBarcodesByIdsAsync(barcodeIds, ct)).ToDictionary(b => b.Id);
        foreach (var item in lines)
        {
            if (item.Quantity <= 0 ||
                (from.Type == WarehouseType.Storge || to.Type == WarehouseType.Storge) && !item.BarcodeId.HasValue ||
                item.BarcodeId.HasValue && (!barcodes.TryGetValue(item.BarcodeId.Value, out var barcode) || barcode.ProductId != item.ProductId))
                return "الكمية يجب أن تكون موجبة، والباركود مطلوب للتحويل من أو إلى المخزن ويجب أن يتبع الصنف";
        }
        return null;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام أمر التحويل ككيان متتبع
        var transfer = await _unitOfWork.StockTransfers.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود أمر التحويل
        if (transfer is null)
        {
            // إرجاع خطأ عدم وجود أمر التحويل
            return ServiceResult.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        // منع حذف أوامر التحويل المكتملة والمرحلة
        if (transfer.Status == StockTransferStatus.Completed)
        {
            // إرجاع خطأ حالة الأمر
            return ServiceResult.Failure("لا يمكن حذف أمر تحويل تم ترحيله وتنفيذه", ErrorCodes.StockTransferInvalidStatus);
        }

        // تطبيق الحذف المنطقي لأمر التحويل
        _unitOfWork.StockTransfers.SoftDelete(transfer);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
