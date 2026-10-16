using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة إدارة نقاط البيع (POS) وحركات الصندوق والورديات النقدية وربط أجهزة الكاشير.
/// </summary>
public interface IPosCashService
{
    /// <summary>
    /// جلب قائمة نقاط البيع المعرفة في النظام مع بيانات الفروع والأدراج الافتراضية.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قائمة نقاط البيع المسجلة أو رسالة فشل</returns>
    // جلب كافة نقاط البيع مع أدراجها وفروعها
    Task<ServiceResult<IReadOnlyList<PosTerminalResponseDto>>> GetTerminalsAsync(CancellationToken ct = default);

    /// <summary>
    /// إنشاء نقطة بيع جديدة وربطها بالفرع ودرج النقدية الافتراضي.
    /// </summary>
    /// <param name="dto">بيانات إنشاء نقطة البيع (الكود، الاسم، الفرع، الدرج)</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات نقطة البيع المنشأة حديثاً</returns>
    // إنشاء نقطة بيع جديدة مع التحقق من صحة الكود والفرع
    Task<ServiceResult<PosTerminalResponseDto>> CreateTerminalAsync(CreatePosTerminalDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات نقطة بيع محددة والتحقق من عدم وجود ورديات مفتوحة تمنع التعديل.
    /// </summary>
    /// <param name="id">معرف نقطة البيع المراد تحديثها</param>
    /// <param name="dto">بيانات التحديث الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات نقطة البيع بعد التحديث</returns>
    // تحديث بيانات نقطة البيع والتحقق من قيود الورديات المفتوحة
    Task<ServiceResult<PosTerminalResponseDto>> UpdateTerminalAsync(Guid id, UpdatePosTerminalDto dto, CancellationToken ct = default);

    /// <summary>
    /// توليد كود اقتران مؤقت لجهاز الكاشير للارتباط بنقطة بيع محددة.
    /// </summary>
    /// <param name="terminalId">معرف نقطة البيع</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>كود الاقتران وصلاحيته الزمنية</returns>
    // توليد رمز اقتران مؤقت جديد لنقطة البيع
    Task<ServiceResult<PairingCodeResponseDto>> GeneratePairingCodeAsync(Guid terminalId, CancellationToken ct = default);

    /// <summary>
    /// ربط جهاز كاشير بنقطة بيع باستخدام كود الاقتران وتوليد توكن وصول الجهاز.
    /// </summary>
    /// <param name="dto">بيانات المطالبة ورمز الاقتران ومعلومات الجهاز</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>توكن الجهاز وبيانات الاعتماد</returns>
    // تأكيد اقتران الجهاز بنقطة البيع وتوليد رمز التعريف المشفر
    Task<ServiceResult<ClaimPosDeviceResponseDto>> ClaimDeviceAsync(ClaimPosDeviceDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب سياق وبيانات نقطة البيع والوردية الحالية باستخدام توكن الجهاز.
    /// </summary>
    /// <param name="deviceToken">توكن تعريف جهاز الكاشير</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>سياق نقطة البيع والوردية النشطة والفرع</returns>
    // استرجاع السياق التشغيلي لنقطة البيع بناءً على توكن الجهاز
    Task<ServiceResult<PosContextDto>> GetContextAsync(string? deviceToken, CancellationToken ct = default);

    /// <summary>
    /// فتح وردية صندوق جديدة لنقطة البيع مع رصيد الافتتاح وتحديد الكاشير المسؤول.
    /// </summary>
    /// <param name="deviceToken">توكن جهاز الكاشير</param>
    /// <param name="dto">بيانات فتح الوردية ومبلغ الافتتاح</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الوردية المفتوحة</returns>
    // فتح وردية نقدية جديدة وتوثيق رصيد البداية
    Task<ServiceResult<CashSessionResponseDto>> OpenSessionAsync(string? deviceToken, OpenCashSessionDto dto, CancellationToken ct = default);

    /// <summary>
    /// إغلاق الوردية النقدية الحالية وحساب العجز أو الفائض ومطابقة النقدية الفعلية.
    /// </summary>
    /// <param name="deviceToken">توكن جهاز الكاشير</param>
    /// <param name="dto">بيانات الإغلاق والنقدية الفعلية المحصية</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الوردية المغلقة ونتائج الجرد</returns>
    // إغلاق الوردية النقدية وحساب الفروقات النقدية
    Task<ServiceResult<CashSessionResponseDto>> CloseSessionAsync(string? deviceToken, CloseCashSessionDto dto, CancellationToken ct = default);

    /// <summary>
    /// اعتماد الفروقات النقدية (العجز أو الفائض) للوردية المغلقة وترحيلها محاسبياً.
    /// </summary>
    /// <param name="sessionId">معرف الوردية النقدية</param>
    /// <param name="dto">بيانات قرار الاعتماد والملاحظات</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الوردية بعد اعتماد الفروقات</returns>
    // اعتماد العجز أو الفائض النقدي للوردية من قبل المشرف
    Task<ServiceResult<CashSessionResponseDto>> ApproveDifferenceAsync(Guid sessionId, ApproveCashDifferenceDto dto, CancellationToken ct = default);

    /// <summary>
    /// التحقق من صلاحية وردية محددة وجاهزيتها للترحيل المالي المرتبط بمستند معين.
    /// </summary>
    /// <param name="sessionId">معرف الوردية</param>
    /// <param name="branchId">معرف الفرع</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>كيان الوردية المفتوحة أو خطأ التحقق</returns>
    // مطابقة وردية مفتوحة للترحيل المالي مع الفرع المحدد
    Task<ServiceResult<CashSession>> ResolveOpenSessionForPostingAsync(Guid sessionId, Guid branchId, CancellationToken ct = default);

    /// <summary>
    /// استرجاع معرف الوردية النقدية المفتوحة المرتبطة بدرج نقدي محدد للترحيل التلقائي.
    /// </summary>
    /// <param name="safeId">معرف درج النقدية/الخزينة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>معرف الوردية المفتوحة إن وجدت</returns>
    // جلب معرف الوردية النشطة لدرج النقدية لإتمام الترحيل
    Task<ServiceResult<Guid?>> ResolveDrawerPostingSessionAsync(Guid safeId, CancellationToken ct = default);

    /// <summary>
    /// التحقق من صحة تعديل الخزينة/الدرج ومنع التعديل في حال وجود ورديات نشطة.
    /// </summary>
    /// <param name="safeId">معرف الخزينة أو الدرج</param>
    /// <param name="newType">النوع الجديد للخزينة</param>
    /// <param name="isActive">حالة النشاط المطلوبة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>نتيجة التحقق من الصلاحية</returns>
    // التحقق من قيود تعديل نوع أو نشاط درج النقدية
    Task<ServiceResult<bool>> ValidateSafeUpdateAsync(Guid safeId, SafeType newType, bool isActive, CancellationToken ct = default);

    /// <summary>منع حذف درج مرتبط بنقطة بيع أو سجل ورديات.</summary>
    Task<ServiceResult<bool>> ValidateSafeDeleteAsync(Guid safeId, CancellationToken ct = default);
}
