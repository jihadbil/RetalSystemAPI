using System;
using System.Collections.Generic;
using System.Linq;

namespace RetalSystemAPI.Models.Constants;

/// <summary>
/// ثوابت ومصفوفة الصلاحيات والأذونات لكافة شاشات وعمليات نظام رتال.
/// </summary>
// كلاس ثابت يحتوي على كافة مجموعات ورموز الصلاحيات والأذونات في النظام
public static class Permissions
{
    /// <summary>
    /// نوع المطالبة (Claim Type) المستخدم في توكن JWT لتمثيل الصلاحيات.
    /// </summary>
    // نوع ادعاء الصلاحية المعياري
    public const string ClaimType = "Permission";

    // ── لوحة التحكم ──

    /// <summary>
    /// صلاحيات وأذونات لوحة التحكم الرئيسية والمؤشرات الإحصائية.
    /// </summary>
    // مجموعة صلاحيات لوحة التحكم
    public static class Dashboard
    {
        /// <summary>صلاحية استعراض لوحة التحكم والمؤشرات</summary>
        // إذن عرض لوحة التحكم
        public const string View = "Permissions.Dashboard.View";
    }

    // ── نقطة البيع POS ──

    /// <summary>
    /// صلاحيات وأذونات شاشات وعمليات نقطة البيع السريعة (POS).
    /// </summary>
    // مجموعة صلاحيات نقطة البيع
    public static class Pos
    {
        /// <summary>صلاحية الدخول لشاشة نقطة البيع</summary>
        // إذن عرض نقطة البيع
        public const string View = "Permissions.Pos.View";

        /// <summary>صلاحية إنشاء وإتمام عمليات البيع</summary>
        // إذن إنشاء عملية بيع
        public const string CreateSale = "Permissions.Pos.CreateSale";

        /// <summary>صلاحية منح وتطبيق الخصومات في نقطة البيع</summary>
        // إذن تطبيق الخصم
        public const string ApplyDiscount = "Permissions.Pos.ApplyDiscount";

        /// <summary>صلاحية إعادة طباعة إيصالات وفواتير سابقة</summary>
        // إذن إعادة طباعة الإيصال
        public const string Reprint = "Permissions.Pos.Reprint";

        /// <summary>صلاحية افتتاح وردية وجلسة كاشير جديدة</summary>
        // إذن فتح الجلسة
        public const string OpenSession = "Permissions.Pos.OpenSession";

        /// <summary>صلاحية جرد وإغلاق وردية الكاشير</summary>
        // إذن إغلاق الجلسة
        public const string CloseSession = "Permissions.Pos.CloseSession";
    }

    // ── الأصناف والمنتجات ──

    /// <summary>
    /// صلاحيات وأذونات كتالوج الأصناف والمنتجات والأسعار.
    /// </summary>
    // مجموعة صلاحيات المنتجات
    public static class Products
    {
        /// <summary>صلاحية استعراض قائمة المنتجات</summary>
        // إذن عرض المنتجات
        public const string View = "Permissions.Products.View";

        /// <summary>صلاحية إنشاء وإضافة منتج جديد</summary>
        // إذن إضافة منتج
        public const string Create = "Permissions.Products.Create";

        /// <summary>صلاحية تعديل بيانات وأسعار المنتجات</summary>
        // إذن تعديل منتج
        public const string Edit = "Permissions.Products.Edit";

        /// <summary>صلاحية حذف منتج من النظام</summary>
        // إذن حذف منتج
        public const string Delete = "Permissions.Products.Delete";

        /// <summary>صلاحية تصدير واستيراد الأصناف عبر ملفات إكسل</summary>
        // إذن استيراد وتصدير إكسل
        public const string ExportImport = "Permissions.Products.ExportImport";
    }

    // ── التصنيفات ──

    /// <summary>
    /// صلاحيات وأذونات إدارة شجرة وتصنيفات الأصناف.
    /// </summary>
    // مجموعة صلاحيات التصنيفات
    public static class Categories
    {
        /// <summary>صلاحية استعراض شجرة التصنيفات</summary>
        // إذن عرض التصنيفات
        public const string View = "Permissions.Categories.View";

        /// <summary>صلاحية إنشاء تصنيف جديد</summary>
        // إذن إضافة تصنيف
        public const string Create = "Permissions.Categories.Create";

        /// <summary>صلاحية تعديل مسميات وبيانات التصنيفات</summary>
        // إذن تعديل تصنيف
        public const string Edit = "Permissions.Categories.Edit";

        /// <summary>صلاحية حذف تصنيف من النظام</summary>
        // إذن حذف تصنيف
        public const string Delete = "Permissions.Categories.Delete";
    }

    // ── وحدات القياس ──

    /// <summary>
    /// صلاحيات وأذونات وحدات قياس وعبوات الأصناف.
    /// </summary>
    // مجموعة صلاحيات وحدات القياس
    public static class Units
    {
        /// <summary>صلاحية استعراض قائمة وحدات القياس</summary>
        // إذن عرض الوحدات
        public const string View = "Permissions.Units.View";

        /// <summary>صلاحية إضافة وحدة قياس جديدة</summary>
        // إذن إضافة وحدة
        public const string Create = "Permissions.Units.Create";

        /// <summary>صلاحية تعديل بيانات وحدة القياس</summary>
        // إذن تعديل وحدة
        public const string Edit = "Permissions.Units.Edit";

        /// <summary>صلاحية حذف وحدة قياس من النظام</summary>
        // إذن حذف وحدة
        public const string Delete = "Permissions.Units.Delete";
    }

    // ── فواتير المبيعات ──

    /// <summary>
    /// صلاحيات وأذونات إدارة وعرض فواتير المبيعات.
    /// </summary>
    // مجموعة صلاحيات فواتير المبيعات
    public static class SalesInvoices
    {
        /// <summary>صلاحية استعراض فواتير المبيعات</summary>
        // إذن عرض فواتير المبيعات
        public const string View = "Permissions.SalesInvoices.View";

        /// <summary>صلاحية إنشاء وتحرير فاتورة مبيعات جديدة</summary>
        // إذن إنشاء فاتورة مبيعات
        public const string Create = "Permissions.SalesInvoices.Create";

        /// <summary>صلاحية تعديل بيانات وبنود فاتورة مبيعات</summary>
        // إذن تعديل فاتورة مبيعات
        public const string Edit = "Permissions.SalesInvoices.Edit";

        /// <summary>صلاحية حذف أو إلغاء فاتورة مبيعات</summary>
        // إذن حذف فاتورة مبيعات
        public const string Delete = "Permissions.SalesInvoices.Delete";

        /// <summary>صلاحية طباعة فاتورة وإيصال المبيعات</summary>
        // إذن طباعة فاتورة مبيعات
        public const string Print = "Permissions.SalesInvoices.Print";
    }

    // ── مرتجعات المبيعات ──

    /// <summary>
    /// صلاحيات وأذونات إدارة وسندات مردودات المبيعات.
    /// </summary>
    // مجموعة صلاحيات مرتجعات المبيعات
    public static class SalesReturns
    {
        /// <summary>صلاحية استعراض قائمة مرتجعات المبيعات</summary>
        // إذن عرض مرتجعات المبيعات
        public const string View = "Permissions.SalesReturns.View";

        /// <summary>صلاحية تسجيل وإصدار سند مرتجع مبيعات</summary>
        // إذن إنشاء مرتجع مبيعات
        public const string Create = "Permissions.SalesReturns.Create";

        /// <summary>صلاحية حذف أو إلغاء سند مرتجع مبيعات</summary>
        // إذن حذف مرتجع مبيعات
        public const string Delete = "Permissions.SalesReturns.Delete";

        /// <summary>صلاحية تسجيل مرتجع مبيعات دون ربطه بفاتورة أصلية</summary>
        // إذن مرتجع بدون فاتورة
        public const string ReturnWithoutInvoice = "Permissions.SalesReturns.ReturnWithoutInvoice";
    }

    // ── العملاء والحسابات ──

    /// <summary>
    /// صلاحيات وأذونات إدارة ملفات العملاء والحسابات المدينة.
    /// </summary>
    // مجموعة صلاحيات العملاء
    public static class Customers
    {
        /// <summary>صلاحية استعراض قائمة وكشوفات حساب العملاء</summary>
        // إذن عرض العملاء
        public const string View = "Permissions.Customers.View";

        /// <summary>صلاحية تسجيل وإضافة ملف عميل جديد</summary>
        // إذن إضافة عميل
        public const string Create = "Permissions.Customers.Create";

        /// <summary>صلاحية تعديل بيانات وسقف دين العميل</summary>
        // إذن تعديل بيانات عميل
        public const string Edit = "Permissions.Customers.Edit";

        /// <summary>صلاحية حذف ملف العميل</summary>
        // إذن حذف عميل
        public const string Delete = "Permissions.Customers.Delete";
    }

    // ── فواتير المشتريات ──

    /// <summary>
    /// صلاحيات وأذونات فواتير المشتريات المباشرة من الموردين.
    /// </summary>
    // مجموعة صلاحيات فواتير المشتريات
    public static class PurchaseInvoices
    {
        /// <summary>صلاحية استعراض فواتير المشتريات</summary>
        // إذن عرض فواتير المشتريات
        public const string View = "Permissions.PurchaseInvoices.View";

        /// <summary>صلاحية تسجيل وإدخال فاتورة مشتريات جديدة</summary>
        // إذن تسجيل فاتورة مشتريات
        public const string Create = "Permissions.PurchaseInvoices.Create";

        /// <summary>صلاحية تعديل بيانات وأسعار فاتورة المشتريات</summary>
        // إذن تعديل فاتورة مشتريات
        public const string Edit = "Permissions.PurchaseInvoices.Edit";

        /// <summary>صلاحية إلغاء أو حذف فاتورة المشتريات</summary>
        // إذن حذف فاتورة مشتريات
        public const string Delete = "Permissions.PurchaseInvoices.Delete";
    }

    // ── مرتجعات المشتريات ──

    /// <summary>
    /// صلاحيات وأذونات سندات مردودات المشتريات للموردين.
    /// </summary>
    // مجموعة صلاحيات مرتجعات المشتريات
    public static class PurchaseReturns
    {
        /// <summary>صلاحية استعراض قائمة مرتجعات المشتريات</summary>
        // إذن عرض مرتجعات المشتريات
        public const string View = "Permissions.PurchaseReturns.View";

        /// <summary>صلاحية تسجيل سند مرتجع مشتريات للمورد</summary>
        // إذن إنشاء مرتجع مشتريات
        public const string Create = "Permissions.PurchaseReturns.Create";

        /// <summary>صلاحية حذف أو إلغاء سند مرتجع مشتريات</summary>
        // إذن حذف مرتجع مشتريات
        public const string Delete = "Permissions.PurchaseReturns.Delete";
    }

    // ── أوامر وطلبيات الشراء ──

    /// <summary>
    /// صلاحيات وأذونات أوامر وطلبيات الشراء الموجهة للموردين.
    /// </summary>
    // مجموعة صلاحيات أوامر الشراء
    public static class PurchaseOrders
    {
        /// <summary>صلاحية استعراض أوامر الشراء</summary>
        // إذن عرض أوامر الشراء
        public const string View = "Permissions.PurchaseOrders.View";

        /// <summary>صلاحية إنشاء وإصدار طلبية شراء جديدة</summary>
        // إذن إنشاء أمر شراء
        public const string Create = "Permissions.PurchaseOrders.Create";

        /// <summary>صلاحية تعديل بنود وكميات أمر الشراء</summary>
        // إذن تعديل أمر شراء
        public const string Edit = "Permissions.PurchaseOrders.Edit";

        /// <summary>صلاحية اعتماد أمر الشراء وتحويله إلى استلام</summary>
        // إذن اعتماد أمر الشراء
        public const string Approve = "Permissions.PurchaseOrders.Approve";

        /// <summary>صلاحية حذف أو إلغاء أمر الشراء</summary>
        // إذن حذف أمر شراء
        public const string Delete = "Permissions.PurchaseOrders.Delete";
    }

    // ── الموردين ──

    /// <summary>
    /// صلاحيات وأذونات إدارة ملفات الموردين والحسابات الدائنة.
    /// </summary>
    // مجموعة صلاحيات الموردين
    public static class Suppliers
    {
        /// <summary>صلاحية استعراض قائمة وكشوفات حساب الموردين</summary>
        // إذن عرض الموردين
        public const string View = "Permissions.Suppliers.View";

        /// <summary>صلاحية تسجيل ملف مورد جديد</summary>
        // إذن إضافة مورد
        public const string Create = "Permissions.Suppliers.Create";

        /// <summary>صلاحية تعديل بيانات وأرقام هواتف المورد</summary>
        // إذن تعديل مورد
        public const string Edit = "Permissions.Suppliers.Edit";

        /// <summary>صلاحية حذف مورد من النظام</summary>
        // إذن حذف مورد
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    // ── المخازن والمستودعات ──

    /// <summary>
    /// صلاحيات وأذونات إدارة المستودعات وصالات العرض.
    /// </summary>
    // مجموعة صلاحيات المخازن
    public static class Warehouses
    {
        /// <summary>صلاحية استعراض قائمة المخازن وصالات العرض</summary>
        // إذن عرض المخازن
        public const string View = "Permissions.Warehouses.View";

        /// <summary>صلاحية إنشاء مستودع أو صالة عرض جديدة</summary>
        // إذن إضافة مخزن
        public const string Create = "Permissions.Warehouses.Create";

        /// <summary>صلاحية تعديل بيانات ومسميات المخازن</summary>
        // إذن تعديل مخزن
        public const string Edit = "Permissions.Warehouses.Edit";

        /// <summary>صلاحية حذف مخزن من المنظومة</summary>
        // إذن حذف مخزن
        public const string Delete = "Permissions.Warehouses.Delete";
    }

    // ── إدارة المخزون والأرصدة ──

    /// <summary>
    /// صلاحيات وأذونات استعراض وضبط أرصدة المخزون.
    /// </summary>
    // مجموعة صلاحيات إدارة المخزون
    public static class Stock
    {
        /// <summary>صلاحية استعراض حركات وكميات الأرصدة المخزنية</summary>
        // إذن عرض المخزون
        public const string View = "Permissions.Stock.View";

        /// <summary>صلاحية إدخال وتعديل الأرصدة الافتتاحية للصنف يدوياً</summary>
        // إذن ضبط رصيد المخزون
        public const string Adjust = "Permissions.Stock.Adjust";
    }

    // ── التحويلات المخزنية ──

    /// <summary>
    /// صلاحيات وأذونات مناقلات وتحويلات البضاعة بين المخازن.
    /// </summary>
    // مجموعة صلاحيات التحويلات المخزنية
    public static class StockTransfers
    {
        /// <summary>صلاحية استعراض سجل التحويلات المخزنية</summary>
        // إذن عرض التحويلات المخزنية
        public const string View = "Permissions.StockTransfers.View";

        /// <summary>صلاحية إنشاء وإصدار إذن تحويل بضاعة</summary>
        // إذن إنشاء تحويل مخزني
        public const string Create = "Permissions.StockTransfers.Create";

        /// <summary>صلاحية اعتماد وتأكيد استلام التحويل في المستودع المستهدف</summary>
        // إذن اعتماد التحويل
        public const string Approve = "Permissions.StockTransfers.Approve";

        /// <summary>صلاحية إلغاء أو حذف إذن تحويل غير معتمد</summary>
        // إذن حذف تحويل مخزني
        public const string Delete = "Permissions.StockTransfers.Delete";
    }

    // ── التسويات الجردية ──

    /// <summary>
    /// صلاحيات وأذونات عمليات وسندات الجرد والتسوية المخزنية.
    /// </summary>
    // مجموعة صلاحيات التسويات الجردية
    public static class StockAdjustments
    {
        /// <summary>صلاحية استعراض سجل التسويات الجردية</summary>
        // إذن عرض التسويات الجردية
        public const string View = "Permissions.StockAdjustments.View";

        /// <summary>صلاحية تحرير سند تسوية جردية لتسجيل الفروقات</summary>
        // إذن إنشاء تسوية جردية
        public const string Create = "Permissions.StockAdjustments.Create";

        /// <summary>صلاحية اعتماد وترحيل التسوية الجردية للأرصدة الفعلية</summary>
        // إذن اعتماد التسوية الجردية
        public const string Approve = "Permissions.StockAdjustments.Approve";

        /// <summary>صلاحية إلغاء سند تسوية جردية غير مرحل</summary>
        // إذن إلغاء تسوية جردية
        public const string Delete = "Permissions.StockAdjustments.Delete";
    }

    // ── الفروع ──

    /// <summary>
    /// صلاحيات وأذونات إدارة وتأسيس فروع المنشأة.
    /// </summary>
    // مجموعة صلاحيات الفروع
    public static class Branches
    {
        /// <summary>صلاحية استعراض قائمة الفروع وتفاصيلها</summary>
        // إذن عرض الفروع
        public const string View = "Permissions.Branches.View";

        /// <summary>صلاحية إنشاء وتأسيس فرع جديد</summary>
        // إذن إضافة فرع
        public const string Create = "Permissions.Branches.Create";

        /// <summary>صلاحية تعديل بيانات وعناوين وهواتف الفرع</summary>
        // إذن تعديل فرع
        public const string Edit = "Permissions.Branches.Edit";

        /// <summary>صلاحية حذف فرع من النظام</summary>
        // إذن حذف فرع
        public const string Delete = "Permissions.Branches.Delete";
    }

    // ── المستخدمين والأدوار ──

    /// <summary>
    /// صلاحيات وأذونات إدارة حسابات المستخدمين.
    /// </summary>
    // مجموعة صلاحيات إدارة المستخدمين
    public static class Users
    {
        /// <summary>صلاحية استعراض قائمة حسابات المستخدمين</summary>
        // إذن عرض المستخدمين
        public const string View = "Permissions.Users.View";

        /// <summary>صلاحية إنشاء حساب مستخدم جديد</summary>
        // إذن إضافة مستخدم
        public const string Create = "Permissions.Users.Create";

        /// <summary>صلاحية تعديل بيانات وصلاحيات وفروع المستخدمين</summary>
        // إذن تعديل مستخدم
        public const string Edit = "Permissions.Users.Edit";

        /// <summary>صلاحية حذف حساب مستخدم نهائياً</summary>
        // إذن حذف مستخدم
        public const string Delete = "Permissions.Users.Delete";

        /// <summary>صلاحية إعادة تعيين وتغيير كلمة المرور لأي مستخدم</summary>
        // إذن إعادة تعيين كلمة المرور
        public const string ResetPassword = "Permissions.Users.ResetPassword";
    }

    /// <summary>
    /// صلاحيات وأذونات إدارة الأدوار الوظيفية ومصفوفة الصلاحيات.
    /// </summary>
    // مجموعة صلاحيات الأدوار الوظيفية
    public static class Roles
    {
        /// <summary>صلاحية استعراض الأدوار الوظيفية والصلاحيات المنسوبة لها</summary>
        // إذن عرض الأدوار
        public const string View = "Permissions.Roles.View";

        /// <summary>صلاحية تعديل وتعيين وحفظ مصفوفة الصلاحيات للأدوار</summary>
        // إذن إدارة الأدوار
        public const string Manage = "Permissions.Roles.Manage";
    }

    // ── ملف المنشأة ──

    /// <summary>
    /// صلاحيات وأذونات إدارة بيانات المستأجر والملف التعريفي للمؤسسة.
    /// </summary>
    // مجموعة صلاحيات المستأجر
    public static class Tenants
    {
        /// <summary>صلاحية استعراض بيانات المنشأة والاشتراك</summary>
        // إذن عرض بيانات المنشأة
        public const string View = "Permissions.Tenants.View";

        /// <summary>صلاحية تعديل بيانات المؤسسة والشعار والإعدادات</summary>
        // إذن تعديل بيانات المؤسسة
        public const string Edit = "Permissions.Tenants.Edit";
    }

    // ── الخزائن والبنوك والحسابات المالية ──

    /// <summary>
    /// صلاحيات وأذونات إدارة الخزائن النقدية والحسابات البنكية والمناقلات.
    /// </summary>
    // مجموعة صلاحيات الخزائن والبنوك
    public static class Safes
    {
        /// <summary>صلاحية استعراض قائمة الخزائن والحسابات المصرفية وأرصدتها</summary>
        // إذن عرض الخزائن
        public const string View = "Permissions.Safes.View";

        /// <summary>صلاحية إنشاء خزينة أو حساب مصرفي جديد وتحديد الرصيد الافتتاحي</summary>
        // إذن إضافة خزينة
        public const string Create = "Permissions.Safes.Create";

        /// <summary>صلاحية تعديل بيانات ومسميات الخزائن والحسابات البنكية</summary>
        // إذن تعديل خزينة
        public const string Edit = "Permissions.Safes.Edit";

        /// <summary>صلاحية حذف الخزينة الخالية من المعاملات</summary>
        // إذن حذف خزينة
        public const string Delete = "Permissions.Safes.Delete";

        /// <summary>صلاحية إجراء مناقلات وتحويلات مالية بين الخزائن والبنوك</summary>
        // إذن تحويل أموال بين الخزائن
        public const string Transfer = "Permissions.Safes.Transfer";

        /// <summary>صلاحية تسجيل إيداعات وسحوبات يدوية وعرض كشف حركات الخزينة</summary>
        // إذن حركات الخزينة اليدوية
        public const string Transactions = "Permissions.Safes.Transactions";
    }

    // ── التدقيق والتسوية المالية ──

    /// <summary>
    /// صلاحيات وأذونات التدقيق والمطابقة والتسوية المالية للمستندات والدفاتر.
    /// </summary>
    // مجموعة صلاحيات التسوية المالية
    public static class FinancialReconciliation
    {
        /// <summary>صلاحية فحص وتدقيق اتساق المستندات ودفاتر العملاء والموردين وأرصدة الخزائن</summary>
        // إذن تدقيق التسوية المالية
        public const string View = "Permissions.FinancialReconciliation.View";
    }

    // ── تصنيفات المصروفات ──

    /// <summary>
    /// صلاحيات وأذونات تصنيفات وبنود المصروفات التشغيلية.
    /// </summary>
    // مجموعة صلاحيات تصنيفات المصروفات
    public static class ExpenseCategories
    {
        /// <summary>صلاحية استعراض قائمة تصنيفات بنود المصروفات</summary>
        // إذن عرض تصنيفات المصروفات
        public const string View = "Permissions.ExpenseCategories.View";

        /// <summary>صلاحية إضافة تصنيف جديد للمصروفات التشغيلية</summary>
        // إذن إضافة تصنيف مصروفات
        public const string Create = "Permissions.ExpenseCategories.Create";

        /// <summary>صلاحية تعديل مسمى وحالة نشاط تصنيف المصروفات</summary>
        // إذن تعديل تصنيف مصروفات
        public const string Edit = "Permissions.ExpenseCategories.Edit";

        /// <summary>صلاحية حذف تصنيف المصروفات غير المرتبط بسندات</summary>
        // إذن حذف تصنيف مصروفات
        public const string Delete = "Permissions.ExpenseCategories.Delete";
    }

    // ── سندات المصروفات التشغيلية ──

    /// <summary>
    /// صلاحيات وأذونات تحرير وإدارة سندات صرف المصروفات.
    /// </summary>
    // مجموعة صلاحيات سندات المصروفات
    public static class Expenses
    {
        /// <summary>صلاحية استعراض وسندات المصروفات التشغيلية والبحث فيها</summary>
        // إذن عرض سندات المصروفات
        public const string View = "Permissions.Expenses.View";

        /// <summary>صلاحية تحرير وإنشاء سند صرف مصروف جديد والخصم من الخزينة</summary>
        // إذن إنشاء سند مصروف
        public const string Create = "Permissions.Expenses.Create";

        /// <summary>صلاحية تعديل البيانات الوصفية لسند المصروف</summary>
        // إذن تعديل سند مصروف
        public const string Edit = "Permissions.Expenses.Edit";

        /// <summary>صلاحية إلغاء وحذف سند المصروف واسترداد قيمته للخزينة</summary>
        // إذن حذف سند مصروف
        public const string Delete = "Permissions.Expenses.Delete";
    }
}

/// <summary>
/// نموذج وصف وتعريف الصلاحية للاستخدام في مصفوفة الصلاحيات وشاشات إدارة المستخدمين في الواجهات.
/// </summary>
// كلاس كيان تعريف الصلاحية المعروض في الواجهات
public class PermissionDefinition
{
    /// <summary>
    /// كود الصلاحية البرمجي الفريد (مثال: Permissions.Products.View).
    /// </summary>
    // خاصية كود الصلاحية
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// الاسم المعرب المعروض للصلاحية في شاشات الإعداد.
    /// </summary>
    // خاصية اسم الصلاحية بالعربية
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// التصنيف العام أو الوحدة التي تنتمي إليها الصلاحية (المبيعات، المخازن، إلخ).
    /// </summary>
    // خاصية فئة أو تصنيف الصلاحية
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// الوصف التفصيلي للأثر والعمليات التي تتيحها هذه الصلاحية للمستخدم.
    /// </summary>
    // خاصية وصف دور الصلاحية
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// يحدد ما إذا كانت الصلاحية تمنح حق الدخول لشاشة رئيسية في القوائم.
    /// </summary>
    // خاصية مؤشر كون الصلاحية تصريح لدخول شاشة
    public bool IsScreenAccess { get; set; }
}

/// <summary>
/// سجل كامل بجميع الصلاحيات المعرفة في النظام مع أسمائها وتصنيفاتها العربية وتوفير دوال استرجاع الأكواد.
/// </summary>
// كلاس ثابت يحتوي على القائمة المركزية الشاملة لكافة الصلاحيات المعرفة
public static class AppPermissions
{
    /// <summary>
    /// القائمة الشاملة لجميع تعريفات الصلاحيات المعتمدة في النظام.
    /// </summary>
    // قائمة الصلاحيات العامة المجمعة
    public static readonly List<PermissionDefinition> All = new()
    {
        // لوحة التحكم
        new() { Code = Permissions.Dashboard.View, Name = "دخول لوحة التحكم", Category = "لوحة التحكم", Description = "عرض الإحصائيات العامة والمؤشرات البيانية", IsScreenAccess = true },

        // نقطة البيع
        new() { Code = Permissions.Pos.View, Name = "دخول نقطة البيع", Category = "نقطة البيع (POS)", Description = "الدخول لشاشة الكاشير ونقطة البيع السريعة", IsScreenAccess = true },
        new() { Code = Permissions.Pos.CreateSale, Name = "إتمام المبيعات", Category = "نقطة البيع (POS)", Description = "إتمام عمليات البيع وطباعة الإيصال", IsScreenAccess = false },
        new() { Code = Permissions.Pos.ApplyDiscount, Name = "منح خصم كاشير", Category = "نقطة البيع (POS)", Description = "تطبيق خصم إضافي على الفاتورة في نقطة البيع", IsScreenAccess = false },
        new() { Code = Permissions.Pos.Reprint, Name = "إعادة طباعة الإيصالات", Category = "نقطة البيع (POS)", Description = "إعادة طباعة فواتير وإيصالات سابقة", IsScreenAccess = false },
        new() { Code = Permissions.Pos.OpenSession, Name = "فتح وردية نقدية", Category = "نقطة البيع (POS)", Description = "فتح وردية على درج النقود المرتبط بالجهاز", IsScreenAccess = false },
        new() { Code = Permissions.Pos.CloseSession, Name = "إغلاق وردية نقدية", Category = "نقطة البيع (POS)", Description = "جرد وإغلاق وردية درج النقود", IsScreenAccess = false },

        // المنتجات والأصناف
        new() { Code = Permissions.Products.View, Name = "دخول المنتجات والأصناف", Category = "الأصناف والمنتجات", Description = "عرض قائمة المنتجات والأسعار والباركودات", IsScreenAccess = true },
        new() { Code = Permissions.Products.Create, Name = "إضافة منتج جديد", Category = "الأصناف والمنتجات", Description = "إضافة أصناف جديدة وتحديد أسعارها", IsScreenAccess = false },
        new() { Code = Permissions.Products.Edit, Name = "تعديل بيانات المنتج", Category = "الأصناف والمنتجات", Description = "تعديل أسعار وبيانات ووحدات الأصناف", IsScreenAccess = false },
        new() { Code = Permissions.Products.Delete, Name = "حذف منتج", Category = "الأصناف والمنتجات", Description = "حذف الصنف نهائياً من المنظومة", IsScreenAccess = false },
        new() { Code = Permissions.Products.ExportImport, Name = "استيراد وتصدير Excel", Category = "الأصناف والمنتجات", Description = "تصدير الأصناف واستيرادها من ملفات Excel", IsScreenAccess = false },

        // التصنيفات
        new() { Code = Permissions.Categories.View, Name = "دخول التصنيفات", Category = "التصنيفات", Description = "عرض قائمة تصنيفات الأصناف", IsScreenAccess = true },
        new() { Code = Permissions.Categories.Create, Name = "إضافة تصنيف", Category = "التصنيفات", Description = "إنشاء تصنيف رئيسي أو فرعي جديد", IsScreenAccess = false },
        new() { Code = Permissions.Categories.Edit, Name = "تعديل تصنيف", Category = "التصنيفات", Description = "تعديل بيانات وتسمية التصنيف", IsScreenAccess = false },
        new() { Code = Permissions.Categories.Delete, Name = "حذف تصنيف", Category = "التصنيفات", Description = "حذف التصنيف من النظام", IsScreenAccess = false },

        // وحدات القياس
        new() { Code = Permissions.Units.View, Name = "دخول وحدات القياس", Category = "وحدات القياس", Description = "عرض وحدات القياس المعرفة للأصناف", IsScreenAccess = true },
        new() { Code = Permissions.Units.Create, Name = "إضافة وحدة قياس", Category = "وحدات القياس", Description = "إضافة وحدة قياس جديدة", IsScreenAccess = false },
        new() { Code = Permissions.Units.Edit, Name = "تعديل وحدة قياس", Category = "وحدات القياس", Description = "تعديل وحدة القياس", IsScreenAccess = false },
        new() { Code = Permissions.Units.Delete, Name = "حذف وحدة قياس", Category = "وحدات القياس", Description = "حذف وحدة القياس", IsScreenAccess = false },

        // فواتير المبيعات
        new() { Code = Permissions.SalesInvoices.View, Name = "دخول فواتير المبيعات", Category = "المبيعات", Description = "عرض قائمة فواتير المبيعات وتفاصيلها", IsScreenAccess = true },
        new() { Code = Permissions.SalesInvoices.Create, Name = "إنشاء فاتورة مبيعات", Category = "المبيعات", Description = "تحرير وإنشاء فاتورة مبيعات جديدة", IsScreenAccess = false },
        new() { Code = Permissions.SalesInvoices.Edit, Name = "تعديل فاتورة مبيعات", Category = "المبيعات", Description = "تعديل بنود فاتورة مبيعات قائمة", IsScreenAccess = false },
        new() { Code = Permissions.SalesInvoices.Delete, Name = "حذف فاتورة مبيعات", Category = "المبيعات", Description = "إلغاء أو حذف فاتورة مبيعات", IsScreenAccess = false },
        new() { Code = Permissions.SalesInvoices.Print, Name = "طباعة فاتورة مبيعات", Category = "المبيعات", Description = "طباعة تقرير وفاتورة المبيعات", IsScreenAccess = false },

        // مرتجعات المبيعات
        new() { Code = Permissions.SalesReturns.View, Name = "دخول مرتجعات المبيعات", Category = "المبيعات", Description = "عرض قائمة مرتجعات المبيعات", IsScreenAccess = true },
        new() { Code = Permissions.SalesReturns.Create, Name = "إنشاء مرتجع مبيعات", Category = "المبيعات", Description = "تسجيل مرتجع بضاعة واسترجاع المبالغ للعميل", IsScreenAccess = false },
        new() { Code = Permissions.SalesReturns.Delete, Name = "حذف مرتجع مبيعات", Category = "المبيعات", Description = "إلغاء مرتجع مبيعات", IsScreenAccess = false },
        new() { Code = Permissions.SalesReturns.ReturnWithoutInvoice, Name = "مرتجع بدون فاتورة أصلية", Category = "المبيعات", Description = "تسجيل مرتجع مبيعات غير مربوط بفاتورة أصلية (تجاوز القيد)", IsScreenAccess = false },

        // العملاء والحسابات
        new() { Code = Permissions.Customers.View, Name = "دخول العملاء والحسابات", Category = "العملاء", Description = "عرض قائمة العملاء وأرصدتهم وكشوفات الحساب", IsScreenAccess = true },
        new() { Code = Permissions.Customers.Create, Name = "إضافة عميل جديد", Category = "العملاء", Description = "تسجيل بيانات عميل جديد في النظام", IsScreenAccess = false },
        new() { Code = Permissions.Customers.Edit, Name = "تعديل بيانات عميل", Category = "العملاء", Description = "تعديل بيانات ورقم هاتف وسقف دين العميل", IsScreenAccess = false },
        new() { Code = Permissions.Customers.Delete, Name = "حذف عميل", Category = "العملاء", Description = "حذف ملف العميل", IsScreenAccess = false },

        // فواتير المشتريات
        new() { Code = Permissions.PurchaseInvoices.View, Name = "دخول فواتير المشتريات", Category = "المشتريات", Description = "عرض فواتير المشتريات المباشرة الواردة", IsScreenAccess = true },
        new() { Code = Permissions.PurchaseInvoices.Create, Name = "تسجيل فاتورة مشتريات", Category = "المشتريات", Description = "إدخال فاتورة مشتريات وتحديث أسعار التكلفة", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseInvoices.Edit, Name = "تعديل فاتورة مشتريات", Category = "المشتريات", Description = "تعديل بيانات وأسعار فاتورة المشتريات", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseInvoices.Delete, Name = "حذف فاتورة مشتريات", Category = "المشتريات", Description = "حذف أو إلغاء فاتورة مشتريات", IsScreenAccess = false },

        // مرتجعات المشتريات
        new() { Code = Permissions.PurchaseReturns.View, Name = "دخول مرتجعات المشتريات", Category = "المشتريات", Description = "عرض قائمة مرتجعات المشتريات للموردين", IsScreenAccess = true },
        new() { Code = Permissions.PurchaseReturns.Create, Name = "إنشاء مرتجع مشتريات", Category = "المشتريات", Description = "تسجيل مرتجع بضاعة للمورد وخصم قيمتها", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseReturns.Delete, Name = "حذف مرتجع مشتريات", Category = "المشتريات", Description = "إلغاء مرتجع مشتريات للمورد", IsScreenAccess = false },

        // أوامر الشراء
        new() { Code = Permissions.PurchaseOrders.View, Name = "دخول أوامر الشراء", Category = "المشتريات", Description = "عرض طلبيات وأوامر الشراء قيد الانتظار", IsScreenAccess = true },
        new() { Code = Permissions.PurchaseOrders.Create, Name = "إنشاء أمر شراء", Category = "المشتريات", Description = "تسجيل طلبية بضاعة جديدة للمورد", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseOrders.Edit, Name = "تعديل أمر شراء", Category = "المشتريات", Description = "تعديل بنود وكميات أمر الشراء", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseOrders.Approve, Name = "اعتماد أوامر الشراء", Category = "المشتريات", Description = "اعتماد وتحويل أمر الشراء إلى فاتورة مستلمة", IsScreenAccess = false },
        new() { Code = Permissions.PurchaseOrders.Delete, Name = "حذف أمر شراء", Category = "المشتريات", Description = "إلغاء وحذف أمر شراء", IsScreenAccess = false },

        // الموردين
        new() { Code = Permissions.Suppliers.View, Name = "دخول الموردين", Category = "الموردين", Description = "عرض قائمة الموردين والحسابات الدائنة", IsScreenAccess = true },
        new() { Code = Permissions.Suppliers.Create, Name = "إضافة مورد جديد", Category = "الموردين", Description = "إضافة مورد جديد للنظام", IsScreenAccess = false },
        new() { Code = Permissions.Suppliers.Edit, Name = "تعديل بيانات مورد", Category = "الموردين", Description = "تعديل بيانات وأرقام هواتف المورد", IsScreenAccess = false },
        new() { Code = Permissions.Suppliers.Delete, Name = "حذف مورد", Category = "الموردين", Description = "حذف مورد من النظام", IsScreenAccess = false },

        // المخازن والمستودعات
        new() { Code = Permissions.Warehouses.View, Name = "دخول المخازن وصالات العرض", Category = "المخازن والمستودعات", Description = "عرض قائمة المخازن وصالات العرض", IsScreenAccess = true },
        new() { Code = Permissions.Warehouses.Create, Name = "إضافة مخزن جديد", Category = "المخازن والمستودعات", Description = "إنشاء مستودع أو صالة عرض جديدة", IsScreenAccess = false },
        new() { Code = Permissions.Warehouses.Edit, Name = "تعديل بيانات مخزن", Category = "المخازن والمستودعات", Description = "تعديل مسميات ومواقع المخازن", IsScreenAccess = false },
        new() { Code = Permissions.Warehouses.Delete, Name = "حذف مخزن", Category = "المخازن والمستودعات", Description = "حذف المخزن من المنظومة", IsScreenAccess = false },

        // إدارة المخزون والأرصدة
        new() { Code = Permissions.Stock.View, Name = "دخول إدارة المخزون والأرصدة", Category = "المخازن والمستودعات", Description = "عرض كميات وحركات الأرصدة في المخازن", IsScreenAccess = true },
        new() { Code = Permissions.Stock.Adjust, Name = "تحديد وتعديل الرصيد الافتتاحي", Category = "المخازن والمستودعات", Description = "إدخال وتعديل الأرصدة الافتتاحية للصنف يدوياً", IsScreenAccess = false },

        // التحويلات المخزنية
        new() { Code = Permissions.StockTransfers.View, Name = "دخول التحويلات المخزنية", Category = "المخازن والمستودعات", Description = "عرض سجل ومناقلات البضائع بين المخازن", IsScreenAccess = true },
        new() { Code = Permissions.StockTransfers.Create, Name = "إنشاء تحويل مخزني", Category = "المخازن والمستودعات", Description = "إصدار إذن تحويل أصناف من مخزن لآخر", IsScreenAccess = false },
        new() { Code = Permissions.StockTransfers.Approve, Name = "اعتماد واستلام التحويل", Category = "المخازن والمستودعات", Description = "اعتماد نقل البضاعة وتأكيد استلامها في المخزن المستهدف", IsScreenAccess = false },
        new() { Code = Permissions.StockTransfers.Delete, Name = "إلغاء تحويل مخزني", Category = "المخازن والمستودعات", Description = "إلغاء إذن تحويل مخزني غير معتمد", IsScreenAccess = false },

        // التسويات الجردية
        new() { Code = Permissions.StockAdjustments.View, Name = "دخول التسويات الجردية", Category = "المخازن والمستودعات", Description = "عرض عمليات الجرد والتسوية المخزنية", IsScreenAccess = true },
        new() { Code = Permissions.StockAdjustments.Create, Name = "إنشاء تسوية جردية", Category = "المخازن والمستودعات", Description = "تسجيل فوارق الجرد (زيادة أو نقص) بالمخازن", IsScreenAccess = false },
        new() { Code = Permissions.StockAdjustments.Approve, Name = "اعتماد وترحيل التسوية", Category = "المخازن والمستودعات", Description = "ترحيل التسوية الجردية وتحديث الرصيد الفعلي فوراً", IsScreenAccess = false },
        new() { Code = Permissions.StockAdjustments.Delete, Name = "إلغاء تسوية جردية", Category = "المخازن والمستودعات", Description = "إلغاء تسوية جردية غير مرحلة", IsScreenAccess = false },

        // الفروع
        new() { Code = Permissions.Branches.View, Name = "دخول إدارة الفروع", Category = "إدارة النظام", Description = "عرض فروع المؤسسة وتفاصيلها", IsScreenAccess = true },
        new() { Code = Permissions.Branches.Create, Name = "إضافة فرع جديد", Category = "إدارة النظام", Description = "إنشاء وتأسيس فرع جديد للمؤسسة", IsScreenAccess = false },
        new() { Code = Permissions.Branches.Edit, Name = "تعديل بيانات فرع", Category = "إدارة النظام", Description = "تعديل بيانات وعنوان وهواتف الفرع", IsScreenAccess = false },
        new() { Code = Permissions.Branches.Delete, Name = "حذف فرع", Category = "إدارة النظام", Description = "حذف الفرع من النظام", IsScreenAccess = false },

        // المستخدمين والصلاحيات
        new() { Code = Permissions.Users.View, Name = "دخول إدارة المستخدمين", Category = "إدارة النظام", Description = "عرض قائمة مستخدمي النظام والحسابات", IsScreenAccess = true },
        new() { Code = Permissions.Users.Create, Name = "إضافة مستخدم جديد", Category = "إدارة النظام", Description = "إنشاء حساب مستخدم جديد وتحديد فرعه ودوره", IsScreenAccess = false },
        new() { Code = Permissions.Users.Edit, Name = "تعديل بيانات المستخدم", Category = "إدارة النظام", Description = "تعديل فرع وبيانات وحالة نشاط المستخدم", IsScreenAccess = false },
        new() { Code = Permissions.Users.Delete, Name = "حذف حساب مستخدم", Category = "إدارة النظام", Description = "حذف مستخدم نهائياً من النظام", IsScreenAccess = false },
        new() { Code = Permissions.Users.ResetPassword, Name = "إعادة تعيين كلمة المرور", Category = "إدارة النظام", Description = "تغيير كلمة المرور لحساب أي مستخدم", IsScreenAccess = false },
        new() { Code = Permissions.Roles.View, Name = "عرض الأدوار ومصفوفة الصلاحيات", Category = "إدارة النظام", Description = "استعراض الأدوار الوظيفية والصلاحيات المنسوبة لها", IsScreenAccess = false },
        new() { Code = Permissions.Roles.Manage, Name = "تعديل وحفظ مصفوفة الصلاحيات", Category = "إدارة النظام", Description = "تحديد ومنح وحجب الأذونات عن الأدوار والمستخدمين", IsScreenAccess = false },

        // ملف المنشأة
        new() { Code = Permissions.Tenants.View, Name = "دخول الملف الشخصي للمؤسسة", Category = "إدارة النظام", Description = "عرض بيانات المنشأة والاشتراك", IsScreenAccess = true },
        new() { Code = Permissions.Tenants.Edit, Name = "تعديل بيانات المؤسسة", Category = "إدارة النظام", Description = "تعديل اسم المنشأة والشعار والبيانات الأساسية", IsScreenAccess = false },

        // الخزائن والبنوك
        new() { Code = Permissions.Safes.View, Name = "دخول الخزائن والبنوك", Category = "الحسابات والمالية", Description = "عرض قائمة الخزائن والحسابات المصرفية وأرصدتها", IsScreenAccess = true },
        new() { Code = Permissions.Safes.Create, Name = "إضافة خزينة / حساب بنكي", Category = "الحسابات والمالية", Description = "إنشاء خزينة أو حساب مصرفي جديد وتحديد الرصيد الافتتاحي", IsScreenAccess = false },
        new() { Code = Permissions.Safes.Edit, Name = "تعديل بيانات خزينة", Category = "الحسابات والمالية", Description = "تعديل بيانات الخزينة أو الحساب البنكي", IsScreenAccess = false },
        new() { Code = Permissions.Safes.Delete, Name = "حذف خزينة", Category = "الحسابات والمالية", Description = "حذف الخزينة بشرط خلوها من المعاملات", IsScreenAccess = false },
        new() { Code = Permissions.Safes.Transfer, Name = "تحويل أموال بين الخزائن", Category = "الحسابات والمالية", Description = "إجراء مناقلات وتحويلات مالية بين الخزائن والبنوك", IsScreenAccess = false },
        new() { Code = Permissions.Safes.Transactions, Name = "حركات الخزينة اليدوية", Category = "الحسابات والمالية", Description = "تسجيل إيداعات وسحوبات يدوية وعرض كشف حركات الخزينة", IsScreenAccess = false },
        new() { Code = Permissions.FinancialReconciliation.View, Name = "تدقيق التسوية المالية", Category = "الحسابات والمالية", Description = "فحص اتساق المستندات ودفاتر العملاء والموردين وأرصدة الخزائن", IsScreenAccess = false },

        // تصنيفات المصروفات
        new() { Code = Permissions.ExpenseCategories.View, Name = "دخول تصنيفات المصروفات", Category = "الحسابات والمالية", Description = "عرض قائمة تصنيفات بنود المصروفات", IsScreenAccess = true },
        new() { Code = Permissions.ExpenseCategories.Create, Name = "إضافة تصنيف مصروفات", Category = "الحسابات والمالية", Description = "إضافة تصنيف جديد للمصروفات التشغيلية", IsScreenAccess = false },
        new() { Code = Permissions.ExpenseCategories.Edit, Name = "تعديل تصنيف مصروفات", Category = "الحسابات والمالية", Description = "تعديل مسمى وحالة نشاط تصنيف المصروفات", IsScreenAccess = false },
        new() { Code = Permissions.ExpenseCategories.Delete, Name = "حذف تصنيف مصروفات", Category = "الحسابات والمالية", Description = "حذف تصنيف المصروفات غير المرتبط بسندات", IsScreenAccess = false },

        // سندات المصروفات
        new() { Code = Permissions.Expenses.View, Name = "دخول سندات المصروفات", Category = "الحسابات والمالية", Description = "عرض سجل وسندات المصروفات التشغيلية والبحث فيها", IsScreenAccess = true },
        new() { Code = Permissions.Expenses.Create, Name = "إنشاء سند مصروف", Category = "الحسابات والمالية", Description = "تحرير سند صرف مصروف جديد والخصم من الخزينة", IsScreenAccess = false },
        new() { Code = Permissions.Expenses.Edit, Name = "تعديل سند مصروف", Category = "الحسابات والمالية", Description = "تعديل البيانات الوصفية لسند المصروف", IsScreenAccess = false },
        new() { Code = Permissions.Expenses.Delete, Name = "إلغاء وحذف سند مصروف", Category = "الحسابات والمالية", Description = "إلغاء سند المصروف واسترداد قيمته للخزينة", IsScreenAccess = false },
    };

    /// <summary>
    /// استرجاع قائمة بجميع رموز وأكواد الصلاحيات النصية المعرفة في النظام.
    /// </summary>
    /// <returns>قائمة سلاسل نصية بكافة أكواد الصلاحيات</returns>
    // دالة استرجاع قائمة بكافة أكواد الصلاحيات المعرفة في النظام
    public static List<string> GetAllPermissionCodes() =>
        // تحويل قائمة الصلاحيات إلى قائمة سلاسل نصية للأكواد فقط
        All.Select(p => p.Code).ToList();
}
