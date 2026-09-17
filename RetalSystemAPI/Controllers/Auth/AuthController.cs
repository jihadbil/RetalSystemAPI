using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Auth.Interfaces;

namespace RetalSystemAPI.Controllers.Auth;

/// <summary>
/// متحكم عمليات المصادقة وإدارة الحسابات وتدفق تسجيل الدخول والخروج في النظام.
/// يوفر نقاط النهاية العامة والآمنة لتوليد رموز JWT والتحقق من صلاحية المستخدمين.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    /// <summary>
    /// خدمة المصادقة والتحقق من الحسابات وتوليد الرموز المميزة.
    /// </summary>
    private readonly IAuthService _authService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المصادقة وحقن خدمة المصادقة.
    /// </summary>
    /// <param name="authService">واجهة خدمة المصادقة وحسابات المستخدمين.</param>
    public AuthController(IAuthService authService)
    {
        // إسناد خدمة المصادقة المحقونة إلى الحقل الخاص
        _authService = authService;
    }

    /// <summary>
    /// تسجيل الدخول للمستخدم والتحقق من بيانات الاعتماد ثم الحصول على رمز التوثيق (JWT Token).
    /// نقطة نهاية عامة ومتاحة للجميع بدون متطلبات توثيق مسبقة.
    /// </summary>
    /// <param name="dto">بيانات تسجيل الدخول التي تشمل اسم المستخدم وكلمة المرور ومعرف المستأجر الاختياري.</param>
    /// <param name="ct">رمز إلغاء العملية لدعم التراجع غير المتزامن عن الطلب.</param>
    /// <returns>بيانات المستخدم ورمز التوثيق وتاريخ الانتهاء مغلفة في كائن استجابة موحد.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المصادقة لتنفيذ عملية التحقق من اسم المستخدم وكلمة المرور وتوليد التوكن
        var result = await _authService.LoginAsync(dto, ct);

        // تحويل نتيجة الخدمة إلى كود استجابة HTTP ملائم (200 OK أو كود الخطأ المناسب)
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء حساب مستخدم جديد في النظام وتسجيله ضمن المستأجر المحدد.
    /// نقطة نهاية عامة تتيح التسجيل الأولي.
    /// </summary>
    /// <param name="dto">بيانات التسجيل الجديد التي تشمل البريد، اسم المستخدم، كلمة المرور، والبيانات الشخصية.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستخدم المنشأ مصحوبة برمز التوثيق مع كود 201 Created عند النجاح.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        // استدعاء خدمة المصادقة لإنشاء الحساب الجديد والتحقق من عدم تكرار البريد أو اسم المستخدم
        var result = await _authService.RegisterAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء الحساب قد اكتملت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مع رسالة التأكيد والبيانات
            return StatusCode(201, ApiResponse<AuthResponseDto>.Ok(result.Data!, "تم إنشاء الحساب بنجاح"));
        }

        // في حال الفشل يتم تحويل نتيجة الخطأ إلى كود HTTP مناسب عبر الدالة الأساسية
        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل الخروج للمستخدم الحالي وإبطال جلسة العمل والرموز النشطة المرتبطة بها.
    /// يتطلب هذا الإجراء توثيقاً مسبقاً (مستخدم مسجل دخول ومعه رمز صالح).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تسجيل الخروج أو رسالة بالخطأ في حال تعذر ذلك.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        // استخراج المعرف الفريد للمستخدم الحالي من مطالبات الهوية (ClaimTypes.NameIdentifier)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // التحقق من وجود معرف المستخدم في رمز التوثيق الحالي
        if (string.IsNullOrEmpty(userId))
        {
            // إرجاع استجابة 401 Unauthorized في حال عدم العثور على هوية المستخدم المعتمدة
            return Unauthorized(ApiResponse.Fail("المستخدم غير مصرح له", "UNAUTHORIZED"));
        }

        // استدعاء خدمة المصادقة لإنهاء جلسة المستخدم الحالية وإبطال الصلاحيات المرتبطة بها
        var result = await _authService.LogoutAsync(userId, ct);

        // تحويل وإرجاع النتيجة النهائية للطلب كاستجابة HTTP موحدة
        return ToActionResult(result);
    }
}
