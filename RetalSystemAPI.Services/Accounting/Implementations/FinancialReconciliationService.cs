using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models.DTOs.Accounting.Reconciliation;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;

/// <summary>
/// تنفيذ خدمة التدقيق والمطابقة المالية لاكتشاف الفجوات المحاسبية بين المستندات التجارية ودفاتر الأستاذ العام (قراءة فقط).
/// لا تقوم الخدمة بتعديل الأرصدة تلقائياً للحفاظ على الدقة والرقابة المحاسبية وتتطلب اعتمادات يدوية.
/// </summary>
public sealed class FinancialReconciliationService : IFinancialReconciliationService
{
    // سياق قاعدة البيانات للقراءة والاستعلام
    private readonly AppDbContext _dbContext;

    // الحد الأقصى للسجلات المقروءة لكل مصدر حماية لذاكرة الخادم
    private readonly int _maxRecordsPerSource;

    /// <summary>
    /// تهيئة خدمة المطابقة المالية مع حقن سياق البيانات وإعدادات التكوين لتحديد سقوف السجلات.
    /// </summary>
    /// <param name="dbContext">سياق قاعدة البيانات</param>
    /// <param name="configuration">إعدادات النظام لقراءة الحد الأقصى للسجلات</param>
    public FinancialReconciliationService(AppDbContext dbContext, IConfiguration? configuration = null)
    {
        // تعيين سياق قاعدة البيانات
        _dbContext = dbContext;
        // قراءة وتحديد الحد الأقصى للسجلات المحملة وحصره بين 1,000 و 100,000 سجل
        _maxRecordsPerSource = Math.Clamp(
            configuration?.GetValue<int?>("FinancialReconciliation:MaxRecordsPerSource") ?? 25_000,
            1_000,
            100_000);
    }

    /// <inheritdoc />
    /// <summary>
    /// تنفيذ عملية الفحص والتدقيق المالي الشامل للفروع والمستندات ودفاتر الحسابات النقدية والعملاء والموردين.
    /// </summary>
    public async Task<ServiceResult<FinancialReconciliationReportDto>> AuditAsync(
        Guid? branchId = null,
        decimal tolerance = 0.01m,
        int maxIssues = 500,
        CancellationToken ct = default)
    {
        // ضبط نسبة السماح في الفروقات بين 0.01 و 100
        tolerance = Math.Clamp(tolerance, 0.01m, 100m);
        // ضبط الحد الأقصى للمشكلات المستخرجة بين 1 و 5,000 مشكلة
        maxIssues = Math.Clamp(maxIssues, 1, 5_000);

        // قائمة لتجميع المشكلات والفجوات المحاسبية المكتشفة
        var issues = new List<FinancialReconciliationIssueDto>();
        // مؤشر لمعرفة ما إذا تم بلوغ سقف السجلات الأقصى أثناء الفحص
        var scanLimitReached = false;

        // 1. فحص الفروع والخزائن النشطة التابعة لها
        var branches = await _dbContext.Branches.AsNoTracking()
            // تصفية حسب الفرع إن كان محدداً
            .Where(x => !branchId.HasValue || x.Id == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار المعرف والاسم فقط
            .Select(x => new { x.Id, x.Name })
            // أخذ سقف السجلات الأقصى + 1 لمعرفة التجاوز
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // تقليص السجلات إلى السقف المسموح وتحديث حالة التجاوز
        scanLimitReached |= TrimToScanLimit(branches);

        // جلب الخزائن التابعة للفروع المفحوصة
        var safes = await _dbContext.Safes.AsNoTracking()
            // تصفية بالفرع
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار الحقول الأساسية
            .Select(x => new { x.Id, x.BranchId, x.Name, x.OpeningBalance, x.CurrentBalance, x.IsActive })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // تقليص السجلات والتأكد من عدم تجاوز السقف
        scanLimitReached |= TrimToScanLimit(safes);

        // التكرار على كل فرع للتحقق من وجود خزينة نشطة وحيدة
        foreach (var branch in branches)
        {
            // حساب عدد الخزائن النشطة في الفرع
            var activeSafeCount = safes.Count(x => x.BranchId == branch.Id && x.IsActive);
            // إذا كانت هناك خزينة واحدة نشطة فالوضع سليم
            if (activeSafeCount == 1)
                // الانتقال للفرع التالي
                continue;

            // تسجيل مشكلة عدم وجود خزينة نشطة أو وجود أكثر من خزينة نشطة بدون تعيين افتراضي
            AddIssue(issues, new FinancialReconciliationIssueDto
            {
                // كود المشكلة
                Code = activeSafeCount == 0 ? "BRANCH_WITHOUT_ACTIVE_SAFE" : "BRANCH_WITH_AMBIGUOUS_SAFES",
                // درجة الخطورة
                Severity = activeSafeCount == 0 ? "Critical" : "Warning",
                // نوع الكيان
                EntityType = "Branch",
                // معرف الكيان
                EntityId = branch.Id,
                // معرف الفرع
                BranchId = branch.Id,
                // الرقم المرجعي
                ReferenceNumber = branch.Name,
                // القيمة الفعلية
                ActualAmount = activeSafeCount,
                // القيمة المتوقعة
                ExpectedAmount = 1,
                // الفرق
                Difference = activeSafeCount - 1,
                // نص رسالة المشكلة
                Message = activeSafeCount == 0
                    ? $"الفرع [{branch.Name}] لا يملك خزينة نشطة."
                    : $"الفرع [{branch.Name}] يملك {activeSafeCount} خزائن نشطة؛ يجب اختيار الخزينة صراحة عند كل حركة.",
                // الإجراء المقترح لحل المشكلة
                SuggestedAction = activeSafeCount == 0
                    ? "أنشئ خزينة تشغيلية نشطة للفرع قبل استقبال أي حركة مالية."
                    : "تحقق من أن جميع شاشات وتكاملات API ترسل معرف الخزينة صراحة."
            });
        }

        // 2. تحميل دفاتر حركة العملاء لفواتير ومرتجعات المبيعات
        var customerLedger = new List<DocumentLedgerAmount>();
        // التكرار على أنواع مستندات العملاء
        foreach (var documentType in new[] { FinancialDocumentType.SalesInvoice, FinancialDocumentType.SalesReturn })
        {
            // تحميل حركات دفتر الأستاذ للنوع المحدد
            var batch = await LoadCustomerLedgerAsync(documentType, branchId, ct);
            // تحديث حالة بلوغ السقف
            scanLimitReached |= batch.IsTruncated;
            // إضافة السجلات للقائمة
            customerLedger.AddRange(batch.Rows);
        }
        // تحويل الحركات إلى قاموس مفهرس بنوع ومعرف المستند
        var customerLedgerByDocument = customerLedger.ToDictionary(
            x => (x.DocumentType, x.DocumentId), x => x.Amount);

        // 3. تحميل دفاتر حركة الموردين لفواتير ومرتجعات المشتريات
        var supplierLedger = new List<DocumentLedgerAmount>();
        // التكرار على أنواع مستندات الموردين
        foreach (var documentType in new[] { FinancialDocumentType.PurchaseInvoice, FinancialDocumentType.PurchaseReturn })
        {
            // تحميل حركات دفتر الأستاذ للموردين
            var batch = await LoadSupplierLedgerAsync(documentType, branchId, ct);
            // تحديث حالة بلوغ السقف
            scanLimitReached |= batch.IsTruncated;
            // إضافة السجلات
            supplierLedger.AddRange(batch.Rows);
        }
        // تحويل الحركات إلى قاموس مفهرس بنوع ومعرف المستند
        var supplierLedgerByDocument = supplierLedger.ToDictionary(
            x => (x.DocumentType, x.DocumentId), x => x.Amount);

        // 4. تحميل دفاتر حركات الخزائن لكافة المستندات النقدية
        var safeLedger = new List<DocumentLedgerAmount>();
        // التكرار على المستندات النقدية المؤثرة في الخزائن
        foreach (var documentType in new[]
                 {
                     FinancialDocumentType.SalesInvoice,
                     FinancialDocumentType.PurchaseInvoice,
                     FinancialDocumentType.SalesReturn,
                     FinancialDocumentType.PurchaseReturn
                 })
        {
            // تحميل حركات الخزينة لكل نوع مستند
            var batch = await LoadSafeLedgerAsync(documentType, branchId, ct);
            // تحديث حالة بلوغ السقف
            scanLimitReached |= batch.IsTruncated;
            // إضافة السجلات
            safeLedger.AddRange(batch.Rows);
        }
        // تحويل حركات الخزينة لقاموس مفهرس
        var safeLedgerByDocument = safeLedger.ToDictionary(
            x => (x.DocumentType, x.DocumentId), x => x.Amount);

        // 5. فحص ومطابقة فواتير المبيعات
        var salesInvoices = await _dbContext.SalesInvoices.AsNoTracking()
            // تصفية بالفرع
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار الحقول المالية
            .Select(x => new
            {
                x.Id, x.InvoiceNumber, x.BranchId, x.CustomerId, x.Status,
                x.TotalAmount, x.PaidAmount, x.RemainingAmount, x.PaymentSafeId
            })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // جلب البيانات
            .ToListAsync(ct);
        // فحص السقف وتعديل القائمة
        scanLimitReached |= TrimToScanLimit(salesInvoices);

        // فحص كل فاتورة مبيعات ومقارنة حركاتها
        foreach (var invoice in salesInvoices)
        {
            // هل الفاتورة ملغاة
            var cancelled = invoice.Status is InvoiceStatus.Cancelled or InvoiceStatus.Voided;
            // التحقق من وجود خزينة دفع إذا كان هناك مدفوع نقدي للفواتير السارية
            if (!cancelled && invoice.PaidAmount > tolerance && !invoice.PaymentSafeId.HasValue)
                // إضافة مشكلة عدم وجود خزينة للفاتورة المسددة
                AddMissingSafeIssue(issues, "SalesInvoice", invoice.Id, invoice.BranchId, invoice.InvoiceNumber, invoice.PaidAmount);

            // التحقق من مطابقة قيد العميل مع المتبقي الآجل للفاتورة
            if (invoice.CustomerId.HasValue)
            {
                // القيمة المتوقعة في حساب العميل
                var expected = cancelled ? 0m : invoice.RemainingAmount;
                // القيمة الفعلية المسجلة في دفتر حركة العميل
                var actual = GetAmount(customerLedgerByDocument, FinancialDocumentType.SalesInvoice, invoice.Id);
                // إضافة فجوة في حال عدم التطابق
                AddMismatchIfNeeded(issues, tolerance, "CUSTOMER_DOCUMENT_LEDGER_MISMATCH", "SalesInvoice",
                    invoice.Id, invoice.BranchId, invoice.InvoiceNumber, expected, actual,
                    "صافي قيد العميل لا يطابق الرصيد المتبقي لفاتورة المبيعات.");
            }

            // التحقق من مطابقة حركة الخزينة مع المبلغ المدفوع نقداً
            var expectedSafe = cancelled ? 0m : invoice.PaidAmount;
            // المبلغ الفعلي المسجل في حركات الخزينة
            var actualSafe = GetAmount(safeLedgerByDocument, FinancialDocumentType.SalesInvoice, invoice.Id);
            // إضافة فجوة في حال عدم التطابق
            AddMismatchIfNeeded(issues, tolerance, "SAFE_DOCUMENT_LEDGER_MISMATCH", "SalesInvoice",
                invoice.Id, invoice.BranchId, invoice.InvoiceNumber, expectedSafe, actualSafe,
                "صافي حركة الخزينة لا يطابق المبلغ المدفوع لفاتورة المبيعات.");
        }

        // 6. فحص ومطابقة فواتير المشتريات
        var purchaseInvoices = await _dbContext.PurchaseInvoices.AsNoTracking()
            // تصفية بالفرع
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار الحقول المالية
            .Select(x => new
            {
                x.Id, x.InvoiceNumber, x.BranchId, x.SupplierId, x.Status,
                x.TotalAmount, x.PaidAmount, x.RemainingAmount, x.PaymentSafeId
            })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // فحص السقف وتعديل القائمة
        scanLimitReached |= TrimToScanLimit(purchaseInvoices);

        // فحص كل فاتورة مشتريات ومقارنة قيودها
        foreach (var invoice in purchaseInvoices)
        {
            // التحقق من حالة الإلغاء
            var cancelled = invoice.Status is InvoiceStatus.Cancelled or InvoiceStatus.Voided;
            // التحقق من تعيين خزينة الدفع عند وجود دفع نقدي
            if (!cancelled && invoice.PaidAmount > tolerance && !invoice.PaymentSafeId.HasValue)
                // تسجيل مشكلة عدم تعيين الخزينة
                AddMissingSafeIssue(issues, "PurchaseInvoice", invoice.Id, invoice.BranchId, invoice.InvoiceNumber, invoice.PaidAmount);

            // مطابقة قيد المورد مع المتبقي الآجل للفاتورة
            var expectedSupplier = cancelled ? 0m : invoice.RemainingAmount;
            // القيمة المسجلة في دفتر حركة المورد
            var actualSupplier = GetAmount(supplierLedgerByDocument, FinancialDocumentType.PurchaseInvoice, invoice.Id);
            // تسجيل عدم التطابق إن وجد
            AddMismatchIfNeeded(issues, tolerance, "SUPPLIER_DOCUMENT_LEDGER_MISMATCH", "PurchaseInvoice",
                invoice.Id, invoice.BranchId, invoice.InvoiceNumber, expectedSupplier, actualSupplier,
                "صافي قيد المورد لا يطابق الرصيد المتبقي لفاتورة المشتريات.");

            // مطابقة حركة الصرف من الخزينة مع المبلغ المدفوع للمورد
            var expectedSafe = cancelled ? 0m : -invoice.PaidAmount;
            // الحركة الفعلية في الخزينة
            var actualSafe = GetAmount(safeLedgerByDocument, FinancialDocumentType.PurchaseInvoice, invoice.Id);
            // تسجيل عدم التطابق إن وجد
            AddMismatchIfNeeded(issues, tolerance, "SAFE_DOCUMENT_LEDGER_MISMATCH", "PurchaseInvoice",
                invoice.Id, invoice.BranchId, invoice.InvoiceNumber, expectedSafe, actualSafe,
                "صافي حركة الخزينة لا يطابق المبلغ المدفوع لفاتورة المشتريات.");
        }

        // 7. فحص ومطابقة مرتجعات المبيعات
        var salesReturns = await _dbContext.SalesReturns.AsNoTracking()
            // تصفية بالفرع
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار الحقول
            .Select(x => new { x.Id, x.ReturnNumber, x.BranchId, x.CustomerId, x.TotalAmount, x.RefundedAmount, x.RefundSafeId })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // جلب القائمة
            .ToListAsync(ct);
        // تقليص السجلات للسقف
        scanLimitReached |= TrimToScanLimit(salesReturns);

        // فحص كل مرتجع مبيعات
        foreach (var salesReturn in salesReturns)
        {
            // التحقق من تعيين خزينة الاسترداد في حال وجود رد نقدي
            if (salesReturn.RefundedAmount > tolerance && !salesReturn.RefundSafeId.HasValue)
                // تسجيل مشكلة عدم تعيين خزينة الاسترداد
                AddMissingSafeIssue(issues, "SalesReturn", salesReturn.Id, salesReturn.BranchId,
                    salesReturn.ReturnNumber, salesReturn.RefundedAmount);

            // مطابقة قيد العميل الآجل
            if (salesReturn.CustomerId.HasValue)
            {
                // المبلغ المتوقع خصمه من رصيد العميل
                var expectedCustomer = -(salesReturn.TotalAmount - salesReturn.RefundedAmount);
                // القيمة الفعلية في دفتر العميل
                var actualCustomer = GetAmount(customerLedgerByDocument, FinancialDocumentType.SalesReturn, salesReturn.Id);
                // فحص عدم التطابق
                AddMismatchIfNeeded(issues, tolerance, "CUSTOMER_DOCUMENT_LEDGER_MISMATCH", "SalesReturn",
                    salesReturn.Id, salesReturn.BranchId, salesReturn.ReturnNumber, expectedCustomer, actualCustomer,
                    "صافي قيد العميل لا يطابق الجزء المضاف لحسابه من مرتجع المبيعات.");
            }

            // مطابقة حركة الصرف النقدي للمرتجع من الخزينة
            var actualSafe = GetAmount(safeLedgerByDocument, FinancialDocumentType.SalesReturn, salesReturn.Id);
            // فحص عدم التطابق في الخزينة
            AddMismatchIfNeeded(issues, tolerance, "SAFE_DOCUMENT_LEDGER_MISMATCH", "SalesReturn",
                salesReturn.Id, salesReturn.BranchId, salesReturn.ReturnNumber, -salesReturn.RefundedAmount, actualSafe,
                "صافي حركة الخزينة لا يطابق المبلغ المعاد للعميل.");
        }

        // 8. فحص ومطابقة مرتجعات المشتريات
        var purchaseReturns = await _dbContext.PurchaseReturns.AsNoTracking()
            // تصفية بالفرع
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب
            .OrderBy(x => x.Id)
            // اختيار الحقول
            .Select(x => new { x.Id, x.ReturnNumber, x.BranchId, x.SupplierId, x.TotalAmount, x.PaymentMethod, x.RefundSafeId })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // جلب البيانات
            .ToListAsync(ct);
        // ضبط السقف
        scanLimitReached |= TrimToScanLimit(purchaseReturns);

        // فحص كل مرتجع مشتريات
        foreach (var purchaseReturn in purchaseReturns)
        {
            // هل طريقة التسوية آجل
            var isCredit = purchaseReturn.PaymentMethod == PaymentMethod.Credit;
            // إذا لم يكن آجلاً وكان هناك استرداد نقدي دون تحديد خزينة
            if (!isCredit && purchaseReturn.TotalAmount > tolerance && !purchaseReturn.RefundSafeId.HasValue)
                // تسجيل مشكلة عدم تعيين الخزينة
                AddMissingSafeIssue(issues, "PurchaseReturn", purchaseReturn.Id, purchaseReturn.BranchId,
                    purchaseReturn.ReturnNumber, purchaseReturn.TotalAmount);

            // مطابقة قيد المورد الآجل
            var expectedSupplier = isCredit ? -purchaseReturn.TotalAmount : 0m;
            // القيمة الفعلية في دفتر المورد
            var actualSupplier = GetAmount(supplierLedgerByDocument, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id);
            // فحص عدم التطابق
            AddMismatchIfNeeded(issues, tolerance, "SUPPLIER_DOCUMENT_LEDGER_MISMATCH", "PurchaseReturn",
                purchaseReturn.Id, purchaseReturn.BranchId, purchaseReturn.ReturnNumber, expectedSupplier, actualSupplier,
                "صافي قيد المورد لا يطابق طريقة تسوية مرتجع المشتريات.");

            // مطابقة حركة القبض النقدي في الخزينة
            var expectedSafe = isCredit ? 0m : purchaseReturn.TotalAmount;
            // الحركة الفعلية في الخزينة
            var actualSafe = GetAmount(safeLedgerByDocument, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id);
            // فحص عدم تطابق الخزينة
            AddMismatchIfNeeded(issues, tolerance, "SAFE_DOCUMENT_LEDGER_MISMATCH", "PurchaseReturn",
                purchaseReturn.Id, purchaseReturn.BranchId, purchaseReturn.ReturnNumber, expectedSafe, actualSafe,
                "صافي حركة الخزينة لا يطابق قيمة مرتجع المشتريات النقدي.");
        }

        // 9. مطابقة الأرصدة التراكمية المخزنة للخزائن والعملاء والموردين
        scanLimitReached |= await AuditStoredBalancesAsync(issues, branchId, tolerance, ct);

        // ترتيب المشكلات حسب الأولوية: الحرجة أولاً، ثم الكود، ثم الرقم المرجعي
        var ordered = issues
            // الأولوية للحرجة
            .OrderBy(x => x.Severity == "Critical" ? 0 : 1)
            // ثم كود المشكلة
            .ThenBy(x => x.Code)
            // ثم الرقم المرجعي
            .ThenBy(x => x.ReferenceNumber)
            // تحويل لقائمة مرتبة
            .ToList();

        // تجميع وبناء كائن التقرير النهائي
        var report = new FinancialReconciliationReportDto
        {
            // وقت إنشاء التقرير
            GeneratedAtUtc = DateTime.UtcNow,
            // نسبة السماح المستخدمة
            Tolerance = tolerance,
            // إجمالي المشكلات المكتشفة
            TotalIssues = ordered.Count,
            // عدد المشكلات الحرجة
            CriticalIssues = ordered.Count(x => x.Severity == "Critical"),
            // عدد التحذيرات
            WarningIssues = ordered.Count(x => x.Severity == "Warning"),
            // هل تم اقتطاع النتائج بسبب بلوغ السقف
            IsTruncated = scanLimitReached || ordered.Count > maxIssues,
            // قائمة المشكلات بحسب الحد الأقصى المطلوب
            Issues = ordered.Take(maxIssues).ToList()
        };

        // إرجاع التقرير بنجاح
        return ServiceResult<FinancialReconciliationReportDto>.Success(report);
    }

    /// <summary>
    /// تحميل مجمّع حركات دفتر أستاذ العملاء لمستند محدد وتجميع الفروقات (مدين - دائن).
    /// </summary>
    // تجميع مبالغ دفتر العملاء لكل مستند
    private async Task<LedgerBatch> LoadCustomerLedgerAsync(
        FinancialDocumentType documentType,
        Guid? branchId,
        CancellationToken ct)
    {
        // استعلام حركات العملاء بدون تتبع وتجميعها بالمستند
        var rows = await _dbContext.CustomerTransactions.AsNoTracking()
            // تصفية بنوع المستند ووجود معرف المستند
            .Where(x => x.DocumentType == documentType && x.DocumentId.HasValue)
            // تصفية بالفرع إن حدد
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // التجميع حسب معرف المستند
            .GroupBy(x => x.DocumentId!.Value)
            // حساب صافي المبلغ (مدين - دائن)
            .Select(group => new DocumentLedgerAmount
            {
                DocumentType = documentType,
                DocumentId = group.Key,
                Amount = group.Sum(x => x.DebitAmount - x.CreditAmount)
            })
            // الترتيب بالمعرف
            .OrderBy(x => x.DocumentId)
            // أخذ سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);

        // إرجاع دفعة السجلات مع مؤشر الاقتطاع
        return new LedgerBatch(rows, TrimToScanLimit(rows));
    }

    /// <summary>
    /// تحميل مجمّع حركات دفتر أستاذ الموردين لمستند محدد وتجميع الفروقات (دائن - مدين).
    /// </summary>
    // تجميع مبالغ دفتر الموردين لكل مستند
    private async Task<LedgerBatch> LoadSupplierLedgerAsync(
        FinancialDocumentType documentType,
        Guid? branchId,
        CancellationToken ct)
    {
        // استعلام حركات الموردين بدون تتبع وتجميعها بالمستند
        var rows = await _dbContext.SupplierTransactions.AsNoTracking()
            // تصفية بنوع المستند ومعرفه
            .Where(x => x.DocumentType == documentType && x.DocumentId.HasValue)
            // تصفية بالفرع إن حدد
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // التجميع بمعرف المستند
            .GroupBy(x => x.DocumentId!.Value)
            // حساب صافي المبلغ (دائن - مدين)
            .Select(group => new DocumentLedgerAmount
            {
                DocumentType = documentType,
                DocumentId = group.Key,
                Amount = group.Sum(x => x.CreditAmount - x.DebitAmount)
            })
            // الترتيب بالمعرف
            .OrderBy(x => x.DocumentId)
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);

        // إرجاع دفعة السجلات مع مؤشر الاقتطاع
        return new LedgerBatch(rows, TrimToScanLimit(rows));
    }

    /// <summary>
    /// تحميل مجمّع حركات الخزائن لمستند محدد وتجميع صافي النقدية (وارد - صادر).
    /// </summary>
    // تجميع مبالغ حركات الخزائن لكل مستند
    private async Task<LedgerBatch> LoadSafeLedgerAsync(
        FinancialDocumentType documentType,
        Guid? branchId,
        CancellationToken ct)
    {
        // استعلام حركات الخزائن بدون تتبع وتجميعها بالمستند
        var rows = await _dbContext.SafeTransactions.AsNoTracking()
            // تصفية بنوع المستند ووجود معرفه
            .Where(x => x.DocumentType == documentType && x.DocumentId.HasValue)
            // تصفية بالفرع التابع للخزينة
            .Where(x => !branchId.HasValue || x.Safe.BranchId == branchId.Value)
            // التجميع بمعرف المستند
            .GroupBy(x => x.DocumentId!.Value)
            // حساب صافي الحركة (وارد - صادر)
            .Select(group => new DocumentLedgerAmount
            {
                DocumentType = documentType,
                DocumentId = group.Key,
                Amount = group.Sum(x => x.Direction == TransactionDirection.Inbound ? x.Amount : -x.Amount)
            })
            // الترتيب بالمعرف
            .OrderBy(x => x.DocumentId)
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);

        // إرجاع دفعة السجلات مع مؤشر الاقتطاع
        return new LedgerBatch(rows, TrimToScanLimit(rows));
    }

    /// <summary>
    /// تدقيق ومطابقة الأرصدة التراكمية الحالية المخزنة في جداول الكيانات مقارنة بمجموع القيود في دفاتر الحركة.
    /// </summary>
    // تدقيق صحة الأرصدة التراكمية المخزنة ومطابقتها مع دفاتر الأستاذ
    private async Task<bool> AuditStoredBalancesAsync(
        List<FinancialReconciliationIssueDto> issues,
        Guid? branchId,
        decimal tolerance,
        CancellationToken ct)
    {
        // مؤشر وصول السقف
        var scanLimitReached = false;

        // تدقيق أرصدة الخزائن مقارنة بجميع حركاتها ورصيد الافتتاح
        var safeBalances = await _dbContext.Safes.AsNoTracking()
            // تصفية بالفرع إن حدد
            .Where(x => !branchId.HasValue || x.BranchId == branchId.Value)
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // اختيار الحقول وحساب رصيد الحركات التراكمي
            .Select(x => new
            {
                x.Id, x.BranchId, x.Name, x.OpeningBalance, x.CurrentBalance,
                // هل يوجد قيد رصيد افتتاحي مسجل كحركة
                HasOpeningEntry = x.Transactions.Any(t => t.DocumentType == FinancialDocumentType.OpeningBalance),
                // مجموع الحركات النقدية (وارد - صادر)
                LedgerBalance = x.Transactions.Sum(t => (decimal?)(t.Direction == TransactionDirection.Inbound ? t.Amount : -t.Amount)) ?? 0m
            })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // فحص السقف وتعديل القائمة
        scanLimitReached |= TrimToScanLimit(safeBalances);

        // فحص رصيد كل خزينة
        foreach (var safe in safeBalances)
        {
            // حساب الرصيد المتوقع (رصيد الحركات + الرصيد الافتتاحي إن لم يكن مسجلاً كحركة)
            var expected = safe.LedgerBalance + (safe.HasOpeningEntry ? 0m : safe.OpeningBalance);
            // إضافة مشكلة في حال عدم تطابق الرصيد المخزن مع المتوقع
            AddMismatchIfNeeded(issues, tolerance, "SAFE_BALANCE_MISMATCH", "Safe", safe.Id, safe.BranchId,
                safe.Name, expected, safe.CurrentBalance, "الرصيد المخزن للخزينة لا يساوي صافي سجل حركاتها.");
        }

        // أرصدة العملاء إجمالية على مستوى المستأجر
        var customerBalances = await _dbContext.Customers.AsNoTracking()
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // حساب الرصيد ومجموع حركات العميل
            .Select(x => new
            {
                x.Id, x.Name, x.OpeningBalance, x.CurrentBalance,
                // صافي الحركات (مدين - دائن)
                LedgerDelta = x.CustomerTransactions.Sum(t => (decimal?)(t.DebitAmount - t.CreditAmount)) ?? 0m
            })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // فحص السقف
        scanLimitReached |= TrimToScanLimit(customerBalances);
        // فحص رصيد كل عميل
        foreach (var customer in customerBalances)
        {
            // الرصيد المتوقع (الافتتاحي + صافي الحركات)
            var expected = customer.OpeningBalance + customer.LedgerDelta;
            // التحقق من المطابقة وتسجيل المشكلة إن وجدت
            AddMismatchIfNeeded(issues, tolerance, "CUSTOMER_BALANCE_MISMATCH", "Customer", customer.Id, null,
                customer.Name, expected, customer.CurrentBalance, "الرصيد المخزن للعميل لا يساوي رصيده الافتتاحي وصافي حركاته.");
        }

        // أرصدة الموردين إجمالية على مستوى المستأجر
        var supplierBalances = await _dbContext.Suppliers.AsNoTracking()
            // الترتيب بالمعرف
            .OrderBy(x => x.Id)
            // حساب الرصيد ومجموع حركات المورد
            .Select(x => new
            {
                x.Id, x.Name, x.OpeningBalance, x.CurrentBalance,
                // صافي الحركات (دائن - مدين)
                LedgerDelta = x.SupplierTransactions.Sum(t => (decimal?)(t.CreditAmount - t.DebitAmount)) ?? 0m
            })
            // سقف السجلات
            .Take(_maxRecordsPerSource + 1)
            // تنفيذ الاستعلام
            .ToListAsync(ct);
        // فحص السقف
        scanLimitReached |= TrimToScanLimit(supplierBalances);
        // فحص رصيد كل مورد
        foreach (var supplier in supplierBalances)
        {
            // الرصيد المتوقع (الافتتاحي + صافي الحركات)
            var expected = supplier.OpeningBalance + supplier.LedgerDelta;
            // التحقق من المطابقة وتسجيل المشكلة إن وجدت
            AddMismatchIfNeeded(issues, tolerance, "SUPPLIER_BALANCE_MISMATCH", "Supplier", supplier.Id, null,
                supplier.Name, expected, supplier.CurrentBalance, "الرصيد المخزن للمورد لا يساوي رصيده الافتتاحي وصافي حركاته.");
        }

        // إرجاع حالة وصول السقف
        return scanLimitReached;
    }

    /// <summary>
    /// تقليص قائمة السجلات إذا تجاوزت الحد الأقصى وإرجاع مؤشر التجاوز.
    /// </summary>
    // التحقق من بلوغ السقف وتقليص القائمة
    private bool TrimToScanLimit<T>(List<T> records)
    {
        // إذا كان عدد السجلات ضمن السقف
        if (records.Count <= _maxRecordsPerSource) return false;

        // حذف السجلات الزائدة عن السقف
        records.RemoveRange(_maxRecordsPerSource, records.Count - _maxRecordsPerSource);
        // تم بلوغ السقف واقتطاع النتائج
        return true;
    }

    /// <summary>
    /// كلاس داخلي لتخزين رصيد حركة الدفتر المرتبط بمستند معين.
    /// </summary>
    // كائن داخلي لربط مبالغ الدفتر بالمستند
    private sealed class DocumentLedgerAmount
    {
        /// <summary>نوع المستند المالي</summary>
        // نوع المستند
        public FinancialDocumentType DocumentType { get; init; }

        /// <summary>معرف المستند</summary>
        // معرف المستند
        public Guid DocumentId { get; init; }

        /// <summary>المبلغ الإجمالي لحركة الدفتر</summary>
        // مبلغ الحركة
        public decimal Amount { get; init; }
    }

    /// <summary>
    /// سجل داخلي لحفظ دفعة السجلات المحملة ومؤشر اقتطاعها.
    /// </summary>
    // كائن مجمّع لدفعة سجلات الدفتر
    private sealed record LedgerBatch(
        List<DocumentLedgerAmount> Rows,
        bool IsTruncated);

    /// <summary>
    /// دالة مساعدة لاستخراج المبلغ من قاموس الحركات المفهرس بالمستند.
    /// </summary>
    // استخراج المبلغ المسجل للمستند أو إرجاع صفر افتراضياً
    private static decimal GetAmount(
        IReadOnlyDictionary<(FinancialDocumentType DocumentType, Guid DocumentId), decimal> values,
        FinancialDocumentType documentType,
        Guid documentId) => values.GetValueOrDefault((documentType, documentId));

    /// <summary>
    /// إضافة مشكلة عدم وجود خزينة مرتبطة بمستند تم دفع مبلغ نقدي فيه.
    /// </summary>
    // إنشاء وتسجيل مشكلة مستند نقدي غير مربوط بخزينة
    private static void AddMissingSafeIssue(
        List<FinancialReconciliationIssueDto> issues,
        string entityType,
        Guid entityId,
        Guid branchId,
        string referenceNumber,
        decimal amount)
    {
        // إضافة المشكلة إلى القائمة
        AddIssue(issues, new FinancialReconciliationIssueDto
        {
            Code = "PAID_DOCUMENT_WITHOUT_SAFE",
            Severity = "Critical",
            EntityType = entityType,
            EntityId = entityId,
            BranchId = branchId,
            ReferenceNumber = referenceNumber,
            ExpectedAmount = amount,
            ActualAmount = 0,
            Difference = -amount,
            Message = $"المستند [{referenceNumber}] يتضمن مبلغاً نقدياً {amount:N2} دون خزينة مرتبطة.",
            SuggestedAction = "حدد الخزينة التاريخية الصحيحة ثم أنشئ قيد تسوية معتمد؛ لا تخمّن الخزينة تلقائياً."
        });
    }

    /// <summary>
    /// فحص الفروقات بين القيمة المتوقعة والفعلية وإضافة مشكلة عدم تطابق إذا تجاوزت نسبة السماح.
    /// </summary>
    // فحص الفروقات وإضافة مشكلة عدم اتساق إن وجدت
    private static void AddMismatchIfNeeded(
        List<FinancialReconciliationIssueDto> issues,
        decimal tolerance,
        string code,
        string entityType,
        Guid entityId,
        Guid? branchId,
        string referenceNumber,
        decimal expected,
        decimal actual,
        string message)
    {
        // حساب قيمة الفرق بين الفعلي والمتوقع
        var difference = actual - expected;
        // إذا كان الفرق ضمن نسبة السماح المقبولة
        if (Math.Abs(difference) <= tolerance)
            // لا حاجة لتسجيل مشكلة
            return;

        // إضافة تفاصيل الفجوة المحاسبية المكتشفة
        AddIssue(issues, new FinancialReconciliationIssueDto
        {
            Code = code,
            Severity = "Critical",
            EntityType = entityType,
            EntityId = entityId,
            BranchId = branchId,
            ReferenceNumber = referenceNumber,
            ExpectedAmount = expected,
            ActualAmount = actual,
            Difference = difference,
            Message = message,
            SuggestedAction = "راجع المستند والقيود المرتبطة به وأنشئ قيد تسوية موثق بعد اعتماد المحاسب."
        });
    }

    /// <summary>
    /// إضافة كائن المشكلة إلى قائمة المشكلات المستخرجة.
    /// </summary>
    // إضافة المشكلة إلى القائمة المجمعة
    private static void AddIssue(List<FinancialReconciliationIssueDto> issues, FinancialReconciliationIssueDto issue)
        => issues.Add(issue);
}
