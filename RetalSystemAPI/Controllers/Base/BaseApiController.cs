using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Controllers.Base;

/// <summary>
/// المتحكم الأساسي المشترك لجميع متحكمات واجهة برمجة التطبيقات (API Controllers) في النظام.
/// يوفر بنية موحدة لمعالجة وتغليف نتائج طبقة الخدمات (ServiceResult) وتحويلها إلى كائنات استجابة موحدة (ApiResponse)
/// مع ربط أكواد الأخطاء المنطقية بأكواد استجابة HTTP المناسبة (RESTful HTTP Status Codes).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// تحويل نتيجة الخدمة العامة الحاملة للبيانات إلى كائن استجابة HTTP ملائم ومغلف داخل <see cref="ApiResponse{T}"/>.
    /// </summary>
    /// <typeparam name="T">نوع البيانات المحمولة داخل نتيجة الخدمة.</typeparam>
    /// <param name="result">كائن نتيجة الخدمة الحامل لحالة التنفيذ والبيانات أو كود ورسالة الخطأ.</param>
    /// <returns>
    /// كائن <see cref="IActionResult"/> يحتوي على كود 200 OK عند النجاح،
    /// أو كود الخطأ المناسب (400, 401, 404, 409, 422) عند الفشل.
    /// </returns>
    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        // التحقق مما إذا كانت نتيجة تنفيذ عملية الخدمة قد تمت بنجاح
        if (result.IsSuccess)
            // إرجاع كود الاستجابة 200 OK وتغليف البيانات المرجعة في كائن ApiResponse الناجح
            return Ok(ApiResponse<T>.Ok(result.Data!));

        // تحليل وتحديد كود استجابة HTTP المناسب بناءً على كود الخطأ المنطقي الوارد
        int statusCode = ResolveStatusCode(result.ErrorCode);

        // إرجاع كود الخطأ المحدد مصحوباً بتغليف رسالة وكود الخطأ داخل كائن ApiResponse الفاشل
        return StatusCode(statusCode, ApiResponse<T>.Fail(result.ErrorMessage!, result.ErrorCode));
    }

    /// <summary>
    /// تحويل نتيجة الخدمة البسيطة (التي لا تحمل بيانات إضافية) إلى كائن استجابة HTTP ملائم ومغلف داخل <see cref="ApiResponse"/>.
    /// </summary>
    /// <param name="result">كائن نتيجة الخدمة الحامل لحالة التنفيذ ورسالة وكود الخطأ في حال الفشل.</param>
    /// <returns>
    /// كائن <see cref="IActionResult"/> يحتوي على كود 200 OK عند النجاح،
    /// أو كود الخطأ المناسب (400, 401, 404, 409, 422) عند الفشل.
    /// </returns>
    protected IActionResult ToActionResult(ServiceResult result)
    {
        // التحقق من نجاح العملية المنفذة في طبقة الخدمات
        if (result.IsSuccess)
            // إرجاع كود الاستجابة 200 OK مع كائن ApiResponse الفارغ الدال على النجاح
            return Ok(ApiResponse.Ok());

        // تحديد كود الـ HTTP المناسب المقابل لكود الخطأ الوارد من الخدمة
        int statusCode = ResolveStatusCode(result.ErrorCode);

        // إرجاع كود الحالة المطلوب مع تغليف تفاصيل الخطأ في استجابة الفشل الموحدة
        return StatusCode(statusCode, ApiResponse.Fail(result.ErrorMessage!, result.ErrorCode));
    }

    /// <summary>
    /// تعيين كود استجابة الـ HTTP القياسي (Status Code) المناسب لكل كود خطأ منطقي في النظام.
    /// </summary>
    /// <param name="errorCode">كود الخطأ النصي المعرف مسبقاً في ثوابت النظام <see cref="ErrorCodes"/>.</param>
    /// <returns>رقم كود حالة الـ HTTP المناسب (مثل 404, 401, 409, 422, أو 400 افتراضياً).</returns>
    private static int ResolveStatusCode(string? errorCode) => errorCode switch
    {
        // مطابقة حالات عدم وجود المورد أو الكيان المطلوب في قاعدة البيانات (404 Not Found)
        ErrorCodes.NotFound
        or ErrorCodes.TenantNotFound
        or ErrorCodes.BranchNotFound
        or ErrorCodes.CategoryNotFound
        or ErrorCodes.ProductNotFound
        or ErrorCodes.UnitNotFound
        or ErrorCodes.ProductUnitNotFound
        or ErrorCodes.BarCodeNotFound
        or ErrorCodes.ImageNotFound
        or ErrorCodes.SupplierNotFound
        or ErrorCodes.WarehouseNotFound
        or ErrorCodes.StockNotFound
        or ErrorCodes.PurchaseOrderNotFound
        or ErrorCodes.CustomerNotFound
        or ErrorCodes.SalesInvoiceNotFound
        or ErrorCodes.SalesReturnNotFound
        or ErrorCodes.StockTransferNotFound
        or ErrorCodes.StockAdjustmentNotFound => 404,

        // مطابقة حالات فشل المصادقة أو عدم التحقق من هوية وبيانات تسجيل الدخول (401 Unauthorized)
        ErrorCodes.Unauthorized
        or ErrorCodes.InvalidCredentials => 401,

        // مطابقة حالات التضارب وتكرار السجلات الفريدة أو تعارض التزامن (409 Conflict)
        ErrorCodes.UserAlreadyExists
        or ErrorCodes.TenantNameExists
        or ErrorCodes.BranchNameExists
        or ErrorCodes.UnitNameExists
        or ErrorCodes.ProductUnitDuplicate
        or ErrorCodes.BarCodeDuplicate
        or ErrorCodes.CategoryNameExists
        or ErrorCodes.SupplierNameExists
        or ErrorCodes.WarehouseNameExists
        or ErrorCodes.PurchaseOrderNumberExists
        or ErrorCodes.CustomerCodeExists
        or ErrorCodes.SalesInvoiceNumberExists
        or ErrorCodes.SalesReturnNumberExists
        or ErrorCodes.StockTransferNumberExists
        or ErrorCodes.StockAdjustmentNumberExists
        or ErrorCodes.ConcurrencyError => 409,

        // مطابقة حالات القيود المنطقية والعلاقات المانعة للحذف أو التعديل (422 Unprocessable Entity)
        ErrorCodes.BranchHasUsers
        or ErrorCodes.CategoryHasProducts
        or ErrorCodes.UnitInUse
        or ErrorCodes.CategoryCircularRef
        or ErrorCodes.WarehouseHasStock => 422,

        // مطابقة أخطاء التحقق وقواعد الأعمال والمدخلات غير الصالحة (400 Bad Request)
        ErrorCodes.InvalidFileType
        or ErrorCodes.FileTooLarge
        or ErrorCodes.ValidationError
        or ErrorCodes.InsufficientStock
        or ErrorCodes.InsufficientShowroomStock
        or ErrorCodes.CustomerCreditExceeded
        or ErrorCodes.PurchaseOrderInvalidStatus
        or ErrorCodes.SalesInvoiceInvalidStatus
        or ErrorCodes.StockTransferInvalidStatus
        or ErrorCodes.StockTransferSameWarehouse => 400,

        // الحالة الافتراضية لأي كود خطأ عام أو غير معرّف بشكل صريح (400 Bad Request)
        _ => 400
    };
}
