using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Catalog.Specifications;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة كتالوج المنتجات والأصناف والباركودات وتوليد سجلات الأرصدة الافتتاحية في المخازن والصالات.
/// </summary>
public class ProductService : IProductService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة المنتجات مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> GetAllAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام ملخص المنتجات مع فلتر التصنيف الاختياري
        var spec = new ProductSummarySpec(categoryId);
        // جلب قائمة المنتجات المطابقة للمواصفة
        var products = await _unitOfWork.Products.FindAsync(spec, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var result = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(products);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المنتج بالمعرف مع تفاصيل الوحدات والباركودات والصور
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(id), ct);
        // التحقق من وجود المنتج في قاعدة البيانات
        if (product is null)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة المفصل
        var result = _mapper.Map<ProductResponseDto>(product);
        // إرجاع النتيجة الناجحة مع بيانات المنتج
        return ServiceResult<ProductResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        CancellationToken ct = default)
    {
        // بناء مواصفة استعلام المنتجات للترقيم مع فلتر التصنيف
        var spec = new ProductSummarySpec(categoryId);
        // تنفيذ الاستعلام الصفحي لجلب سجلات الصفحة الحالية والعدد الإجمالي
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(items);
        // إنشاء كائن النتيجة المصفحة مع احتساب إجمالي الصفحات
        var pagedResult = PagedResult<ProductSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة المصفحة بنجاح
        return ServiceResult<PagedResult<ProductSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default)
    {
        // التحقق من تزويد كلمة للبحث
        if (string.IsNullOrWhiteSpace(query))
        {
            // إرجاع مصفوفة فارغة في حال كان نص البحث فارغاً
            return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(Array.Empty<ProductSummaryDto>());
        }

        // بناء مواصفة البحث بالاسم والباركود
        var spec = new ProductSearchSpec(query);
        // جلب المنتجات المطابقة للبحث
        var products = await _unitOfWork.Products.FindAsync(spec, ct);

        // التطابق التام للباركود أولاً ثم الأسماء التي تبدأ بالكلمة ثم بقية النتائج
        var ordered = products
            .OrderBy(p => p.ProductBarCodes.Any(b => b.BarCode == query) ? 0 : 1)
            .ThenBy(p => p.Name.StartsWith(query) ? 0 : 1)
            .ThenBy(p => p.Name)
            .ToList();

        // تحويل النتائج المرتبة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(ordered);

        // إرجاع قائمة نتائج البحث بنجاح
        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default)
    {
        // الاستعلام عن سجل الباركود في جدول باركودات المنتجات
        var barCodeEntry = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(b => b.BarCode == barCode, ct);
        // التحقق من وجود الباركود
        if (barCodeEntry is null)
        {
            // إرجاع خطأ بعدم العثور على الباركود
            return ServiceResult<ProductResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // جلب تفاصيل المنتج المرتبط بهذا الباركود بواسطة المعرف
        return await GetByIdAsync(barCodeEntry.ProductId, ct);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة وجود التصنيف التابع له المنتج
        bool categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId, ct);
        // في حال عدم وجود التصنيف
        if (!categoryExists)
        {
            // إرجاع خطأ بعدم وجود التصنيف
            return ServiceResult<ProductResponseDto>.Failure("التصنيف المحدد غير موجود", ErrorCodes.CategoryNotFound);
        }

        // التحقق من إيجابية الأسعار
        if (dto.CostPrice < 0 || dto.SalePrice < 0)
        {
            // إرجاع خطأ بوجوب عدم سلبية الأسعار
            return ServiceResult<ProductResponseDto>.Failure("أسعار المنتج يجب أن لا تكون سالبة", ErrorCodes.ValidationError);
        }

        // تحويل بيانات الإدخال إلى كيان المنتج
        var product = _mapper.Map<Product>(dto);

        // معالجة وحدات القياس المسندة للمنتج
        if (dto.Units != null && dto.Units.Count > 0)
        {
            // إنشاء كيانات ProductUnit لكل وحدة مدخلة
            product.ProductUnits = dto.Units.Select(u => new ProductUnit
            {
                // معرف الوحدة
                UnitId = u.UnitId,
                // معامل التحويل
                ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                // تحديد ما إذا كانت افتراضية
                IsDefault = u.IsDefault
            }).ToList();
        }

        // معالجة الباركودات والنكهات والتحقق من عدم تكرارها
        if (dto.BarCodes != null && dto.BarCodes.Count > 0)
        {
            // استخراج نصوص الباركودات وتنظيفها من الفراغات
            var barCodeStrings = dto.BarCodes.Select(b => b.BarCode.Trim()).ToList();
            // فحص التكرار داخل نفس طلب الإدخال
            bool duplicateInDto = barCodeStrings.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1);
            // في حال وجود تكرار داخلي
            if (duplicateInDto)
            {
                // إرجاع خطأ تكرار الباركود في الطلب
                return ServiceResult<ProductResponseDto>.Failure("يوجد باركود مكرر في البيانات المدخلة", ErrorCodes.BarCodeDuplicate);
            }

            // فحص ما إذا كان أي من الباركودات مسجلاً مسبقاً لصنف آخر في النظام
            var existingBarcode = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(
                b => barCodeStrings.Contains(b.BarCode), ct);
            // في حال وجود الباركود مسبقاً
            if (existingBarcode != null)
            {
                // إرجاع خطأ تكرار الباركود
                return ServiceResult<ProductResponseDto>.Failure($"الباركود '{existingBarcode.BarCode}' مسجل مسبقاً لصنف آخر في النظام", ErrorCodes.BarCodeDuplicate);
            }

            // إنشاء كيانات ProductBarCode للباركودات
            product.ProductBarCodes = dto.BarCodes.Select(b => new ProductBarCode
            {
                // رقم الباركود
                BarCode = b.BarCode.Trim(),
                // عنوان النكهة أو الصنف
                Title = string.IsNullOrWhiteSpace(b.Title) ? dto.Name : b.Title,
                // وصف إضافي
                Description = b.Description
            }).ToList();
        }

        // إضافة المنتج الجديد إلى المستودع
        await _unitOfWork.Products.AddAsync(product, ct);
        // حفظ التغييرات في قاعدة البيانات لتوليد المعرفات
        await _unitOfWork.SaveChangesAsync(ct);

        // 1. توليد سجل مخزون الصالة لكل صالة عرض قائمة (مع التراجع لجلب كافة المخازن إن لم تتوفر صالات مصنفة)
        var showrooms = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Show, ct);
        // في حال عدم وجود صالات عرض مصنفة
        if (showrooms.Count == 0)
        {
            // التراجع لجلب كافة المستودعات
            showrooms = await _unitOfWork.Warehouses.GetAllAsync(ct);
        }

        // بناء خريطة الأرصدة الافتتاحية لصالات العرض المتعددة
        var showroomQtyMap = dto.ShowroomInitialQuantities?.ToDictionary(s => s.WarehouseId, s => s.Quantity) ?? new Dictionary<Guid, int>();

        // المرور على صالات العرض وتأسيس أرصدة الصنف
        foreach (var show in showrooms)
        {
            // الكمية الافتتاحية الافتراضية
            int initShowQty = 0;
            // فحص وجود كمية مخصصة لهذه الصالة في الخريطة
            if (showroomQtyMap.TryGetValue(show.Id, out int showQty))
            {
                // تعيين الكمية المخصصة
                initShowQty = showQty;
            }
            // فحص تحديد صالة محددة برصيد مفرد
            else if (dto.ShowroomWarehouseId.HasValue)
            {
                if (show.Id == dto.ShowroomWarehouseId.Value)
                {
                    initShowQty = dto.InitialShowroomQuantity;
                }
            }
            // إسناد الكمية للصالة الأولى افتراضياً
            else
            {
                if (showrooms.Count > 0 && show.Id == showrooms[0].Id)
                {
                    initShowQty = dto.InitialShowroomQuantity;
                }
            }

            // إنشاء سجل رصيد صالة العرض
            var showStock = new ShowroomStock
            {
                TenantId = product.TenantId,
                WarehouseId = show.Id,
                ProductId = product.Id,
                Quantity = initShowQty,
                MinStockLevel = 0
            };
            // إضافة سجل رصيد الصالة
            await _unitOfWork.ShowroomStocks.AddAsync(showStock, ct);
        }

        // 2. توليد سجلات مخزون التخزين لكل باركود/نكهة في كل مخزن تخزين قائم (مع التراجع لجلب كافة المخازن)
        var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
        // في حال عدم وجود مخازن تخزين مصنفة
        if (storgeWarehouses.Count == 0)
        {
            // التراجع لجلب كافة المستودعات
            storgeWarehouses = await _unitOfWork.Warehouses.GetAllAsync(ct);
        }

        // تأسيس أرصدة المخازن للباركودات
        if (product.ProductBarCodes != null && product.ProductBarCodes.Count > 0)
        {
            // خريطة الكميات الافتتاحية المفردة للباركودات
            var initialQtyMap = dto.BarCodes?.ToDictionary(b => b.BarCode, b => b.InitialQuantity) ?? new Dictionary<string, int>();
            // خريطة الكميات الافتتاحية للمخازن المتعددة
            var storageMultiQtyMap = dto.StorageInitialQuantities?.ToDictionary(s => $"{s.WarehouseId}_{s.BarCode}", s => s.Quantity) ?? new Dictionary<string, int>();

            // المرور على مخازن التخزين
            foreach (var storgeWh in storgeWarehouses)
            {
                // المرور على باركودات ونكهات المنتج
                foreach (var bc in product.ProductBarCodes)
                {
                    // الكمية المبدئية
                    int initQty = 0;
                    // مفتاح الربط المركب بين المخزن والباركود
                    string key = $"{storgeWh.Id}_{bc.BarCode}";

                    // فحص وجود كمية مخصصة في الخريطة المتعددة
                    if (storageMultiQtyMap.TryGetValue(key, out int multiQty))
                    {
                        initQty = multiQty;
                    }
                    else
                    {
                        // فحص الكمية المطلوبة للباركود
                        int requestedQty = initialQtyMap.TryGetValue(bc.BarCode, out int q) ? q : 0;
                        // فحص المخزن المحدد
                        if (dto.StorageWarehouseId.HasValue)
                        {
                            if (storgeWh.Id == dto.StorageWarehouseId.Value)
                            {
                                initQty = requestedQty;
                            }
                        }
                        else
                        {
                            if (storgeWarehouses.Count > 0 && storgeWh.Id == storgeWarehouses[0].Id)
                            {
                                initQty = requestedQty;
                            }
                        }
                    }

                    // إنشاء سجل رصيد المخزن للباركود
                    var storgeStock = new StorgeStock
                    {
                        TenantId = product.TenantId,
                        WarehouseId = storgeWh.Id,
                        ProductBarcodeId = bc.Id,
                        Quantity = initQty,
                        MinStockLevel = 0
                    };
                    // إضافة سجل رصيد المخزن
                    await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
                }
            }
        }

        // حفظ سجلات الأرصدة الافتتاحية في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة تحميل المنتج مع كافة تفاصيله لضمان دقة كائن الاستجابة
        var createdProduct = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(product.Id), ct) ?? product;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<ProductResponseDto>(createdProduct);

        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        // تحميل متتبع (Tracked) لضمان حفظ تعديلات الأبناء (باركودات/وحدات) وحذف المحذوف منها فعلياً،
        // وتفادي تضارب تتبع النسخ المكررة عند تكرار نفس الصنف في البنود.
        var product = await _unitOfWork.Products.FirstOrDefaultTrackedAsync(new ProductWithDetailsSpec(id), ct);
        // التحقق من وجود المنتج
        if (product is null)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من التصنيف الجديد في حال تعديله
        if (dto.CategoryId != product.CategoryId)
        {
            // الاستعلام عن وجود التصنيف
            bool categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId, ct);
            // في حال عدم وجود التصنيف
            if (!categoryExists)
            {
                // إرجاع خطأ بعدم وجود التصنيف
                return ServiceResult<ProductResponseDto>.Failure("التصنيف المحدد غير موجود", ErrorCodes.CategoryNotFound);
            }
        }

        // التحقق من إيجابية الأسعار
        if (dto.CostPrice < 0 || dto.SalePrice < 0)
        {
            // إرجاع خطأ سلبية الأسعار
            return ServiceResult<ProductResponseDto>.Failure("أسعار المنتج يجب أن لا تكون سالبة", ErrorCodes.ValidationError);
        }

        // التحقق من فرادة الباركودات المدخلة إن تم تمريرها
        if (dto.BarCodes != null && dto.BarCodes.Count > 0)
        {
            // استخراج وتنسيق الباركودات
            var barCodeStrings = dto.BarCodes.Select(b => b.BarCode.Trim()).ToList();
            // فحص التكرار الداخلي في نفس الطلب
            bool duplicateInDto = barCodeStrings.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1);
            // في حال وجود تكرار
            if (duplicateInDto)
            {
                // إرجاع خطأ تكرار الباركود في الطلب
                return ServiceResult<ProductResponseDto>.Failure("يوجد باركود مكرر في البيانات المدخلة", ErrorCodes.BarCodeDuplicate);
            }

            // فحص ما إذا كان الباركود مستخدماً لصنف آخر غير الصنف الحالي
            var existingWithOtherProduct = await _unitOfWork.ProductBarCodes.ExistsAsync(
                b => barCodeStrings.Contains(b.BarCode) && b.ProductId != id, ct);
            // في حال كان مستخدماً لمنتج آخر
            if (existingWithOtherProduct)
            {
                // إرجاع خطأ استخدام الباركود لصنف آخر
                return ServiceResult<ProductResponseDto>.Failure("أحد الباركودات المدخلة مستخدم بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
            }
        }

        // تطبيق التعديلات الأساسية على الكيان
        _mapper.Map(dto, product);
        // تأكيد ثبات المعرف
        product.Id = id;

        // تحديث وحدات المنتج
        if (dto.Units != null)
        {
            // تجميع معرفات الوحدات في الطلب الجديد
            var dtoUnitIds = dto.Units.Select(u => u.UnitId).ToHashSet();

            // 1. إزالة الوحدات المستبعدة من الطلب
            var unitsToRemove = product.ProductUnits.Where(pu => !dtoUnitIds.Contains(pu.UnitId)).ToList();
            // حذف كل وحدة مستبعدة
            foreach (var u in unitsToRemove)
            {
                // إزالة من المجموعة
                product.ProductUnits.Remove(u);
            }

            // 2. تحديث الوحدات الموجودة أو إضافة الوحدات الجديدة
            foreach (var u in dto.Units)
            {
                // البحث عن الوحدة المسجلة مسبقاً
                var existingUnit = product.ProductUnits.FirstOrDefault(pu => pu.UnitId == u.UnitId);
                // في حال وجودها مسبقاً
                if (existingUnit != null)
                {
                    // تحديث معامل التحويل
                    existingUnit.ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1;
                    // تحديث حالة الافتراضية
                    existingUnit.IsDefault = u.IsDefault;
                }
                else
                {
                    // إضافة وحدة جديدة
                    product.ProductUnits.Add(new ProductUnit
                    {
                        ProductId = id,
                        UnitId = u.UnitId,
                        ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                        IsDefault = u.IsDefault
                    });
                }
            }
        }

        // تحديث باركودات ونكهات المنتج
        if (dto.BarCodes != null)
        {
            // تجميع أكواد الباركودات الجديدة
            var dtoBarCodes = dto.BarCodes.Select(b => b.BarCode.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 1. تحديد الباركودات المستبعدة من الطلب
            var barcodesToRemove = product.ProductBarCodes
                .Where(pb => !dtoBarCodes.Contains(pb.BarCode.Trim()))
                .ToList();

            // معالجة وحماية الباركودات المستبعدة
            if (barcodesToRemove.Count > 0)
            {
                // استخراج معرفات الباركودات المحذوفة
                var removedBarcodeIds = barcodesToRemove.Select(b => b.Id).ToList();

                // منع حذف باركود له حركة مخزنية أو فواتير أو صور مرتبطة
                bool usedInSales = await _unitOfWork.SalesInvoiceItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInSalesReturns = await _unitOfWork.SalesReturnItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInPurchases = await _unitOfWork.PurchaseInvoiceItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturnItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInOrders = await _unitOfWork.PurchaseOrderItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId), ct);
                bool hasStock = await _unitOfWork.StorgeStocks.ExistsAsync(
                    s => removedBarcodeIds.Contains(s.ProductBarcodeId) && s.Quantity != 0, ct);

                // في حال وجود أي استخدام سابق
                if (usedInSales || usedInSalesReturns || usedInPurchases || usedInPurchaseReturns || usedInOrders || hasStock)
                {
                    // إرجاع خطأ يمنع حذف الباركود المرتبط
                    return ServiceResult<ProductResponseDto>.Failure(
                        "لا يمكن حذف الباركود '" + string.Join("', '", barcodesToRemove.Select(b => b.BarCode)) +
                        "' لوجود حركة مخزنية أو فواتير مرتبطة به. يرجى الاحتفاظ به أو حذف الأصناف المرتبطة أولاً.",
                        ErrorCodes.BarCodeInUse);
                }

                // حذف منطقي متسلسل: الصور المرتبطة ثم أرصدة المخازن ثم الباركود نفسه
                var linkedImages = await _unitOfWork.ProductImages.FindTrackedAsync(
                    img => removedBarcodeIds.Contains(img.BarcodeId!.Value), ct);
                // حذف الصور المرتبطة بالباركود منطقياً
                foreach (var img in linkedImages)
                {
                    _unitOfWork.ProductImages.SoftDelete(img);
                }

                // استعلام الأرصدة المرتبطة بالباركود في المخازن
                var linkedStocks = await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => removedBarcodeIds.Contains(s.ProductBarcodeId), ct);
                // حذف الأرصدة منطقياً
                foreach (var stock in linkedStocks)
                {
                    _unitOfWork.StorgeStocks.SoftDelete(stock);
                }

                // حذف الباركود نفسه منطقياً
                foreach (var b in barcodesToRemove)
                {
                    _unitOfWork.ProductBarCodes.SoftDelete(b);
                }
            }

            // 2. تحديث الباركودات القائمة أو إضافة باركودات جديدة
            var newlyAddedBarCodes = new List<ProductBarCode>();
            foreach (var b in dto.BarCodes)
            {
                // تنظيف كود الباركود
                var trimmedCode = b.BarCode.Trim();
                // البحث عن الباركود القائم
                var existingBc = product.ProductBarCodes
                    .FirstOrDefault(pb => string.Equals(pb.BarCode.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase));

                // في حال وجوده مسبقاً يتم تحديث بياناته
                if (existingBc != null)
                {
                    existingBc.Title = string.IsNullOrWhiteSpace(b.Title) ? product.Name : b.Title.Trim();
                    existingBc.Description = b.Description;
                }
                else
                {
                    // إنشاء باركود جديد
                    var newBc = new ProductBarCode
                    {
                        ProductId = id,
                        BarCode = trimmedCode,
                        Title = string.IsNullOrWhiteSpace(b.Title) ? product.Name : b.Title.Trim(),
                        Description = b.Description
                    };
                    // إضافته للمنتج
                    product.ProductBarCodes.Add(newBc);
                    // إضافته لقائمة الباركودات المضافة حديثاً لتأسيس أرصدتها
                    newlyAddedBarCodes.Add(newBc);
                }
            }

            // 3. تأسيس أرصدة المخازن للباركودات والنكهات الجديدة المضافة
            if (newlyAddedBarCodes.Count > 0)
            {
                // جلب مخازن التخزين
                var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
                // التراجع لجلب كافة المستودعات إن لم تتوفر مخازن
                if (storgeWarehouses.Count == 0)
                {
                    storgeWarehouses = await _unitOfWork.Warehouses.GetAllAsync(ct);
                }

                // إنشاء رصيد صفري في كل مخزن للباركود الجديد
                foreach (var storgeWh in storgeWarehouses)
                {
                    foreach (var newBc in newlyAddedBarCodes)
                    {
                        var storgeStock = new StorgeStock
                        {
                            TenantId = product.TenantId,
                            WarehouseId = storgeWh.Id,
                            ProductBarcode = newBc,
                            Quantity = 0,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
                    }
                }
            }
        }

        try
        {
            // المنتج محمّل متتبعاً؛ SaveChanges يلتقط كل تعديلات الجذر والأبناء (إضافة/تعديل/حذف منطقي)
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // معالجة تعارض التعديل المتزامن
            return ServiceResult<ProductResponseDto>.Failure("حدث تعارض أثناء حفظ التعديلات، يرجى إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }

        // إعادة تحميل المنتج مع تفاصيله المحدثة
        var updatedProduct = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(id), ct) ?? product;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<ProductResponseDto>(updatedProduct);

        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المنتج بالمعرف
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
        // التحقق من وجود المنتج
        if (product is null)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // منع حذف صنف له حركة مبيعات أو مشتريات أو أرصدة قائمة
        bool usedInSalesInvoices = await _unitOfWork.SalesInvoiceItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInSalesReturns = await _unitOfWork.SalesReturnItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInPurchaseInvoices = await _unitOfWork.PurchaseInvoiceItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturnItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInStockTransfers = await _unitOfWork.StockTransferItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInStockAdjustments = await _unitOfWork.StockAdjustmentItems.ExistsAsync(i => i.ProductId == id, ct);

        // جلب معرفات الباركودات المرتبطة بالمنتج
        var barcodeIds = (await _unitOfWork.ProductBarCodes.FindAsync(b => b.ProductId == id, ct))
            .Select(b => b.Id)
            .ToList();

        // فحص وجود الصنف في أوامر الشراء
        bool usedInPurchaseOrders = barcodeIds.Count > 0 && await _unitOfWork.PurchaseOrderItems.ExistsAsync(
            i => barcodeIds.Contains(i.ProductBarCodeId), ct);

        // فحص وجود أرصدة غير صفرية في مخازن التخزين
        bool hasStorgeStock = barcodeIds.Count > 0 && await _unitOfWork.StorgeStocks.ExistsAsync(
            s => barcodeIds.Contains(s.ProductBarcodeId) && s.Quantity != 0, ct);
        // فحص وجود أرصدة غير صفرية في صالات العرض
        bool hasShowroomStock = await _unitOfWork.ShowroomStocks.ExistsAsync(
            s => s.ProductId == id && s.Quantity != 0, ct);

        // في حال وجود أي حركات سابقة أو رصيد غير صفري
        if (usedInSalesInvoices || usedInSalesReturns || usedInPurchaseInvoices || usedInPurchaseReturns ||
            usedInStockTransfers || usedInStockAdjustments || usedInPurchaseOrders || hasStorgeStock || hasShowroomStock)
        {
            // إرجاع رسالة المنع
            return ServiceResult.Failure(
                "لا يمكن حذف الصنف لوجود حركة مبيعات أو مشتريات أو أرصدة مخزنية مرتبطة به. يرجى تصفير أرصدته والتأكد من عدم ارتباطه بفواتير أو حركات مخزنية.",
                ErrorCodes.ProductInUse);
        }

        // حذف منطقي متسلسل للأبناء (الباركودات وصورها، الوحدات، صور المنتج، والأرصدة الصفرية)

        // جلب صور المنتج وحذفها منطقياً
        var productImages = await _unitOfWork.ProductImages.FindTrackedAsync(img => img.ProductId == id, ct);
        foreach (var img in productImages)
        {
            _unitOfWork.ProductImages.SoftDelete(img);
        }

        // جلب باركودات المنتج وحذفها منطقياً
        var productBarCodes = await _unitOfWork.ProductBarCodes.FindTrackedAsync(b => b.ProductId == id, ct);
        foreach (var bc in productBarCodes)
        {
            _unitOfWork.ProductBarCodes.SoftDelete(bc);
        }

        // جلب وحدات المنتج وحذفها منطقياً
        var productUnits = await _unitOfWork.ProductUnits.FindTrackedAsync(u => u.ProductId == id, ct);
        foreach (var unit in productUnits)
        {
            _unitOfWork.ProductUnits.SoftDelete(unit);
        }

        // جلب أرصدة صالات العرض وحذفها منطقياً
        var showroomStocks = await _unitOfWork.ShowroomStocks.FindTrackedAsync(s => s.ProductId == id, ct);
        foreach (var stock in showroomStocks)
        {
            _unitOfWork.ShowroomStocks.SoftDelete(stock);
        }

        // حذف المنتج نفسه منطقياً
        _unitOfWork.Products.SoftDelete(product);
        // حفظ كافة عمليات الحذف المنطقي في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
