namespace RetalSystemAPI.Desktop.Constants;

/// <summary>
/// ثوابت الصلاحيات المستخدمة في تطبيق سطح المكتب للتحقق والتحكم في ظهور العناصر.
/// </summary>
public static class Permissions
{
    public static class Dashboard
    {
        public const string View = "Permissions.Dashboard.View";
    }

    public static class Pos
    {
        public const string Access = "Permissions.Pos.Access";
    }

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
        public const string ExportImport = "Permissions.Products.ExportImport";
    }

    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Edit = "Permissions.Categories.Edit";
        public const string Delete = "Permissions.Categories.Delete";
    }

    public static class Units
    {
        public const string View = "Permissions.Units.View";
        public const string Create = "Permissions.Units.Create";
        public const string Edit = "Permissions.Units.Edit";
        public const string Delete = "Permissions.Units.Delete";
    }

    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Create = "Permissions.Customers.Create";
        public const string Edit = "Permissions.Customers.Edit";
        public const string Delete = "Permissions.Customers.Delete";
    }

    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    public static class SalesInvoices
    {
        public const string View = "Permissions.SalesInvoices.View";
        public const string Create = "Permissions.SalesInvoices.Create";
        public const string Edit = "Permissions.SalesInvoices.Edit";
        public const string Delete = "Permissions.SalesInvoices.Delete";
    }

    public static class SalesReturns
    {
        public const string View = "Permissions.SalesReturns.View";
        public const string Create = "Permissions.SalesReturns.Create";
        public const string Delete = "Permissions.SalesReturns.Delete";
        public const string ReturnWithoutInvoice = "Permissions.SalesReturns.ReturnWithoutInvoice";
    }

    public static class PurchaseInvoices
    {
        public const string View = "Permissions.PurchaseInvoices.View";
        public const string Create = "Permissions.PurchaseInvoices.Create";
        public const string Edit = "Permissions.PurchaseInvoices.Edit";
        public const string Delete = "Permissions.PurchaseInvoices.Delete";
    }

    public static class PurchaseReturns
    {
        public const string View = "Permissions.PurchaseReturns.View";
        public const string Create = "Permissions.PurchaseReturns.Create";
        public const string Delete = "Permissions.PurchaseReturns.Delete";
    }

    public static class PurchaseOrders
    {
        public const string View = "Permissions.PurchaseOrders.View";
        public const string Create = "Permissions.PurchaseOrders.Create";
        public const string Edit = "Permissions.PurchaseOrders.Edit";
        public const string Approve = "Permissions.PurchaseOrders.Approve";
        public const string Delete = "Permissions.PurchaseOrders.Delete";
    }

    public static class Warehouses
    {
        public const string View = "Permissions.Warehouses.View";
        public const string Create = "Permissions.Warehouses.Create";
        public const string Edit = "Permissions.Warehouses.Edit";
        public const string Delete = "Permissions.Warehouses.Delete";
    }

    public static class Stock
    {
        public const string View = "Permissions.Stock.View";
        public const string Adjust = "Permissions.Stock.Adjust";
    }

    public static class StockTransfers
    {
        public const string View = "Permissions.StockTransfers.View";
        public const string Create = "Permissions.StockTransfers.Create";
        public const string Approve = "Permissions.StockTransfers.Approve";
        public const string Delete = "Permissions.StockTransfers.Delete";
    }

    public static class StockAdjustments
    {
        public const string View = "Permissions.StockAdjustments.View";
        public const string Create = "Permissions.StockAdjustments.Create";
        public const string Approve = "Permissions.StockAdjustments.Approve";
        public const string Delete = "Permissions.StockAdjustments.Delete";
    }

    public static class Branches
    {
        public const string View = "Permissions.Branches.View";
        public const string Create = "Permissions.Branches.Create";
        public const string Edit = "Permissions.Branches.Edit";
        public const string Delete = "Permissions.Branches.Delete";
    }

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

    public static class Tenants
    {
        public const string View = "Permissions.Tenants.View";
        public const string Edit = "Permissions.Tenants.Edit";
    }
}
