namespace RetalSystemAPI.Responses;

/// <summary>
/// غلاف استجابة واجهة برمجة التطبيقات الموحد (Generic API Response Envelope) لجميع العمليات التي تُرجع بيانات.
/// يضمن توحيد شكل المخرجات لجميع عملاء الـ API (تطبيقات الويب والموبايل) شاملاً مؤشر النجاح، البيانات، الرسالة وكود الخطأ.
/// </summary>
/// <typeparam name="T">نوع البيانات المغلفة داخل الاستجابة.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// مؤشر يعبر عن حالة نجاح العملية (true) أو فشلها (false).
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// البيانات الفعلية المرجعة من العملية في حال النجاح.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// رسالة نصية توضيحية للمستخدم أو العميل تعبر عن نتيجة العملية أو تفاصيل الخطأ.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// كود الخطأ المنطقي المعياري في النظام للمساعدة في المعالجة البرمجية بالواجهات الأمامية.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// إنشاء استجابة ناجحة جديدة تحتوي على البيانات ورسالة اختيارية.
    /// </summary>
    /// <param name="data">البيانات المراد إرجاعها.</param>
    /// <param name="message">رسالة نجاح اختيارية توضح تفاصيل العملية.</param>
    /// <returns>نسخة مهيأة من <see cref="ApiResponse{T}"/> تشير إلى النجاح.</returns>
    public static ApiResponse<T> Ok(T data, string? message = null)
        // تهيئة وإرجاع كائن استجابة جديد مع تعيين النجاح إلى صحيح وتعبئة البيانات والرسالة
        => new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// إنشاء استجابة غير ناجحة تحتوي على رسالة خطأ وكود خطأ اختياري.
    /// </summary>
    /// <param name="message">نص رسالة الخطأ التوضيحية.</param>
    /// <param name="code">كود الخطأ المعياري في النظام.</param>
    /// <returns>نسخة مهيأة من <see cref="ApiResponse{T}"/> تشير إلى الفشل.</returns>
    public static ApiResponse<T> Fail(string message, string? code = null)
        // تهيئة وإرجاع كائن استجابة جديد مع تعيين النجاح إلى خطأ وتعبئة رسالة وكود الخطأ
        => new() { Success = false, Message = message, ErrorCode = code };
}

/// <summary>
/// غلاف استجابة واجهة برمجة التطبيقات الموحد (Non-generic API Response Envelope) للعمليات التي لا تتطلب إرجاع بيانات (مثل الحذف أو التحديث البسيط).
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// مؤشر يعبر عن حالة نجاح العملية (true) أو فشلها (false).
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// رسالة نصية توضيحية للمستخدم أو العميل تعبر عن نتيجة العملية أو تفاصيل الخطأ.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// كود الخطأ المنطقي المعياري في النظام.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// إنشاء استجابة ناجحة جديدة بدون بيانات مع رسالة اختيارية.
    /// </summary>
    /// <param name="message">رسالة نجاح اختيارية.</param>
    /// <returns>نسخة مهيأة من <see cref="ApiResponse"/> تشير إلى النجاح.</returns>
    public static ApiResponse Ok(string? message = null)
        // تهيئة وإرجاع كائن استجابة دال على نجاح العملية مع الرسالة المصاحبة
        => new() { Success = true, Message = message };

    /// <summary>
    /// إنشاء استجابة فاشلة بدون بيانات مع رسالة وكود خطأ اختياري.
    /// </summary>
    /// <param name="message">نص رسالة الخطأ التوضيحية.</param>
    /// <param name="code">كود الخطأ المعياري في النظام.</param>
    /// <returns>نسخة مهيأة من <see cref="ApiResponse"/> تشير إلى الفشل.</returns>
    public static ApiResponse Fail(string message, string? code = null)
        // تهيئة وإرجاع كائن استجابة دال على فشل العملية مع تعبئة رسالة وكود الخطأ
        => new() { Success = false, Message = message, ErrorCode = code };
}
