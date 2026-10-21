using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
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
    /// <param name = "unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name = "mapper">خدمة تحويل النماذج</param>
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> GetAllAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام ملخص المنتجات مع فلتر التصنيف الاختياري
        // جلب قائمة المنتجات المطابقة للمواصفة
        var products = await _unitOfWork.Products.ListAsync(new ProductQuery { CategoryId = categoryId }, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var result = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(products);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(result);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المنتج بالمعرف مع تفاصيل الوحدات والباركودات والصور
        var product = await _unitOfWork.Products.GetDetailsAsync(id, ct);
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

    /// <inheritdoc/>
    public Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId = null, CancellationToken ct = default) =>
        GetPagedAsync(pageNumber, pageSize, categoryId, null, ct);

    public async Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? categoryId, string? search, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام المنتجات للترقيم مع فلتر التصنيف
        // تنفيذ الاستعلام الصفحي لجلب سجلات الصفحة الحالية والعدد الإجمالي
        var(items, totalCount) = await _unitOfWork.Products.SearchAsync(new ProductQuery { CategoryId = categoryId, Search = search?.Trim(), PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(items);
        // إنشاء كائن النتيجة المصفحة مع احتساب إجمالي الصفحات
        var pagedResult = PagedResult<ProductSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة المصفحة بنجاح
        return ServiceResult<PagedResult<ProductSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default)
    {
        // التحقق من تزويد كلمة للبحث
        if (string.IsNullOrWhiteSpace(query))
        {
            // إرجاع مصفوفة فارغة في حال كان نص البحث فارغاً
            return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(Array.Empty<ProductSummaryDto>());
        }

        // بناء مواصفة البحث بالاسم والباركود
        // جلب المنتجات المطابقة للبحث
        var products = await _unitOfWork.Products.ListAsync(new ProductQuery { Search = query }, ct);
        // التطابق التام للباركود أولاً ثم الأسماء التي تبدأ بالكلمة ثم بقية النتائج مع حد أقصى 100 نتيجة
        var ordered = products.OrderBy(p => p.ProductBarCodes.Any(b => b.BarCode == query) ? 0 : 1).ThenBy(p => p.Name.StartsWith(query) ? 0 : 1).ThenBy(p => p.Name).Take(100).ToList();
        // تحويل النتائج المرتبة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(ordered);
        // إرجاع قائمة نتائج البحث بنجاح
        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default)
    {
        // الاستعلام عن سجل الباركود في جدول باركودات المنتجات
        var barCodeEntry = await _unitOfWork.Products.GetBarcodeByValueAsync(barCode, ct);
        // التحقق من وجود الباركود
        if (barCodeEntry is null)
        {
            // إرجاع خطأ بعدم العثور على الباركود
            return ServiceResult<ProductResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // جلب تفاصيل المنتج المرتبط بهذا الباركود بواسطة المعرف
        return await GetByIdAsync(barCodeEntry.ProductId, ct);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProductResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (dto.ShowroomInitialQuantities?.Select(q => q.WarehouseId).Distinct().Count() != dto.ShowroomInitialQuantities?.Count ||
            dto.StorageInitialQuantities?.Select(q => (q.WarehouseId, q.BarCode.Trim().ToUpperInvariant())).Distinct().Count() != dto.StorageInitialQuantities?.Count)
            return ServiceResult<ProductResponseDto>.Failure("الأرصدة الافتتاحية تحتوي مفاتيح مخازن أو باركودات مكررة", ErrorCodes.ValidationError);
        var requestedWarehouses = (dto.ShowroomInitialQuantities ?? []).Select(q => (q.WarehouseId, WarehouseType.Show))
            .Concat((dto.StorageInitialQuantities ?? []).Select(q => (q.WarehouseId, WarehouseType.Storge)))
            .Concat(dto.ShowroomWarehouseId.HasValue ? [(dto.ShowroomWarehouseId.Value, WarehouseType.Show)] : [])
            .Concat(dto.StorageWarehouseId.HasValue ? [(dto.StorageWarehouseId.Value, WarehouseType.Storge)] : []);
        foreach (var (warehouseId, type) in requestedWarehouses.Distinct())
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(warehouseId, ct);
            if (warehouse is null || !warehouse.IsActive || warehouse.Type != type)
                return ServiceResult<ProductResponseDto>.Failure("مخزن الرصيد الافتتاحي غير موجود أو نوعه غير صحيح", ErrorCodes.ValidationError);
        }
        var showrooms = (await _unitOfWork.Warehouses.GetByTypeAsync(WarehouseType.Show, ct)).Where(w => w.IsActive).ToList();
        var storgeWarehouses = (await _unitOfWork.Warehouses.GetByTypeAsync(WarehouseType.Storge, ct)).Where(w => w.IsActive).ToList();
        if (dto.InitialShowroomQuantity > 0 && showrooms.Count == 0 ||
            dto.BarCodes?.Any(b => b.InitialQuantity > 0) == true && storgeWarehouses.Count == 0)
            return ServiceResult<ProductResponseDto>.Failure("يجب توفير مخزن نشط من النوع المناسب للأرصدة الافتتاحية", ErrorCodes.ValidationError);
        var suppliedBarcodes = (dto.BarCodes ?? []).Select(b => b.BarCode.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (dto.StorageInitialQuantities?.Any(q => !suppliedBarcodes.Contains(q.BarCode.Trim())) == true)
            return ServiceResult<ProductResponseDto>.Failure("باركود الرصيد الافتتاحي غير موجود ضمن باركودات الصنف", ErrorCodes.ValidationError);
        if (dto.Units is not null && (dto.Units.Count(u => u.IsDefault) > 1 || dto.Units.Any(u => u.ConversionFactor <= 0) || dto.Units.Select(u => u.UnitId).Distinct().Count() != dto.Units.Count))
            return ServiceResult<ProductResponseDto>.Failure("وحدات الصنف مكررة أو تحتوي معامل تحويل غير صحيح أو أكثر من وحدة افتراضية", ErrorCodes.ValidationError);
        // التحقق من صحة وجود التصنيف التابع له المنتج
        bool categoryExists = await _unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
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
            product.ProductUnits = dto.Units.Select(u => new ProductUnit { // معرف الوحدة
 UnitId = u.UnitId, // معامل التحويل
 ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1, // تحديد ما إذا كانت افتراضية
 IsDefault = u.IsDefault }).ToList();
            if (!product.ProductUnits.Any(u => u.IsDefault))
                product.ProductUnits.First().IsDefault = true;
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
            var existingBarcode = await _unitOfWork.Products.GetFirstMatchingBarcodeAsync(barCodeStrings, ct);
            // في حال وجود الباركود مسبقاً
            if (existingBarcode != null)
            {
                // إرجاع خطأ تكرار الباركود
                return ServiceResult<ProductResponseDto>.Failure($"الباركود '{existingBarcode.BarCode}' مسجل مسبقاً لصنف آخر في النظام", ErrorCodes.BarCodeDuplicate);
            }

            // إنشاء كيانات ProductBarCode للباركودات
            product.ProductBarCodes = dto.BarCodes.Select(b => new ProductBarCode { // رقم الباركود
 BarCode = b.BarCode.Trim(), // عنوان النكهة أو الصنف
 Title = string.IsNullOrWhiteSpace(b.Title) ? dto.Name : b.Title, // وصف إضافي
 Description = b.Description }).ToList();
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // إضافة المنتج الجديد إلى المستودع
            await _unitOfWork.Products.AddAsync(product, ct);
            // حفظ التغييرات في قاعدة البيانات لتوليد المعرفات
            await _unitOfWork.SaveChangesAsync(ct);
            // توليد رصيد صالة العرض في صالات العرض النشطة فقط.

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
                await _unitOfWork.Stock.AddShowroomStockAsync(showStock, ct);
            }

            // توليد رصيد الباركود في مخازن التخزين النشطة فقط.

            // تأسيس أرصدة المخازن للباركودات
            if (product.ProductBarCodes != null && product.ProductBarCodes.Count > 0)
            {
                // خريطة الكميات الافتتاحية المفردة للباركودات
                var initialQtyMap = dto.BarCodes?.ToDictionary(b => b.BarCode.Trim(), b => b.InitialQuantity, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>();
                // خريطة الكميات الافتتاحية للمخازن المتعددة
                var storageMultiQtyMap = dto.StorageInitialQuantities?.ToDictionary(s => $"{s.WarehouseId}_{s.BarCode.Trim()}", s => s.Quantity, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, int>();
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
                        await _unitOfWork.Stock.AddStorgeStockAsync(storgeStock, ct);
                    }
                }
            }

            // حفظ سجلات الأرصدة الافتتاحية في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        // إعادة تحميل المنتج مع كافة تفاصيله لضمان دقة كائن الاستجابة
        var createdProduct = await _unitOfWork.Products.GetDetailsAsync(product.Id, ct) ?? product;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<ProductResponseDto>(createdProduct);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProductResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (dto.Units is not null && (dto.Units.Count(u => u.IsDefault) > 1 || dto.Units.Any(u => u.ConversionFactor <= 0) || dto.Units.Select(u => u.UnitId).Distinct().Count() != dto.Units.Count))
            return ServiceResult<ProductResponseDto>.Failure("وحدات الصنف مكررة أو تحتوي معامل تحويل غير صحيح أو أكثر من وحدة افتراضية", ErrorCodes.ValidationError);
        // تحميل متتبع (Tracked) لضمان حفظ تعديلات الأبناء (باركودات/وحدات) وحذف المحذوف منها فعلياً،
        // وتفادي تضارب تتبع النسخ المكررة عند تكرار نفس الصنف في البنود.
        var product = await _unitOfWork.Products.GetDetailsForUpdateAsync(id, ct);
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
            bool categoryExists = await _unitOfWork.Categories.ExistsByIdAsync(dto.CategoryId, ct);
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
            var existingWithOtherProduct = await _unitOfWork.Products.BarcodesExistOutsideProductAsync(barCodeStrings, id, ct);
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
                    product.ProductUnits.Add(new ProductUnit { ProductId = id, UnitId = u.UnitId, ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1, IsDefault = u.IsDefault });
                }
            }
        }

        // تحديث باركودات ونكهات المنتج
        if (dto.BarCodes != null)
        {
            // تجميع أكواد الباركودات الجديدة
            var dtoBarCodes = dto.BarCodes.Select(b => b.BarCode.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            // 1. تحديد الباركودات المستبعدة من الطلب
            var barcodesToRemove = product.ProductBarCodes.Where(pb => !dtoBarCodes.Contains(pb.BarCode.Trim())).ToList();
            // معالجة وحماية الباركودات المستبعدة
            if (barcodesToRemove.Count > 0)
            {
                // استخراج معرفات الباركودات المحذوفة
                var removedBarcodeIds = barcodesToRemove.Select(b => b.Id).ToList();
                // منع حذف باركود له حركة مخزنية أو فواتير أو صور مرتبطة
                bool usedInSales = await _unitOfWork.SalesInvoices.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInSalesReturns = await _unitOfWork.SalesReturns.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInPurchases = await _unitOfWork.PurchaseInvoices.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturns.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInOrders = await _unitOfWork.PurchaseOrders.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInTransfers = await _unitOfWork.StockTransfers.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool usedInAdjustments = await _unitOfWork.StockAdjustments.HasBarcodeReferencesAsync(removedBarcodeIds, ct);
                bool hasStock = await _unitOfWork.Stock.HasNonzeroStorageByBarcodesAsync(removedBarcodeIds, ct);
                // في حال وجود أي استخدام سابق
                if (usedInSales || usedInSalesReturns || usedInPurchases || usedInPurchaseReturns || usedInOrders || usedInTransfers || usedInAdjustments || hasStock)
                {
                    // إرجاع خطأ يمنع حذف الباركود المرتبط
                    return ServiceResult<ProductResponseDto>.Failure("لا يمكن حذف الباركود '" + string.Join("', '", barcodesToRemove.Select(b => b.BarCode)) + "' لوجود حركة مخزنية أو فواتير مرتبطة به. يرجى الاحتفاظ به أو حذف الأصناف المرتبطة أولاً.", ErrorCodes.BarCodeInUse);
                }

                // حذف منطقي متسلسل: الصور المرتبطة ثم أرصدة المخازن ثم الباركود نفسه
                var linkedImages = await _unitOfWork.Products.GetImagesByBarcodesForUpdateAsync(removedBarcodeIds, ct);
                // حذف الصور المرتبطة بالباركود منطقياً
                foreach (var img in linkedImages)
                {
                    _unitOfWork.Products.SoftDeleteImage(img);
                }

                // استعلام الأرصدة المرتبطة بالباركود في المخازن
                var linkedStocks = await _unitOfWork.Stock.GetStorageByBarcodesForUpdateAsync(removedBarcodeIds, ct);
                // حذف الأرصدة منطقياً
                foreach (var stock in linkedStocks)
                {
                    _unitOfWork.Stock.SoftDeleteStorgeStock(stock);
                }

                // حذف الباركود نفسه منطقياً
                foreach (var b in barcodesToRemove)
                {
                    _unitOfWork.Products.SoftDeleteBarCode(b);
                }
            }

            // 2. تحديث الباركودات القائمة أو إضافة باركودات جديدة
            var newlyAddedBarCodes = new List<ProductBarCode>();
            foreach (var b in dto.BarCodes)
            {
                // تنظيف كود الباركود
                var trimmedCode = b.BarCode.Trim();
                // البحث عن الباركود القائم
                var existingBc = product.ProductBarCodes.FirstOrDefault(pb => string.Equals(pb.BarCode.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase));
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
                var storgeWarehouses = await _unitOfWork.Warehouses.GetByTypeAsync(WarehouseType.Storge, ct);

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
                        await _unitOfWork.Stock.AddStorgeStockAsync(storgeStock, ct);
                    }
                }
            }
        }

        try
        {
            // المنتج محمّل متتبعاً؛ SaveChanges يلتقط كل تعديلات الجذر والأبناء (إضافة/تعديل/حذف منطقي)
            if (dto.Units is not null)
            {
                var defaultUnit = product.ProductUnits.FirstOrDefault(u => u.IsDefault) ?? product.ProductUnits.FirstOrDefault();
                foreach (var unit in product.ProductUnits)
                    unit.IsDefault = false;
                await _unitOfWork.BeginTransactionAsync(ct);
                try
                {
                    await _unitOfWork.SaveChangesAsync(ct);
                    if (defaultUnit is not null)
                        defaultUnit.IsDefault = true;
                    await _unitOfWork.CommitTransactionAsync(ct);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    throw;
                }
            }
            else
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            // معالجة تعارض التعديل المتزامن
            return ServiceResult<ProductResponseDto>.Failure("حدث تعارض أثناء حفظ التعديلات، يرجى إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }

        // إعادة تحميل المنتج مع تفاصيله المحدثة
        var updatedProduct = await _unitOfWork.Products.GetDetailsAsync(id, ct) ?? product;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<ProductResponseDto>(updatedProduct);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المنتج بالمعرف
        var product = await _unitOfWork.Products.GetByIdForUpdateAsync(id, ct);
        // التحقق من وجود المنتج
        if (product is null)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // منع حذف صنف له حركة مبيعات أو مشتريات أو أرصدة قائمة
        bool usedInSalesInvoices = await _unitOfWork.SalesInvoices.HasProductReferencesAsync(id, ct);
        bool usedInSalesReturns = await _unitOfWork.SalesReturns.HasProductReferencesAsync(id, ct);
        bool usedInPurchaseInvoices = await _unitOfWork.PurchaseInvoices.HasProductReferencesAsync(id, ct);
        bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturns.HasProductReferencesAsync(id, ct);
        bool usedInStockTransfers = await _unitOfWork.StockTransfers.HasProductReferencesAsync(id, ct);
        bool usedInStockAdjustments = await _unitOfWork.StockAdjustments.HasProductReferencesAsync(id, ct);
        // جلب معرفات الباركودات المرتبطة بالمنتج
        var barcodeIds = (await _unitOfWork.Products.GetBarcodesByProductAsync(id, ct)).Select(b => b.Id).ToList();
        // فحص وجود الصنف في أوامر الشراء
        bool usedInPurchaseOrders = barcodeIds.Count > 0 && await _unitOfWork.PurchaseOrders.HasBarcodeReferencesAsync(barcodeIds, ct);
        // فحص وجود أرصدة غير صفرية في مخازن التخزين
        bool hasStorgeStock = barcodeIds.Count > 0 && await _unitOfWork.Stock.HasNonzeroStorageByBarcodesAsync(barcodeIds, ct);
        // فحص وجود أرصدة غير صفرية في صالات العرض
        bool hasShowroomStock = await _unitOfWork.Stock.HasNonzeroShowroomByProductAsync(id, ct);
        // في حال وجود أي حركات سابقة أو رصيد غير صفري
        if (usedInSalesInvoices || usedInSalesReturns || usedInPurchaseInvoices || usedInPurchaseReturns || usedInStockTransfers || usedInStockAdjustments || usedInPurchaseOrders || hasStorgeStock || hasShowroomStock)
        {
            // إرجاع رسالة المنع
            return ServiceResult.Failure("لا يمكن حذف الصنف لوجود حركة مبيعات أو مشتريات أو أرصدة مخزنية مرتبطة به. يرجى تصفير أرصدته والتأكد من عدم ارتباطه بفواتير أو حركات مخزنية.", ErrorCodes.ProductInUse);
        }

        // حذف منطقي متسلسل للأبناء (الباركودات وصورها، الوحدات، صور المنتج، والأرصدة الصفرية)
        // جلب صور المنتج وحذفها منطقياً
        var productImages = await _unitOfWork.Products.GetImagesForUpdateAsync(id, ct);
        foreach (var img in productImages)
        {
            _unitOfWork.Products.SoftDeleteImage(img);
        }

        // جلب باركودات المنتج وحذفها منطقياً
        var productBarCodes = await _unitOfWork.Products.GetBarcodesForUpdateAsync(id, ct);
        foreach (var bc in productBarCodes)
        {
            _unitOfWork.Products.SoftDeleteBarCode(bc);
        }

        // جلب وحدات المنتج وحذفها منطقياً
        var productUnits = await _unitOfWork.Products.GetUnitsForUpdateAsync(id, ct);
        foreach (var unit in productUnits)
        {
            _unitOfWork.Products.SoftDeleteUnit(unit);
        }

        var storgeStocks = await _unitOfWork.Stock.GetStorageByBarcodesForUpdateAsync(barcodeIds, ct);
        foreach (var stock in storgeStocks)
        {
            _unitOfWork.Stock.SoftDeleteStorgeStock(stock);
        }

        // جلب أرصدة صالات العرض وحذفها منطقياً
        var showroomStocks = await _unitOfWork.Stock.GetShowroomByProductForUpdateAsync(id, ct);
        foreach (var stock in showroomStocks)
        {
            _unitOfWork.Stock.SoftDeleteShowroomStock(stock);
        }

        // حذف المنتج نفسه منطقياً
        _unitOfWork.Products.SoftDelete(product);
        // حفظ كافة عمليات الحذف المنطقي في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
