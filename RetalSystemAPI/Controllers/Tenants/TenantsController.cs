using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Tenant.Interfaces;

namespace RetalSystemAPI.Controllers.Tenants;

/// <summary>
/// متحكم إدارة المستأجرين (Tenants) على مستوى نظام السحاب متعدد المستأجرين (Multi-Tenancy).
/// يوفر نقاط النهاية المخصصة للمستأجر الحالي لإدارة بروفايله وشارته، بالإضافة إلى عمليات الإدارة الشاملة (CRUD) لمدراء النظام.
/// </summary>
[Authorize]
[Route("api/tenants")]
public class TenantsController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة المستأجرين في النظام.
    /// </summary>
    private readonly ITenantService _tenantService;

    /// <summary>
    /// خدمة رفع ومعالجة الملفات والصور على الخادم.
    /// </summary>
    private readonly RetalSystemAPI.Services.FileUpload.Interfaces.IFileUploadService _fileUploadService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المستأجرين مع حقن الخدمات المطلوبة.
    /// </summary>
    /// <param name="tenantService">خدمة إدارة المستأجرين.</param>
    /// <param name="fileUploadService">خدمة رفع ومعالجة الملفات.</param>
    public TenantsController(
        ITenantService tenantService,
        RetalSystemAPI.Services.FileUpload.Interfaces.IFileUploadService fileUploadService)
    {
        // إسناد خدمة المستأجرين المحقونة
        _tenantService = tenantService;
        // إسناد خدمة رفع الملفات المحقونة
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// استرجاع بيانات الملف التعريفي والبروفايل للمستأجر الحالي استناداً لمعرف المستأجر في رمز التوثيق (JWT Token).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لدعم إيقاف الطلب غير المتزامن.</param>
    /// <returns>بيانات المستأجر الحالي مغلفة في استجابة الـ API الموحدة.</returns>
    [HttpGet("me")]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        // استخراج مطالبة معرف المستأجر من مطالبات المستخدم الموثق الحالي
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        // التحقق من صلاحية وصحة تنسيق معرف المستأجر كـ Guid صحيح وغير فارغ
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            // إرجاع كود استجابة 401 في حال غياب أو خطأ معرف المستأجر في رمز التوثيق
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        // استدعاء خدمة المستأجرين لجلب بيانات المستأجر بالمعرف المستخرج
        var result = await _tenantService.GetByIdAsync(tenantId, ct);

        // تحويل النتيجة المرجعة من الخدمة إلى استجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث بيانات الملف التعريفي والبروفايل للمستأجر الحالي المسجل دخوله.
    /// </summary>
    /// <param name="dto">بيانات التحديث المطلوبة للمستأجر الحالي.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر المحدثة بعد حفظ التغييرات بنجاح.</returns>
    [HttpPut("me")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        // استخراج قيمة معرف المستأجر من رمز التوثيق الخاص بالمستخدم الحالي
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        // التحقق من صحة تنسيق معرف المستأجر المستخرج
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            // إرجاع خطأ عدم التصريح 401 لعدم صلاحية أو وجود معرف المستأجر
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        // فرض إسناد معرف المستأجر المستخرج من التوكن لضمان عدم إمكانية تعديل مستأجر آخر
        dto.Id = tenantId;

        // استدعاء خدمة المستأجرين لتنفيذ عملية التحديث
        var result = await _tenantService.UpdateAsync(tenantId, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP موحدة
        return ToActionResult(result);
    }

    /// <summary>
    /// رفع وتحديث الشعار الرسمي (Logo) الخاص بالمستأجر الحالي.
    /// </summary>
    /// <param name="file">ملف الصورة المرفوع عبر نموذج الطلب المتعدد (multipart/form-data).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر بعد تحديث مسار الشعار الجديد.</returns>
    [HttpPost("me/logo")]
    [Consumes("multipart/form-data")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> UploadLogo(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
    {
        // استخراج معرف المستأجر من مطالبات التوكن للمستخدم الحالي
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        // التأكد من صحة معرف المستأجر كـ Guid معتمد
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            // إرجاع 401 عند عدم العثور على معرف مستأجر صحيح
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        // التحقق من وجود ملف مرفوع وألا يكون حجمه صفراً
        if (file is null || file.Length == 0)
        {
            // إرجاع كود 400 في حال عدم تزويد ملف الشعار
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("ملف الشعار مطلوب", "VALIDATION_ERROR"));
        }

        // فحص الامتداد ونوع الملف لضمان كونه صورة صالحة (.jpg, .png, .webp)
        if (!_fileUploadService.IsValidImageExtension(file.FileName))
        {
            // إرجاع كود 400 عند رفع ملف بامتداد غير مسموح
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("نوع الملف غير مسموح به، يرجى رفع صورة صالحة (.jpg, .png, .webp)", "INVALID_FILE_TYPE"));
        }

        // فحص حجم الملف وألا يتجاوز الحد الأقصى المسموح به (5 ميجابايت)
        if (!_fileUploadService.IsWithinSizeLimit(file.Length))
        {
            // إرجاع كود 400 عند تجاوز حجم الصورة للحد الأقصى
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("حجم الصورة يتجاوز الحد المسموح به (5 ميجابايت)", "FILE_TOO_LARGE"));
        }

        // رفع الصورة وحفظها فعلياً على الخادم داخل مجلد شعارات المستأجرين
        var uploadResult = await _fileUploadService.UploadAsync(file, "tenants/logos", ct);

        // التحقق من نجاح عملية الرفع
        if (!uploadResult.IsSuccess)
        {
            // إرجاع الخطأ الناتج عن فشل رفع الملف
            return ToActionResult(uploadResult);
        }

        // تحديث مسار الشعار الجديد في سجل المستأجر بقاعدة البيانات
        var updateResult = await _tenantService.UpdateLogoAsync(tenantId, uploadResult.Data!, ct);

        // إرجاع نتيجة عملية تحديث الشعار كاستجابة HTTP
        return ToActionResult(updateResult);
    }

    /// <summary>
    /// استرجاع قائمة كاملة بجميع المستأجرين المسجلين في النظام.
    /// مخصص للمدراء ومسؤولي النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بكل المستأجرين مغلفة في استجابة موحدة.</returns>
    [HttpGet]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين لجلب القائمة الكاملة لجميع المستأجرين
        var result = await _tenantService.GetAllAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة صفحية (Paged List) بالمستأجرين مع تحديد رقم الصفحة وحجمها.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المستهدفة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة (الافتراضي 10).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة مجزأة صفحاتياً تحتوي على بيانات المستأجرين وإجمالي العناصر والصفحات.</returns>
    [HttpGet("paged")]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        // استدعاء خدمة المستأجرين لجلب الصفحة المطلوبة استناداً لرقم الصفحة وحجمها
        var result = await _tenantService.GetPagedAsync(pageNumber, pageSize, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع البيانات التفصيلية لمستأجر محدد عبر معرفه الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المراد البحث عنه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر المطابق أو كود 404 في حال عدم العثور عليه.</returns>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين للبحث عن المستأجر بالمعرف المحدد
        var result = await _tenantService.GetByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستأجر جديد في النظام وتهيئة إعداداته الافتراضية.
    /// </summary>
    /// <param name="dto">بيانات المستأجر الجديد المراد إنشاؤه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر المنشأ مصحوبة بكود الاستجابة 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين لإنشاء المستأجر والتحقق من عدم تكرار الاسم
        var result = await _tenantService.CreateAsync(dto, ct);

        // التحقق مما إذا كانت عملية الإنشاء قد تكللت بالنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة 201 Created مصحوباً ببيانات المستأجر ورسالة التأكيد
            return StatusCode(201, ApiResponse<TenantResponseDto>.Ok(result.Data!, "تم إنشاء المستأجر بنجاح"));
        }

        // تحويل وإرجاع كود الخطأ في حال فشل العملية
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات مستأجر قائم في النظام.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المطلوب تعديله.</param>
    /// <param name="dto">البيانات الجديدة المراد تطبيقها على المستأجر.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر بعد التحديث أو تفاصيل الخطأ.</returns>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين لتطبيق التعديلات على المستأجر المحدد
        var result = await _tenantService.UpdateAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستأجر نهائياً من النظام في حال عدم ارتباطه ببيانات معلقة تمنع الحذف.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المراد حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح الحذف أو تبين مانع الحذف.</returns>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين لتنفيذ عملية الحذف والتحقق من الشروط المانعة
        var result = await _tenantService.DeleteAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط المستأجر بين التفعيل والتعطيل (Toggle Active Status).
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المراد تغيير حالته.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستأجر بالوضعية المحدثة بعد تبديل حالته.</returns>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        // استدعاء خدمة المستأجرين لتبديل حالة نشاط المستأجر (Active / Inactive)
        var result = await _tenantService.ToggleActiveStatusAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}
