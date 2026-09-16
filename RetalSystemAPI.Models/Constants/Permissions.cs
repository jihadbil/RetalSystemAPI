using System;
using System.Collections.Generic;
using System.Linq;

namespace RetalSystemAPI.Models.Constants;

/// <summary>
/// ثوابت ومصفوفة الصلاحيات والأذونات لكافة شاشات وعمليات نظام رتال.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "Permission";

    // ── لوحة التحكم ──
    public static class Dashboard
    {
        public const string View = "Permissions.Dashboard.View";
    }

    // ── نقطة البيع POS ──
    public static class Pos
    {
        public const string View = "Permissions.Pos.View";
        public const string CreateSale = "Permissions.Pos.CreateSale";
        public const string ApplyDiscount = "Permissions.Pos.ApplyDiscount";
        public const string Reprint = "Permissions.Pos.Reprint";
    }

    // ── الأصناف والمنتجات ──
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
        public const string ExportImport = "Permissions.Products.ExportImport";
    }

    // ── التصنيفات ──
    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Edit = "Permissions.Categories.Edit";
        public const string Delete = "Permissions.Categories.Delete";
    }

    // ── وحدات القياس ──
    public static class Units
    {
        public const string View = "Permissions.Units.View";
        public const string Create = "Permissions.Units.Create";
        public const string Edit = "Permissions.Units.Edit";
        public const string Delete = "Permissions.Units.Delete";
    }

    // ── فواتير المبيعات ──
    public static class SalesInvoices
    {
        public const string View = "Permissions.SalesInvoices.View";
        public const string Create = "Permissions.SalesInvoices.Create";
        public const string Edit = "Permissions.SalesInvoices.Edit";
        public const string Delete = "Permissions.SalesInvoices.Delete";
        public const string Print = "Permissions.SalesInvoices.Print";
    }

    // ── مرتجعات المبيعات ──
    public static class SalesReturns
    {
        public const string View = "Permissions.SalesReturns.View";
        public const string Create = "Permissions.SalesReturns.Create";
        public const string Delete = "Permissions.SalesReturns.Delete";
        public const string ReturnWithoutInvoice = "Permissions.SalesReturns.ReturnWithoutInvoice";
    }

    // ── العملاء والحسابات ──
    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Create = "Permissions.Customers.Create";
        public const string Edit = "Permissions.Customers.Edit";
        public const string Delete = "Permissions.Customers.Delete";
    }

    // ── فواتير المشتريات ──
    public static class PurchaseInvoices
    {
        public const string View = "Permissions.PurchaseInvoices.View";
        public const string Create = "Permissions.PurchaseInvoices.Create";
        public const string Edit = "Permissions.PurchaseInvoices.Edit";
        public const string Delete = "Permissions.PurchaseInvoices.Delete";
    }

    // ── مرتجعات المشتريات ──
    public static class PurchaseReturns
    {
        public const string View = "Permissions.PurchaseReturns.View";
        public const string Create = "Permissions.PurchaseReturns.Create";
        public const string Delete = "Permissions.PurchaseReturns.Delete";
    }

    // ── أوامر وطلبيات الشراء ──
    public static class PurchaseOrders
    {
        public const string View = "Permissions.PurchaseOrders.View";
        public const string Create = "Permissions.PurchaseOrders.Create";
        public const string Edit = "Permissions.PurchaseOrders.Edit";
        public const string Approve = "Permissions.PurchaseOrders.Approve";
        public const string Delete = "Permissions.PurchaseOrders.Delete";
    }

    // ── الموردين ──
    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    // ── المخازن والمستودعات ──
    public static class Warehouses
    {
        public const string View = "Permissions.Warehouses.View";
        public const string Create = "Permissions.Warehouses.Create";
        public const string Edit = "Permissions.Warehouses.Edit";
        public const string Delete = "Permissions.Warehouses.Delete";
    }

    // ── إدارة المخزون والأرصدة ──
    public static class Stock
    {
        public const string View = "Permissions.Stock.View";
        public const string Adjust = "Permissions.Stock.Adjust";
    }

    // ── التحويلات المخزنية ──
    public static class StockTransfers
    {
        public const string View = "Permissions.StockTransfers.View";
        public const string Create = "Permissions.StockTransfers.Create";
        public const string Approve = "Permissions.StockTransfers.Approve";
        public const string Delete = "Permissions.StockTransfers.Delete";
    }

    // ── التسويات الجردية ──
    public static class StockAdjustments
    {
        public const string View = "Permissions.StockAdjustments.View";
        public const string Create = "Permissions.StockAdjustments.Create";
        public const string Approve = "Permissions.StockAdjustments.Approve";
        public const string Delete = "Permissions.StockAdjustments.Delete";
    }

    // ── الفروع ──
    public static class Branches
    {
        public const string View = "Permissions.Branches.View";
        public const string Create = "Permissions.Branches.Create";
        public const string Edit = "Permissions.Branches.Edit";
        public const string Delete = "Permissions.Branches.Delete";
    }

    // ── المستخدمين والأدوار ──
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
        public const string ResetPassword = "Permissions.Users.ResetPassword";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Manage = "Permissions.Roles.Manage";
    }

    // ── ملف المنشأة ──
    public static class Tenants
    {
        public const string View = "Permissions.Tenants.View";
        public const string Edit = "Permissions.Tenants.Edit";
    }
}

/// <summary>
/// وصف تفصيلي للصلاحية للاستخدام في مصفوفة الصلاحيات بالواجهات.
/// </summary>
public class PermissionDefinition
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsScreenAccess { get; set; }
}

/// <summary>
/// سجل كامل بجميع الصلاحيات المعرفة في النظام مع أسمائها وتصنيفاتها العربية.
/// </summary>
public static class AppPermissions
{
    public static readonly List<PermissionDefinition> All = new()
    {
        // لوحة التحكم
        new() { Code = Permissions.Dashboard.View, Name = "دخول لوحة التحكم", Category = "لوحة التحكم", Description = "عرض الإحصائيات العامة والمؤشرات البيانية", IsScreenAccess = true },

        // نقطة البيع
        new() { Code = Permissions.Pos.View, Name = "دخول نقطة البيع", Category = "نقطة البيع (POS)", Description = "الدخول لشاشة الكاشير ونقطة البيع السريعة", IsScreenAccess = true },
        new() { Code = Permissions.Pos.CreateSale, Name = "إتمام المبيعات", Category = "نقطة البيع (POS)", Description = "إتمام عمليات البيع وطباعة الإيصال", IsScreenAccess = false },
        new() { Code = Permissions.Pos.ApplyDiscount, Name = "منح خصم كاشير", Category = "نقطة البيع (POS)", Description = "تطبيق خصم إضافي على الفاتورة في نقطة البيع", IsScreenAccess = false },
        new() { Code = Permissions.Pos.Reprint, Name = "إعادة طباعة الإيصالات", Category = "نقطة البيع (POS)", Description = "إعادة طباعة فواتير وإيصالات سابقة", IsScreenAccess = false },

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
    };

    public static List<string> GetAllPermissionCodes() => All.Select(p => p.Code).ToList();
}
